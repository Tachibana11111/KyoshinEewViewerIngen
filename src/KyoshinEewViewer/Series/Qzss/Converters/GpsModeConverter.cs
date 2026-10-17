using Avalonia.Data.Converters;
using KyoshinEewViewer.Localization;
using System;
using System.Globalization;

namespace KyoshinEewViewer.Series.Qzss.Converters;

public class GpsModeConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value switch
		{
			"N" => Loc.Get("Gps.None"),
			"A" => Loc.Get("Gps.Autonomous"),
			"D" => Loc.Get("Gps.Differential"),
			"E" => Loc.Get("Gps.Estimated"),
			_ => Loc.Get("Accuracy.UnknownValue", value),
		};

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}
