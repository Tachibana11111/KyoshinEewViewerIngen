using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using KyoshinEewViewer.Core;
using KyoshinEewViewer.Core.Models;
using KyoshinEewViewer.Core.Models.Events;
using KyoshinEewViewer.DCReportParser;
using KyoshinEewViewer.Series;
using KyoshinEewViewer.Series.Qzss.Events;
using KyoshinEewViewer.Services;
using KyoshinEewViewer.Services.ExternalPublishers.Axis;
using KyoshinEewViewer.Services.Feedback;
using KyoshinEewViewer.Services.TelegramPublishers.Dmdata;
using KyoshinEewViewer.Services.Voicevox;
using KyoshinEewViewer.Services.Workflows;
using KyoshinEewViewer.Services.Workflows.BuiltinActions;
using KyoshinEewViewer.Views.SettingPages;
using KyoshinMonitorLib;
using R3;
using Scriban.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace KyoshinEewViewer.ViewModels;

public partial class SettingWindowViewModel : ViewModelBase
{
	public static LocMap<KyoshinEventLevel> KyoshinEventLevelNames { get; } = new()
	{
		{ KyoshinEventLevel.Weaker, "EventLevel.Weaker" },
		{ KyoshinEventLevel.Weak, "EventLevel.Weak" },
		{ KyoshinEventLevel.Medium, "EventLevel.Medium" },
		{ KyoshinEventLevel.Strong, "EventLevel.Strong" },
		{ KyoshinEventLevel.Stronger, "EventLevel.Stronger" },
		{ KyoshinEventLevel.Disabled, "EventLevel.Disabled" },
	};
	/// <summary>
	/// 地点予測を展開表示する震度のしきい値の選択肢
	/// </summary>
	public static LocMap<JmaIntensity> PointForecastExpandIntensityNames { get; } = new()
	{
		{ JmaIntensity.Int1, "ShindoThreshold.Int1" },
		{ JmaIntensity.Int2, "ShindoThreshold.Int2" },
		{ JmaIntensity.Int3, "ShindoThreshold.Int3" },
		{ JmaIntensity.Int4, "ShindoThreshold.Int4" },
		{ JmaIntensity.Int5Lower, "ShindoThreshold.Int5Lower" },
		{ JmaIntensity.Int5Upper, "ShindoThreshold.Int5Upper" },
		{ JmaIntensity.Int6Lower, "ShindoThreshold.Int6Lower" },
		{ JmaIntensity.Int6Upper, "ShindoThreshold.Int6Upper" },
		{ JmaIntensity.Int7, "ShindoName.Int7" },
	};
	public static LocMap<KyoshinEewViewerConfiguration.KyoshinMonitorConfig.Mode> KyoshinMonitorModeNames { get; } = new()
	{
		{ KyoshinEewViewerConfiguration.KyoshinMonitorConfig.Mode.None, "MonitorMode.None" },
		{ KyoshinEewViewerConfiguration.KyoshinMonitorConfig.Mode.Kmoni, "Series.KyoshinMonitor" },
		{ KyoshinEewViewerConfiguration.KyoshinMonitorConfig.Mode.Lmoni, "MonitorMode.Lmoni" },
	};
	public static LocMap<ShakeDetectionDisplayMode> ShakeDetectionDisplayModeNames { get; } = new()
	{
		{ ShakeDetectionDisplayMode.None, "ShakeDisplay.None" },
		{ ShakeDetectionDisplayMode.Grid, "ShakeDisplay.Grid" },
		{ ShakeDetectionDisplayMode.ConvexHull, "ShakeDisplay.ConvexHull" },
	};
	public static LocMap<ShakeDetectionAnimationMode> ShakeDetectionAnimationModeNames { get; } = new()
	{
		{ ShakeDetectionAnimationMode.None, "ShakeAnimation.None" },
		{ ShakeDetectionAnimationMode.Blink, "ShakeAnimation.Blink" },
		{ ShakeDetectionAnimationMode.Pulse, "ShakeAnimation.Pulse" },
	};

	public KyoshinEewViewerConfiguration Config { get; }
	public SeriesController SeriesController { get; }
	public SoundPlayerService SoundPlayerService { get; }
	public UpdateCheckService UpdateCheckService { get; }
	public WorkflowService WorkflowService { get; }
	public VoicevoxService VoicevoxService { get; }
	public ISubWindowsService? SubWindowService { get; }

