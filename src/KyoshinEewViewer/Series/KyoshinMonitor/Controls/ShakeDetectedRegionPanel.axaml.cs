using Avalonia;
using KyoshinEewViewer.Localization;
using Avalonia.Controls;
using Avalonia.Data;
using KyoshinEewViewer.Core.Models;
using KyoshinEewViewer.Series.KyoshinMonitor.Models;

namespace KyoshinEewViewer.Series.KyoshinMonitor.Controls;

public partial class ShakeDetectedRegionPanel : UserControl
{
	public ShakeDetectedRegionPanel()
	{
		InitializeComponent();
	}

	public static readonly StyledProperty<ShakeDetectedRegion[]> RegionsProperty =
		AvaloniaProperty.Register<ShakeDetectedRegionPanel, ShakeDetectedRegion[]>(
			nameof(Regions),
			defaultValue: []
		);

	public ShakeDetectedRegion[] Regions
	{
		get => GetValue(RegionsProperty);
		set => SetValue(RegionsProperty, value);
	}

	public static readonly StyledProperty<KyoshinEventLevel> LevelProperty =
		AvaloniaProperty.Register<ShakeDetectedRegionPanel, KyoshinEventLevel>(
			nameof(Level),
			defaultValue: KyoshinEventLevel.Weaker
		);

	public KyoshinEventLevel Level
	{
		get => GetValue(LevelProperty);
		set => SetValue(LevelProperty, value);
	}

	public string Title => GetTitle(Level);

	/// <summary>
	/// KyoshinEventLevel に応じたタイトルを取得
	/// </summary>
	public static string GetTitle(KyoshinEventLevel level)
		=> level switch
		{
			KyoshinEventLevel.Stronger => Loc.Get("Shake.Title.Stronger"),
			KyoshinEventLevel.Strong => Loc.Get("Shake.Title.Strong"),
			KyoshinEventLevel.Medium => Loc.Get("Shake.Title.Medium"),
			KyoshinEventLevel.Weak => Loc.Get("Shake.Title.Weak"),
			_ => Loc.Get("Shake.Title.Weaker"),
		};

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);
		if (change.Property == LevelProperty)
		{
			var oldTitle = change.OldValue is KyoshinEventLevel oldLevel ? GetTitle(oldLevel) : GetTitle(KyoshinEventLevel.Weaker);
			RaisePropertyChanged(TitleProperty, oldTitle, Title);
		}
	}

	private static readonly DirectProperty<ShakeDetectedRegionPanel, string> TitleProperty =
		AvaloniaProperty.RegisterDirect<ShakeDetectedRegionPanel, string>(
			nameof(Title),
			o => o.Title
		);
}
