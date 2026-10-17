using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using KyoshinEewViewer.Services.Workflows;
using KyoshinMonitorLib;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KyoshinEewViewer.Series.KyoshinMonitor.Workflow;

public partial class EewTrigger : WorkflowTrigger
{
	public override Type EventType => typeof(EewEvent);

	public static LocMap<JmaIntensity> ShindoNames { get; } = new()
	{
		{ JmaIntensity.Unknown, "ShindoName.Unknown" },
		{ JmaIntensity.Int0, "ShindoName.Int0" },
		{ JmaIntensity.Int1, "ShindoName.Int1" },
		{ JmaIntensity.Int2, "ShindoName.Int2" },
		{ JmaIntensity.Int3, "ShindoName.Int3" },
		{ JmaIntensity.Int4, "ShindoName.Int4" },
		{ JmaIntensity.Int5Lower, "ShindoName.Int5Lower" },
		{ JmaIntensity.Int5Upper, "ShindoName.Int5Upper" },
		{ JmaIntensity.Int6Lower, "ShindoName.Int6Lower" },
		{ JmaIntensity.Int6Upper, "ShindoName.Int6Upper" },
		{ JmaIntensity.Int7, "ShindoName.Int7" },
	};

	[JsonIgnore]
	public override Control DisplayControl => new EewTriggerControl() { DataContext = this };

	[ObservableProperty]
	public partial bool New { get; set; } = true;

	[ObservableProperty]
	public partial bool NewWarning { get; set; } = false;

	[ObservableProperty]
	public partial bool ContinueWarning { get; set; } = false;

	[ObservableProperty]
	public partial bool Continue { get; set; } = true;

	[ObservableProperty]
	public partial bool UpdateWithMoreAccurate { get; set; } = true;

	[ObservableProperty]
	public partial bool Final { get; set; } = true;

	[ObservableProperty]
	public partial bool Cancel { get; set; } = true;

	[ObservableProperty]
	public partial bool CancelWarning { get; set; } = false;

	[ObservableProperty]
	public partial bool WarningLevelReached { get; set; } = false;

	[ObservableProperty]
	public partial bool IncreaseInIntensity { get; set; } = false;

	[ObservableProperty]
	public partial bool DecreaseInIntensity { get; set; } = false;

	[ObservableProperty]
	public partial JmaIntensity Intensity { get; set; } = JmaIntensity.Unknown;

	[ObservableProperty]
	public partial bool IncludeGreaterIntensity { get; set; } = true;

	[ObservableProperty]
	public partial float Magnitude { get; set; } = 0;

	[ObservableProperty]
	public partial bool UseAndCondition { get; set; } = true;

	public override bool CheckTrigger(WorkflowEvent content)
	{
		if (content is not EewEvent eewEvent)
			return false;

		// 震度条件のチェック
		bool intensityMatch;
		if (IncludeGreaterIntensity)
			intensityMatch = eewEvent.Intensity >= Intensity;
		else
			intensityMatch = eewEvent.Intensity == Intensity;

		// マグニチュード条件のチェック (M0以上がデフォルトなので常に以上で比較)
		var magnitudeMatch = eewEvent.Magnitude >= Magnitude;

		// AND/OR条件の適用
		var conditionMatch = UseAndCondition
			? intensityMatch && magnitudeMatch
			: intensityMatch || magnitudeMatch;

		if (!conditionMatch)
			return false;

		return eewEvent.EventSubType switch
		{
			EewEventType.New => New,
			EewEventType.UpdateNewSerial => Continue,
			EewEventType.UpdateWithMoreAccurate => UpdateWithMoreAccurate,
			EewEventType.Final => Final,
			EewEventType.Cancel => Cancel,
			EewEventType.NewWarning => NewWarning,
			EewEventType.UpdateWarning => ContinueWarning,
			EewEventType.CancelWarning => CancelWarning,
			EewEventType.WarningLevelReached => WarningLevelReached,
			EewEventType.IncreaseMaxIntensity => IncreaseInIntensity,
			EewEventType.DecreaseMaxIntensity => DecreaseInIntensity,
			_ => false,
		};
	}

	public override WorkflowEvent CreateTestEvent()
	{
		var random = new Random();

		var eventTypes = new List<EewEventType>();
		if (New)
			eventTypes.Add(EewEventType.New);
		if (NewWarning)
			eventTypes.Add(EewEventType.NewWarning);
		if (Continue)
			eventTypes.Add(EewEventType.UpdateNewSerial);
		if (UpdateWithMoreAccurate)
			eventTypes.Add(EewEventType.UpdateWithMoreAccurate);
		if (Final)
			eventTypes.Add(EewEventType.Final);
		if (Cancel)
			eventTypes.Add(EewEventType.Cancel);

		EewEventType eventType = 0;
		if (eventTypes.Count > 0)
			eventType = eventTypes[random.Next(eventTypes.Count)];

		// 震度を条件に合わせて生成
		JmaIntensity intensity;
		if (IncludeGreaterIntensity)
			intensity = random.Next(JmaIntensity.Error - Intensity) + Intensity;
		else
			intensity = Intensity;

		// マグニチュードを条件に合わせて生成 (Magnitude以上のランダムな値)
		var magnitude = Magnitude + random.NextSingle() * (10 - Magnitude);

		return new EewEvent(null, eventType)
		{
			IsTest = true,

			OccurrenceAt = DateTime.Now.AddSeconds(-random.Next(60)),

			EewId = DateTime.Now.ToString("yyyyMMddHHmmss"),
			EewSource = "ワークフローのテストボタン",

			SerialNo = random.Next(20),

			IsTrueCancelled = eventType == EewEventType.Cancel ? random.Next() % 2 == 0 : false,

			Intensity = intensity,
			IsIntensityOver = intensity != JmaIntensity.Unknown && eventType != EewEventType.Cancel && random.Next() % 2 == 0,

			EpicenterPlaceName = "テスト",
			EpicenterLocation = new(random.NextSingle() * 180 - 90, random.NextSingle() * 360 - 180),

			Magnitude = magnitude,
			Depth = random.Next(20) * 10,

			IsTemporaryEpicenter = random.Next() % 2 == 0,

			IsWarning = intensity >= JmaIntensity.Int5Lower,
			WarningAreaCodes = intensity >= JmaIntensity.Int5Lower ? [9999] : null,
			WarningAreaNames = intensity >= JmaIntensity.Int5Lower ? ["テスト"] : null,
		};
	}
}
