using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using KyoshinEewViewer.Series.Earthquake.Workflow;
using KyoshinEewViewer.Series.KyoshinMonitor.Workflow;
using KyoshinEewViewer.Series.Qzss.Workflow;
using KyoshinEewViewer.Series.Tsunami.Workflow;
using KyoshinEewViewer.Services.Workflows.BuiltinTriggers;
using System;
using System.Text.Json.Serialization;

namespace KyoshinEewViewer.Services.Workflows;

public record WorkflowTriggerInfo(Type Type, string DisplayNameKey, Func<WorkflowTrigger> Create)
{
	/// <summary>
	/// 現在の表示言語での表示名
	/// </summary>
	public string DisplayName => Loc.Instance[DisplayNameKey];
}

[JsonDerivedType(typeof(DummyTrigger), typeDiscriminator: "Dummy")]
[JsonDerivedType(typeof(AllTrigger), typeDiscriminator: "All")]
[JsonDerivedType(typeof(ShakeDetectTrigger), typeDiscriminator: "KyoshinShakeDetected")]
[JsonDerivedType(typeof(EewTrigger), typeDiscriminator: "Eew")]
[JsonDerivedType(typeof(ApplicationStartupTrigger), typeDiscriminator: "ApplicationStartup")]
[JsonDerivedType(typeof(UpdateAvailableTrigger), typeDiscriminator: "UpdateAvailable")]
[JsonDerivedType(typeof(EarthquakeInformationTrigger), typeDiscriminator: "EarthquakeInformation")]
[JsonDerivedType(typeof(TsunamiInformationTrigger), typeDiscriminator: "TsunamiInformation")]
[JsonDerivedType(typeof(QzssTrigger), typeDiscriminator: "Qzss")]
public abstract class WorkflowTrigger : ObservableObject
{
	/// <summary>
	/// このトリガーが発火するイベントの型
	/// </summary>
	[JsonIgnore]
	public abstract Type EventType { get; }

	static WorkflowTrigger()
	{
		WorkflowService.RegisterTrigger<DummyTrigger>("WfTrigger.Dummy");
		WorkflowService.RegisterTrigger<AllTrigger>("WfTrigger.All");
		WorkflowService.RegisterTrigger<ApplicationStartupTrigger>("WfTrigger.Startup");
		WorkflowService.RegisterTrigger<UpdateAvailableTrigger>("WfTrigger.UpdateAvailable");
		WorkflowService.RegisterTrigger<ShakeDetectTrigger>("WfTrigger.ShakeDetect");
		WorkflowService.RegisterTrigger<EewTrigger>("WfTrigger.Eew");
		WorkflowService.RegisterTrigger<EarthquakeInformationTrigger>("WfTrigger.Earthquake");
		WorkflowService.RegisterTrigger<TsunamiInformationTrigger>("WfTrigger.Tsunami");
		WorkflowService.RegisterTrigger<QzssTrigger>("WfTrigger.Qzss");
	}

	[JsonIgnore]
	public abstract Control DisplayControl { get; }
	public abstract bool CheckTrigger(WorkflowEvent content);

	public abstract WorkflowEvent CreateTestEvent();
}

public class DummyTrigger : WorkflowTrigger
{
	public override Type EventType => typeof(TestEvent);
	[JsonIgnore]
	public override Control DisplayControl => new TextBlock { Text = Loc.Get("WfTrigger.Dummy.Desc") };

	public override bool CheckTrigger(WorkflowEvent content)
		=> content is TestEvent;

	public override WorkflowEvent CreateTestEvent()
		=> new TestEvent();
}

public class TestEvent : WorkflowEvent
{
	public TestEvent(): base("Test", null)
	{
		IsTest = true;
	}
	public DateTime Time { get; } = DateTime.Now;
}
