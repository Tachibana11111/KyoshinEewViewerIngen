using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using KyoshinEewViewer.Services.Workflows.BuiltinActions;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using KyoshinEewViewer.Core;

namespace KyoshinEewViewer.Services.Workflows;

public record WorkflowActionInfo(Type Type, string DisplayNameKey, Func<WorkflowAction> Create)
{
	/// <summary>
	/// 現在の表示言語での表示名
	/// </summary>
	public string DisplayName => Loc.Instance[DisplayNameKey];
}

[JsonDerivedType(typeof(DummyAction), typeDiscriminator: "Dummy")]
[JsonDerivedType(typeof(MultipleAction), typeDiscriminator: "Multiple")]
[JsonDerivedType(typeof(SendNotificationAction), typeDiscriminator: "SendNotification")]
[JsonDerivedType(typeof(PlaySoundAction), typeDiscriminator: "PlaySound")]
[JsonDerivedType(typeof(WindowActivateAction), typeDiscriminator: "WindowActivate")]
[JsonDerivedType(typeof(WaitAction), typeDiscriminator: "Wait")]
[JsonDerivedType(typeof(LogOutputAction), typeDiscriminator: "LogOutput")]
[JsonDerivedType(typeof(WebhookAction), typeDiscriminator: "Webhook")]
[JsonDerivedType(typeof(ExecuteFileAction), typeDiscriminator: "ExecuteFile")]
[JsonDerivedType(typeof(VoicevoxSpeechAction), typeDiscriminator: "VoicevoxSpeech")]
[JsonDerivedType(typeof(SwitchTabAction), typeDiscriminator: "SwitchTab")]
public abstract class WorkflowAction : ObservableObject
{
	static WorkflowAction()
	{
		WorkflowService.RegisterAction<DummyAction>("WfAction.Dummy");
		WorkflowService.RegisterAction<MultipleAction>("WfAction.Multiple");
		WorkflowService.RegisterAction<SendNotificationAction>("WfAction.Notify");
		WorkflowService.RegisterAction<PlaySoundAction>("WfAction.PlaySound");
		WorkflowService.RegisterAction<VoicevoxSpeechAction>("WfAction.Voicevox");
		WorkflowService.RegisterAction<WindowActivateAction>("WfAction.Activate");
		WorkflowService.RegisterAction<SwitchTabAction>("WfAction.SwitchTab");
		WorkflowService.RegisterAction<WaitAction>("WfAction.Wait");
		WorkflowService.RegisterAction<LogOutputAction>("WfAction.Log");
		WorkflowService.RegisterAction<WebhookAction>("WfAction.Webhook");
		WorkflowService.RegisterAction<ExecuteFileAction>("WfAction.ExecuteFile");
	}

	[JsonIgnore]
	public abstract Control DisplayControl { get; }

	public virtual Task PrepareAsync(WorkflowEvent content) => Task.CompletedTask;
	public abstract Task ExecuteAsync(WorkflowEvent content);

	/// <summary>
	/// このアクションが所属するワークフローのトリガーからイベント型を取得する
	/// </summary>
	public Type? FindEventType()
		=> FindWorkflow()?.Trigger?.EventType;

	/// <summary>
	/// このアクションが所属するワークフローを取得する
	/// </summary>
	public Workflow? FindWorkflow()
	{
		var workflowService = ServiceLocator.Current.GetService<WorkflowService>();
		if (workflowService == null)
			return null;
		return workflowService.Workflows.Concat(workflowService.SystemWorkflows)
			.FirstOrDefault(w => w.Actions == this || w.Actions.ChildActions.Any(c => c.Action == this));
	}
}

public class DummyAction : WorkflowAction
{
	[JsonIgnore]
	public override Control DisplayControl => new TextBlock { Text = Loc.Get("WfAction.Dummy.Desc") };
	public override Task ExecuteAsync(WorkflowEvent content)
		=> Task.CompletedTask;
}