	private ILogger Logger { get; }

	private ISettingPage _selectedSettingPage;
	public ISettingPage SelectedSettingPage
	{
		get => _selectedSettingPage;
		set {
			var oldValue = _selectedSettingPage;
			SetProperty(ref _selectedSettingPage, value);
			if (value is BasicSettingPage && oldValue is not BasicSettingPage)
			{
				SelectedSettingPage = oldValue;
				return;
			}
		}
	}
	private BasicSettingPage<UpdatePage> UpdatePage { get; }
	public ISettingPage[] SettingPages { get; }

	public SettingWindowViewModel(
		KyoshinEewViewerConfiguration config,
		SeriesController seriesController,
		UpdateCheckService updateCheckService,
		SoundPlayerService soundPlayerService,
		WorkflowService workflowService,
		VoicevoxService voicevoxService,
		ILogger<SettingWindowViewModel> logger,
		DmdataSettingPage dmdataPage,
		AxisSettingPage axisPage,
		FeedbackSettingPage feedbackPage,
		ISubWindowsService? subWindowService = null)
	{
		Config = config;
		SeriesController = seriesController ?? throw new ArgumentNullException(nameof(seriesController));
		UpdateCheckService = updateCheckService;
		SoundPlayerService = soundPlayerService;
		WorkflowService = workflowService;
		VoicevoxService = voicevoxService;
		SubWindowService = subWindowService;

		Logger = logger;

		Series = SeriesController.AllSeries.Select(s => new SeriesViewModel(s, Config)).ToArray();

		RegisteredSounds = SoundPlayerService.RegisteredSounds.Select(s => new SoundConfigViewModel(s.Key, s.Value)).ToArray();
		OpenSoundFile = new AsyncRelayCommand<KyoshinEewViewerConfiguration.SoundConfig>(async config =>
		{
			if (KyoshinEewViewerApp.TopLevelControl == null)
				return;
			var files = await KyoshinEewViewerApp.TopLevelControl.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
			{
				Title = Loc.Get("Dialog.OpenAudioFile"),
				FileTypeFilter = new List<FilePickerFileType>()
				{
					FilePickerFileTypes.All,
				},
				AllowMultiple = false,
			});
			if (files is not { Count: > 0 } || files[0].TryGetLocalPath() is not { } localPath)
				return;

			config.FilePath = localPath;
			return;
		});

		ResetMapPosition = new RelayCommand(() =>
		{
			Config.Map.Location1 = new(45.619358f, 145.77399f);
			Config.Map.Location2 = new(29.997368f, 128.22534f);
		});

		updateCheckService.Updated += a =>
		{
			VersionInfos = a;
			if ((a?.Length ?? 0) > 0 && UpdatePage != null)
			{
				UpdatePage.IsVisible = true;
				SelectedSettingPage = UpdatePage;
			}
		};
		VersionInfos = updateCheckService.AvailableUpdateVersions;

		updateCheckService.ObservePropertyChanged(x => x.IsUpdateIndeterminate).Subscribe(x => IsUpdateIndeterminate = x);
		updateCheckService.ObservePropertyChanged(x => x.UpdateProgress).Subscribe(x => UpdateProgress = x);
		updateCheckService.ObservePropertyChanged(x => x.UpdateProgressMax).Subscribe(x => UpdateProgressMax = x);
		updateCheckService.ObservePropertyChanged(x => x.UpdateState).Subscribe(x => UpdateState = x);

		SelectedWorkflow = WorkflowService.Workflows.FirstOrDefault();

		VoicevoxService.ObservePropertyChanged(x => x.Speakers)
			.Subscribe(s => VoicevoxSpeakerName = s.SelectMany(t => t switch
			{
				MultiStyleSpeaker ms => ms.Styles,
				SingleStyleSpeaker ss => [ss],
				_ => [],
			}).FirstOrDefault(s => s.SpeakerId == config.Voicevox.SpeakerId)?.Name ?? Loc.Get("Common.Unknown"));


