using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using KyoshinEewViewer.Events;
using KyoshinEewViewer.Series;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KyoshinEewViewer.Services.Workflows.BuiltinActions;

public class SwitchTabAction : WorkflowAction
{
	[JsonIgnore]
	public override Control DisplayControl => new TextBlock {
		Text = Loc.Get("WfAction.SwitchTab.Desc"),
	};

	public override Task ExecuteAsync(WorkflowEvent content)
	{
		if (content.EventedSeries is not SeriesBase series)
			return Task.CompletedTask;
		ActiveRequest.Send(series);
		return Task.CompletedTask;
	}
}
