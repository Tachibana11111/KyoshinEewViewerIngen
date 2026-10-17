using KyoshinEewViewer.Core.Models;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Series.Earthquake.Templates;
using KyoshinEewViewer.Series.Earthquake.Workflow;
using KyoshinEewViewer.Series.KyoshinMonitor.Templates;
using KyoshinEewViewer.Series.KyoshinMonitor.Workflow;
using KyoshinEewViewer.Series.Tsunami.Models;
using KyoshinEewViewer.Series.Tsunami.Templates;
using KyoshinEewViewer.Series.Tsunami.Workflow;
using KyoshinMonitorLib;
using Scriban;

namespace KyoshinEewViewer.Tests.Templates;

[Collection("Loc")]
public class LocalizedTemplatesTest
{
	private static string Render(string key, string japanese, object data)
	{
		var text = TemplateLocalizer.Get(key, japanese);
		var template = Template.Parse(text);
		Assert.False(template.HasErrors, string.Join("\n", template.Messages));
		return template.Render(data, m => m.Name);
	}

	private static EarthquakeInformationEvent CreateEarthquake() => new(null)
	{
		LatestInformationName = "震源・震度に関する情報",
		EarthquakeId = "20241225123456",
		MaxIntensity = JmaIntensity.Int5Upper,
		Hypocenter = new EarthquakeInformationEventHypocenter(
			OccurrenceAt: DateTime.Parse("2024-12-25 12:34:56"),
			PlaceName: "テスト県テスト地方",
			Location: null,
			Magnitude: 5.5f,
			MagnitudeAlternativeText: null,
			Depth: 30,
			IsNoDepthData: false,
			IsVeryShallow: false,
			IsForeign: false),
		IsHypocenterOnly = false,
		IsDetailIntensityApplied = true,
		IsCancelled = false,
		IsTrainingOrTest = false,
		Comment = null,
	};

	private static EewEvent CreateEew(bool isFinal) => new(null, EewEventType.New)
	{
		EewId = "20241225123456",
		EewSource = "気象庁",
		SerialNo = 3,
		IsTrueCancelled = false,
		Intensity = JmaIntensity.Int4,
		IsIntensityOver = true,
		EpicenterPlaceName = "テスト県西部",
		EpicenterLocation = new Location(35.0f, 139.0f),
		Magnitude = 5.2f,
		Depth = 30,
		IsTemporaryEpicenter = false,
		IsWarning = false,
		IsFinal = isFinal,
		IsCancelled = false,
		IsReplay = false,
	};

	[Fact(DisplayName = "表示言語ごとに通知テンプレートと震度表記が切り替わる")]
	public void TemplatesAreLocalizedPerLanguage()
	{
		try
		{
			Loc.Instance.SetLanguage("ja");
			Assert.Equal("最大震度5強/12:34/テスト県テスト地方/30km/M5.5", Render("Tpl.Eq.Message", EarthquakeNotificationTemplates.NotificationMessage, CreateEarthquake()));

			Loc.Instance.SetLanguage("en");
			Assert.Equal("Max Intensity 5 upper/12:34/テスト県テスト地方/30km/M5.5", Render("Tpl.Eq.Message", EarthquakeNotificationTemplates.NotificationMessage, CreateEarthquake()));
			Assert.Equal("Intensity 4 or higher/テスト県西部/M5.2/30km", Render("Tpl.Eew.Message", KyoshinMonitorNotificationTemplates.EewNotificationMessage, CreateEew(false)));
			Assert.Equal("Earthquake Early Warning (No. 03 / Final)", Render("Tpl.Eew.Title", KyoshinMonitorNotificationTemplates.EewNotificationTitle, CreateEew(true)));

			Loc.Instance.SetLanguage("vi");
			Assert.Equal("Tối đa Cường độ 5 mạnh/12:34/テスト県テスト地方/30km/M5.5", Render("Tpl.Eq.Message", EarthquakeNotificationTemplates.NotificationMessage, CreateEarthquake()));
			Assert.Equal("Cảnh báo động đất sớm (bản tin 03)", Render("Tpl.Eew.Title", KyoshinMonitorNotificationTemplates.EewNotificationTitle, CreateEew(false)));

			var tsunami = new TsunamiInformationEvent(null)
			{
				TsunamiInfo = new TsunamiInfo
				{
					EventId = "20241225123456",
					SpecialState = null,
					ReportedAt = DateTime.Parse("2024-12-25 12:34:56"),
					MajorWarningAreas = [new TsunamiWarningArea(100, "沿岸A", "5m", "警報") { ArrivalTime = DateTime.Parse("2024-12-25 13:00:00") }],
				},
				PreviousLevel = TsunamiLevel.None,
			};
			Loc.Instance.SetLanguage("en");
			Assert.StartsWith("Major Tsunami Warning has been issued.", Render("Tpl.Tsunami.Message", TsunamiNotificationTemplates.NotificationMessage, tsunami));
			Assert.Equal("Major Tsunami Warning", Render("Tpl.Tsunami.Title", TsunamiNotificationTemplates.NotificationTitle, tsunami));
			Loc.Instance.SetLanguage("vi");
			Assert.StartsWith("Cảnh báo sóng thần lớn đã được công bố.", Render("Tpl.Tsunami.Message", TsunamiNotificationTemplates.NotificationMessage, tsunami));
		}
		finally
		{
			Loc.Instance.SetLanguage("ja");
		}
	}
}

[CollectionDefinition("Loc", DisableParallelization = true)]
public class LocCollection;
