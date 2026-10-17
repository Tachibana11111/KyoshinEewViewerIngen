using Avalonia.Data.Converters;
using KyoshinEewViewer.Localization;
using System;
using System.Globalization;

namespace KyoshinEewViewer.Series.KyoshinMonitor.Converters;

public class AccuracyDetailToStringConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> parameter switch
		{
			"epicenter" or "depth" => value switch
			{
				0 => Loc.Get("Common.Unknown"),
				1 => Loc.Get("Accuracy.Epi.1"),
				2 => Loc.Get("Accuracy.Epi.2"),
				3 => Loc.Get("Accuracy.Epi.3"),
				4 => Loc.Get("Accuracy.Epi.4"),
				5 => Loc.Get("Accuracy.Epi.5"),
				6 => Loc.Get("Accuracy.Epi.6"),
				7 => Loc.Get("Accuracy.Epi.7"),
				8 => Loc.Get("Accuracy.Epi.8"),
				_ => Loc.Get("Accuracy.UnknownValue", value),
			},
			"magnitude" => value switch
			{
				0 => Loc.Get("Common.Unknown"),
				2 => Loc.Get("Accuracy.Mag.2"),
				3 => Loc.Get("Accuracy.Mag.3"),
				4 => Loc.Get("Accuracy.Mag.4"),
				5 => Loc.Get("Accuracy.Mag.5"),
				6 => "EPOS",
				8 => Loc.Get("Accuracy.Mag.8"),
				_ => Loc.Get("Accuracy.UnknownValue", value),
			},
			_ => Loc.Get("Accuracy.NoParameter"),
		};

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}
