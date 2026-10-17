using Avalonia.Data.Converters;
using KyoshinEewViewer.Localization;
using DmdataSharp.Redundancy;
using System;
using System.Globalization;

namespace KyoshinEewViewer.Services.TelegramPublishers.Dmdata.Converters;

/// <summary>
/// RedundancyStatusを日本語文字列に変換するコンバーター
/// </summary>
public class RedundancyStatusToStringConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not RedundancyStatus status)
			return Loc.Get("Common.Unknown");

		return status switch
		{
			RedundancyStatus.FullyConnected => Loc.Get("Redundancy.Full"),
			RedundancyStatus.PartiallyConnected => Loc.Get("Redundancy.Partial"),
			RedundancyStatus.Disconnected => Loc.Get("Redundancy.Disconnected"),
			_ => status.ToString(),
		};
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}