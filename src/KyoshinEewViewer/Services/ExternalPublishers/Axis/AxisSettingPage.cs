using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Core.Models;
using KyoshinEewViewer.Series;

namespace KyoshinEewViewer.Services.ExternalPublishers.Axis;

public class AxisSettingPage : LocalizedObservableObject, ISettingPage
{
	public bool IsVisible => true;

	public string? Icon => null;

	public string Title => Loc.Instance["Setting.Axis"];

	public Control DisplayControl => new AxisPage() { DataContext = this };

	public ISettingPage[] SubPages => [];


	public KyoshinEewViewerConfiguration Config { get; }
	public AxisInformationProvider Client { get; }

	public AxisSettingPage(KyoshinEewViewerConfiguration config, AxisInformationProvider client)
	{
		Config = config;
		Client = client;
	}
}
