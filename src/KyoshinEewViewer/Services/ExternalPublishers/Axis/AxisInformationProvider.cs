using CommunityToolkit.Mvvm.ComponentModel;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Core;
using KyoshinEewViewer.Core.Models;
using KyoshinEewViewer.Services.ExternalPublishers.Axis.ApiModels;
using R3;
using System;
using System.Reactive.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace KyoshinEewViewer.Services.ExternalPublishers.Axis;

public partial class AxisInformationProvider : ObservableObject
{
	private ILogger Logger { get; }
	private KyoshinEewViewerConfiguration Config { get; }

	private AxisApiClient ApiClient { get; } = new AxisApiClient();
	private AxisWebSocketConnection WebSocketConnection { get; }

	private Timer JwtRefreshTimer { get; }

	private double BackoffTime { get; set; } = 1;
	private Timer ReconnectTimer { get; }

	[ObservableProperty]
	public partial bool IsConnected { get; private set; }

	/// <summary>
	/// 現時点でこの機能が求められているかどうか<br/>
	/// 求められていない場合接続しない
	/// </summary>
	private bool IsFeatureRequired { get; set; }

	[ObservableProperty]
	public partial AxisJwtPayload? CurrentPayload { get; private set; }

	[ObservableProperty]
	public partial string? PayloadErrorMessage { get; private set; }

	[ObservableProperty]
	public partial string? CurrentStatus { get; private set; } = Loc.Get("Axis.Status.Idle");

	public event Action<AxisWebSocketMessage>? MessageReceived;

	public AxisInformationProvider(KyoshinEewViewerConfiguration config, ILogger<AxisInformationProvider> logger)
	{
		Logger = logger;
		Config = config;

		JwtRefreshTimer = new Timer(_ => CheckAndRefreshJwt(), null, Timeout.Infinite, Timeout.Infinite);
		ReconnectTimer = new Timer(_ => TryReconnect(), null, Timeout.Infinite, Timeout.Infinite);

		WebSocketConnection = new AxisWebSocketConnection(ApiClient);
		WebSocketConnection.Connected += () =>
		{
			IsConnected = true;
			CurrentStatus = Loc.Get("Axis.Status.Connected");
			BackoffTime = 1;
		};
		WebSocketConnection.PingCompleted += time =>
		{
			CurrentStatus = Loc.Get("Axis.Status.ConnectedRtt", time.TotalMilliseconds);
		};
		WebSocketConnection.Error += (message, canRetry) =>
		{
			CurrentStatus = message;
			if (!canRetry)
				Config.Axis.Enable = false;

		};
		WebSocketConnection.Disconnected += () =>
		{
			IsConnected = false;
			CurrentStatus = Loc.Get("Axis.Status.Disconnected");
			if (Config.Axis.Enable)
			{
				CurrentStatus = Loc.Get("Axis.Status.DisconnectedRetry", BackoffTime);
				ReconnectTimer.Change(TimeSpan.FromSeconds(BackoffTime), Timeout.InfiniteTimeSpan);
			}
		};
		WebSocketConnection.MessageReceived += message =>
		{
			Logger.LogDebug("受信しました: {Channel}", message.Channel);
			MessageReceived?.Invoke(message);
		};

		Config.Axis.ObservePropertyChanged(x => x.Jwt).Subscribe(jwt =>
		{
			if (string.IsNullOrWhiteSpace(jwt))
			{
				Config.Axis.Enable = false;
				ApiClient.Jwt = null;
				CurrentPayload = null;
				PayloadErrorMessage = Loc.Get("Axis.NoToken");
				return;
			}
			try
			{
				CurrentPayload = AxisJwtPayload.Parse(jwt);
				ApiClient.Jwt = jwt;
				if (DateTimeOffset.FromUnixTimeSeconds(CurrentPayload.Exp) < DateTimeOffset.UtcNow)
				{
					Config.Axis.Enable = false;
					PayloadErrorMessage = Loc.Get("Axis.TokenExpired");
				}
				else
					PayloadErrorMessage = null;
			}
			catch (Exception ex)
			{
				Config.Axis.Enable = false;
				CurrentPayload = null;
				ApiClient.Jwt = null;
				PayloadErrorMessage = ex.Message;
			}
		});

		Config.Axis.ObservePropertyChanged(x => x.Enable).Debounce(TimeSpan.FromSeconds(1)).Subscribe(enabled =>
		{
			if (!IsFeatureRequired)
				return;

			if (enabled)
			{
				Connect();
				return;
			}
			// 無効化
			Disconnect();
		});
	}

	public void Initialize()
	{
		if (IsFeatureRequired)
			return;

		IsFeatureRequired = true;
		if (Config.Axis.Enable)
			Connect();
	}

	private Random Random { get; } = new();
	private void TryReconnect()
	{
		if (!Config.Axis.Enable)
			return;
		// 1.5倍～2.5倍の間で最大1時間までバックオフタイムを伸ばしていく
		BackoffTime = Math.Min(BackoffTime * (1.5 + Random.NextDouble()), 3600);
		Connect();
	}

	private bool IsConnecting { get; set; }
	private async void Connect()
	{
		if (IsConnecting || WebSocketConnection.IsConnected)
			return;
		try
		{
			IsConnecting = true;
			// 有効化
			CurrentStatus = Loc.Get("Axis.Status.Connecting");
			await WebSocketConnection.ConnectAsync();
			JwtRefreshTimer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromDays(1));
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "接続に失敗しました。");
			CurrentStatus = Loc.Get("Axis.Status.ConnectFailed");
			ReconnectTimer.Change(TimeSpan.FromSeconds(BackoffTime), Timeout.InfiniteTimeSpan);
			IsConnected = false;
		}
		IsConnecting = false;
	}

	private async void Disconnect()
	{
		try
		{
			CurrentStatus = Loc.Get("Axis.Status.Disconnecting");
			JwtRefreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
			ReconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
			await WebSocketConnection.DisconnectAsync();
			IsConnected = false;
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "切断に失敗しました。");
		}
		CurrentStatus = Loc.Get("Axis.Status.DisconnectDone");
	}

	public async void CheckAndRefreshJwt()
	{
		if (!Config.Axis.Enable || string.IsNullOrWhiteSpace(Config.Axis.Jwt))
			return;

		try
		{
			var payload = AxisJwtPayload.Parse(Config.Axis.Jwt);
			// まだまだ余裕がある
			if (payload.ExpDateTime - DateTimeOffset.UtcNow > TimeSpan.FromDays(5))
				return;
			// 間に合わなかった
			if (payload.ExpDateTime <= DateTimeOffset.UtcNow)
			{
				PayloadErrorMessage = Loc.Get("Axis.TokenExpired");
				Config.Axis.Enable = false;
				return;
			}

			Logger.LogInformation("トークンの有効期限が近づいているため、更新します。");
			// トークンを更新
			await ApiClient.RefreshJwt();
			Config.Axis.Jwt = ApiClient.Jwt ?? throw new Exception("新しいトークンが取得できませんでした。");

			Logger.LogInformation("トークンを更新しました。");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "トークンの更新に失敗しました。");
		}
	}
}
