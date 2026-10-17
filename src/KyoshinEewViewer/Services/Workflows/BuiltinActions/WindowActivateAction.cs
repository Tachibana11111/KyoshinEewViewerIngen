using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using CommunityToolkit.Mvvm.Messaging;
using KyoshinEewViewer.Core.Models.Events;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KyoshinEewViewer.Services.Workflows.BuiltinActions;

public class WindowActivateAction : WorkflowAction
{
	[JsonIgnore]
	public override Control DisplayControl => new TextBlock { Text = Loc.Get("WfAction.Activate.Desc") };

	public override Task ExecuteAsync(WorkflowEvent content)
	{
		StrongReferenceMessenger.Default.Send(new ShowMainWindowRequested());
		return Task.CompletedTask;
	}
}