		UpdatePage = new BasicSettingPage<UpdatePage>("\xf071", "Setting.Update", []) { IsVisible = false };
		SettingPages = [
			UpdatePage,
			new BasicSettingPage<GeneralPage>("\xf53f", "Setting.General", []),
			new BasicSettingPage<FeaturePage>("\xf085", "Feature.Title", []),
			new BasicSettingPage<NotifyPage>("\xf075", "Notify.Title", []),
			new BasicSettingPage<MultiWindowPage>("\xf2d2", "MultiWindow.Title", []),
			new BasicSettingPage<SoundPage>("\xf028", "Setting.Sound", []),
			new BasicSettingPage<WorkflowPage>("\xe289", "Setting.Workflow", []),
			new BasicSettingPage<VoicevoxPage>("\xf075", "VOICEVOX", []),
			..SeriesController.EnabledSeries.SelectMany(s => s.SettingPages),
			new BasicSettingPage("\xf48b", "Setting.Delivery", [
				dmdataPage,
				axisPage,
			]),
			new BasicSettingPage<MapPage>("\xf5a0", "Setting.Map", []),
			feedbackPage,
			new BasicSettingPage<AboutPage>("\xf129", "Setting.About", []),
			new BasicSettingPage<LicencePage>("\xf2c2", "Setting.License", []),
#if DEBUG
			new BasicSettingPage<DebugMenuPage>("\xf188", "デバッグメニュー", []),
#endif
		];
		_selectedSettingPage = SettingPages[1];
		if ((updateCheckService.AvailableUpdateVersions?.Length ?? 0) > 0)
		{
			UpdatePage.IsVisible = true;
			SelectedSettingPage = UpdatePage;
		}

