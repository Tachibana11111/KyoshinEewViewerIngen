using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using DmdataSharp.Redundancy;
using KyoshinEewViewer.Core;
using KyoshinEewViewer.Core.Models;
using KyoshinEewViewer.Series;
using R3;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KyoshinEewViewer.Services.TelegramPublishers.Dmdata;

public partial class DmdataSettingPage : LocalizedObservableObject, ISettingPage
{
	public bool IsVisible => true;

	public string? Icon => null;

	public string Title => Loc.Instance["DM-D.S.S"];

	public Control DisplayControl => new DmdataPage() { DataContext = this };

	public ISettingPage[] SubPages => [];

	private ILogger Logger { get; }
	public DmdataRedundantTelegramPublisher DmdataRedundantTelegramPublisher { get; }
	public KyoshinEewViewerConfiguration Config { get; }


	[ObservableProperty]
	public partial string DmdataStatusString { get; set; } = Loc.Get("Dmdata.Status.NotAuth");

	[ObservableProperty]
	public partial CancellationTokenSource? AuthorizeCancellationTokenSource { get; set; } = null;


	public DmdataSettingPage(
		ILogger<DmdataSettingPage> logger,
		DmdataRedundantTelegramPublisher dmdataTelegramPublisher,
		KyoshinEewViewerConfiguration config)
	{
		Logger = logger;
		Config = config;
		DmdataRedundantTelegramPublisher = dmdataTelegramPublisher;

		UpdateDmdataStatus();
		
		// WebSocket接続状態を監視
		DmdataRedundantTelegramPublisher.ObservePropertyChanged(x => x.RedundancyStatus)
			.Subscribe(status =>
			{
				IsWebSocketConnected = status == RedundancyStatus.FullyConnected || 
				                      status == RedundancyStatus.PartiallyConnected;
			});
	}

	public void CancelAuthorizeDmdata()
		=> AuthorizeCancellationTokenSource?.Cancel();

	public async Task AuthorizeDmdata()
	{
		if (AuthorizeCancellationTokenSource != null)
		{
			AuthorizeCancellationTokenSource.Cancel();
			return;
		}
		if (!string.IsNullOrEmpty(Config.Dmdata.RefreshToken))
			return;

		DmdataStatusString = Loc.Get("Dmdata.Status.Authenticating");

		AuthorizeCancellationTokenSource = new CancellationTokenSource();
		try
		{
			await DmdataRedundantTelegramPublisher.AuthorizeAsync(AuthorizeCancellationTokenSource.Token);
			DmdataStatusString = Loc.Get("Dmdata.Status.AuthSuccess");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "認可フロー中に例外が発生しました");
		}
		finally
		{
			AuthorizeCancellationTokenSource = null;
		}

		UpdateDmdataStatus();
	}

	public async Task UnauthorizeDmdata()
	{
		if (string.IsNullOrEmpty(Config.Dmdata.RefreshToken))
			return;

		DmdataStatusString = Loc.Get("Dmdata.Status.Unlinking");
		try
		{
			await DmdataRedundantTelegramPublisher.UnauthorizeAsync();
		}
		catch
		{
			DmdataStatusString = Loc.Get("Dmdata.Status.RevokeFailed");
		}

		UpdateDmdataStatus();
	}

	/// <summary>
	/// WebSocket接続を即座に再接続します
	/// </summary>
	public async Task ReconnectImmediately()
	{
		try
		{
			await DmdataRedundantTelegramPublisher.ReconnectImmediatelyAsync();
			DmdataStatusString = Loc.Get("Dmdata.Status.ReconnectStarted");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "即時再接続に失敗しました");
			DmdataStatusString = Loc.Get("Dmdata.Status.ReconnectFailed");
		}
	}

	/// <summary>
	/// WebSocketが接続されているかどうかを取得します
	/// </summary>
	[ObservableProperty]
	public partial bool IsWebSocketConnected { get; private set; }

	private void UpdateDmdataStatus()
	{
		if (!string.IsNullOrWhiteSpace(Config.Dmdata.OAuthClientSecret))
		{
			DmdataStatusString = Loc.Get("Dmdata.Status.ClientCredentials");
			return;
		}
		if (string.IsNullOrEmpty(Config.Dmdata.RefreshToken))
		{
			DmdataStatusString = Loc.Get("Dmdata.Status.NotAuth");
			return;
		}
		DmdataStatusString = Loc.Get("Dmdata.Status.Authenticated");
	}
}