		if (Design.IsDesignMode)
		{
			IsDebug = true;
			VersionInfos =
			[
				new VersionInfo
				{
					Time = DateTime.Now,
					Message = "test",
					VersionString = "1.1.31.0"
				},
			];
			IsUpdating = true;
			IsUpdateIndeterminate = false;
			UpdateProgressMax = 100;
			UpdateProgress = 50;
			return;
		}
#if DEBUG
		IsDebug = true;
#endif
	}

	public string Title => Loc.Instance["Setting.WindowTitle"];

	[ObservableProperty]
	public partial bool IsDebug { get; set; }

	public List<JmaIntensity> Ints { get; } = [
		JmaIntensity.Unknown,
		JmaIntensity.Int0,
		JmaIntensity.Int1,
		JmaIntensity.Int2,
		JmaIntensity.Int3,
		JmaIntensity.Int4,
		JmaIntensity.Int5Lower,
		JmaIntensity.Int5Upper,
		JmaIntensity.Int6Lower,
		JmaIntensity.Int6Upper,
		JmaIntensity.Int7,
		JmaIntensity.Error,
	];

	public List<LpgmIntensity> LpgmInts { get; } = [
		LpgmIntensity.Unknown,
		LpgmIntensity.LpgmInt0,
		LpgmIntensity.LpgmInt1,
		LpgmIntensity.LpgmInt2,
		LpgmIntensity.LpgmInt3,
		LpgmIntensity.LpgmInt4,
		LpgmIntensity.Error,
	];

	public SeriesViewModel[] Series { get; }

	public bool IsSoundActivated => SoundPlayerService.IsAvailable;
	public SoundConfigViewModel[] RegisteredSounds { get; }

	[ObservableProperty]
	public partial Workflow? SelectedWorkflow { get; set; }

	public void LoadWorkflows()
	{
		WorkflowService.LoadWorkflows();
		SelectedWorkflow = WorkflowService.Workflows.FirstOrDefault(w => w.Id == SelectedWorkflow?.Id)
			?? WorkflowService.Workflows.FirstOrDefault();
	}
	public void AddWorkflow()
	{
		var wf = new Workflow() { Name = Loc.Get("Workflow.NewName"), Trigger = new DummyTrigger() };
		WorkflowService.Workflows.Add(wf);
		SelectedWorkflow = wf;
	}
	public async void RemoveWorkflow(object? parameter)
	{
		if (parameter is not Workflow workflow)
			return;
		var result = await DialogHelper.ShowSettingWindowConfirmationDialogAsync(
			Loc.Get("Dialog.DeleteWorkflow.Title"),
			Loc.Get("Dialog.DeleteWorkflow.Message", workflow.Name));
		
		if (result)
		{
			WorkflowService.Workflows.Remove(workflow);
			SelectedWorkflow = WorkflowService.Workflows.FirstOrDefault();
		}
	}
	public async Task TestRunWorkflow(object? parameter)
	{
		if (parameter is not Workflow workflow)
			return;
		workflow.IsTestRunning = true;
		try
		{
			await workflow.TestRunAsync();
		}
		catch (ScriptRuntimeException ex)
		{
			// ユーザーが記述したテンプレートの問題のため、Sentry に送信しない
			Logger.LogWarning(ex, "ワークフローのテスト実行中にテンプレートのエラーが発生しました");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "ワークフローのテスト実行中に例外が発生しました");
		}
		finally
		{
			workflow.IsTestRunning = false;
		}
	}
	public async Task OpenSoundFileForWorkflow(object? parameter)
	{
		if (parameter is not PlaySoundAction action)
			return;
		if (KyoshinEewViewerApp.TopLevelControl == null)
			return;
		var files = await KyoshinEewViewerApp.TopLevelControl.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
		{
			Title = Loc.Get("Dialog.OpenAudioFile"),
			FileTypeFilter = [FilePickerFileTypes.All],
			AllowMultiple = false,
		});
		if (files is not { Count: > 0 } || files[0].TryGetLocalPath() is not { } localPath)
			return;

		action.FilePath = localPath;
	}
	public void OpenWorkflowPage()
		=> UrlOpener.OpenUrl("https://github.com/ingen084/KyoshinEewViewerIngen/blob/develop/workflow-guide.md");


	[ObservableProperty]
	public partial string VoicevoxSpeakerName { get; set; } = Loc.Get("Voicevox.SpeakersNotLoaded");
	[ObservableProperty]
	public partial bool IsVoicevoxTestPlaying { get; set; }

	public async Task PlayVoicevoxTestSound()
	{
		try
		{
			IsVoicevoxTestPlaying = true;
			await VoicevoxService.PlayTest();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "VOICEVOX のテスト再生中に例外が発生しました");
		}
		finally
		{
			IsVoicevoxTestPlaying = false;
		}
	}
	public Task UpdateVoicevoxSpeakers()
		=> VoicevoxService.GetSpeakers();
	public void UpdateVoicevoxSpeaker(object? parameter)
	{
		if (parameter is not SingleStyleSpeaker ss)
			return;
		Config.Voicevox.SpeakerId = ss.SpeakerId;
		VoicevoxSpeakerName = ss.Name;
	}
	public void ClearVoicevoxCache()
	{
		try
		{
			VoicevoxService.ClearCache();
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "VoiceVoxキャッシュのクリア中に例外が発生しました");
		}
	}

	#region Update

	[ObservableProperty]
	public partial VersionInfo[]? VersionInfos { get; set; }

	[ObservableProperty]
	public partial bool UpdaterEnable { get; set; } = true;

	[ObservableProperty]
	public partial bool IsUpdating { get; set; }

	[ObservableProperty]
	public partial bool IsUpdateIndeterminate { get; set; }

	[ObservableProperty]
	public partial double UpdateProgress { get; set; }

	[ObservableProperty]
	public partial double UpdateProgressMax { get; set; }

	[ObservableProperty]
	public partial string UpdateState { get; set; } = "-";

	public void StartUpdater()
	{
		UpdaterEnable = false;
		IsUpdating = true;
		UpdateCheckService.StartUpdater()
			.ContinueWith(_ => UpdaterEnable = true).ConfigureAwait(false);
	}

	public void ForceStartUpdater()
	{
		UpdaterEnable = false;
		IsUpdating = true;
		UpdateCheckService.StartUpdater(forceUpdate: true)
			.ContinueWith(_ => UpdaterEnable = true).ConfigureAwait(false);
	}
	#endregion

	public async Task ResetMultiWindowPositions()
	{
		var result = await DialogHelper.ShowSettingWindowConfirmationDialogAsync(
			Loc.Get("Dialog.ResetWindows.Title"),
			Loc.Get("Dialog.ResetWindows.Message"));

		if (!result)
			return;

		if (Config.MultiWindow.Enable)
			SubWindowService?.CloseAllSeriesWindows();

		Config.MultiWindow.SeriesWindows.Clear();
	}

	public async Task EditWindowTheme()
	{
		if (SubWindowService == null || KyoshinEewViewerApp.Selector == null)
			return;
		await SubWindowService.ShowDialogWindowThemeEditWindow(KyoshinEewViewerApp.Selector.SelectedWindowTheme);
	}
	public async Task EditIntensityTheme()
	{
		if (SubWindowService == null || KyoshinEewViewerApp.Selector == null)
			return;
		await SubWindowService.ShowDialogIntensityThemeEditWindow(KyoshinEewViewerApp.Selector.SelectedIntensityTheme);
	}

	public bool IsLinux { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
	public bool IsWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
	public bool IsMacOs { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
	// トレイアイコンはデスクトップ (Windows/macOS/Linux) で利用できる
	public bool IsDesktop { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
	public bool IsLogDirectoryCustomizable { get; } = PlatformDirectories.IsLogDirectoryCustomizable;
	public bool IsUseCurrentDirectoryOptionAvailable { get; } = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	public void OpenLogDirectory()
	{
		string logPath;
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			logPath = PlatformDirectories.Logs;
		}
		else if (Config.Logging.UseCurrentDirectory)
		{
			logPath = Path.IsPathFullyQualified(Config.Logging.Directory)
				? Config.Logging.Directory
				: Path.Combine(Environment.CurrentDirectory, Config.Logging.Directory);
		}
		else
		{
			logPath = Path.IsPathFullyQualified(Config.Logging.Directory)
				? Config.Logging.Directory
				: Path.Combine(PlatformDirectories.ApplicationData, Config.Logging.Directory);
		}

		try
		{
			UrlOpener.OpenUrl(logPath);
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "ログディレクトリを開けませんでした");
		}
	}

	public IRelayCommand RegistMapPosition { get; } = new RelayCommand(() => StrongReferenceMessenger.Default.Send(new RegistMapPositionRequested()));
	public IRelayCommand ResetMapPosition { get; }
	public IRelayCommand<string> OpenUrl { get; } = new RelayCommand<string>(url => { if (url != null) UrlOpener.OpenUrl(url); });

	public IAsyncRelayCommand<KyoshinEewViewerConfiguration.SoundConfig> OpenSoundFile { get; }

	#region debug
	public string CurrentDirectory => Environment.CurrentDirectory;

	[ObservableProperty]
	public partial string ReplayBasePath { get; set; } = "";

	[ObservableProperty]
	public partial DateTimeOffset ReplaySelectedDate { get; set; } = DateTimeOffset.Now;

	[ObservableProperty]
	public partial TimeSpan ReplaySelectedTime { get; set; }

	[ObservableProperty]
	public partial string JmaEqdbId { get; set; } = "20180618075834";
	public void ProcessJmaEqdbRequest()
		=> ProcessJmaEqdbRequested.Request(JmaEqdbId);

	[ObservableProperty]
	public partial string QzqsmHexString { get; set; } = "9AAF8DED25000325BA00DA4A0F5AAC5A8000000008000000200000136DCCFB40";

	public void ProcessDCReportRequest()
	{
		var lines = QzqsmHexString.Split('\n');
		foreach (var line in lines)
		{
			var trimmedLine = line.Trim();
			if (string.IsNullOrWhiteSpace(trimmedLine))
				continue;
			try
			{
				DCReport report;
				if (trimmedLine.StartsWith("$QZQSM"))
				{
					// NMEAセンテンスとしてパース
					report = DCReport.ParseFromNmea(trimmedLine);
				}
				else
				{
					// HEX文字列としてパース
					report = DCReport.Parse(Convert.FromHexString(trimmedLine.Length % 2 != 0 ? trimmedLine + "0" : trimmedLine));
				}
				ProcessManualDCReportRequested.Request(report);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "デバッグ用DCレポートの解析中にエラーが発生しました");
				System.Diagnostics.Debug.WriteLine($"デバッグ用DCレポートの解析中にエラーが発生しました: {ex.Message}");
			}
		}
	}

	public void CrashApp()
		=> throw new ApplicationException("クラッシュボタンが押下されました。");
	#endregion
}

public record class SeriesViewModel(SeriesMeta Meta, KyoshinEewViewerConfiguration Config)
{
	public bool IsEnabled
	{
		get => Config.SeriesEnable.TryGetValue(Meta.Key, out var e) ? e : Meta.IsDefaultEnabled;
		set => Config.SeriesEnable[Meta.Key] = value;
	}
}

public record class SoundConfigViewModel(SoundCategory Category, List<Sound> Sounds);
