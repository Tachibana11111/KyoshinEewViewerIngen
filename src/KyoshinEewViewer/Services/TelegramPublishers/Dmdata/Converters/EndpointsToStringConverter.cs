using Avalonia.Data.Converters;
using KyoshinEewViewer.Localization;
using DmdataSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KyoshinEewViewer.Services.TelegramPublishers.Dmdata.Converters;

/// <summary>
/// エンドポイント配列を改行区切り文字列に変換するコンバーター
/// </summary>
public class EndpointsToStringConverter : IValueConverter
{
	private static readonly Dictionary<string, string> EndpointNameKeys = new()
	{
		{ DmdataV2SocketEndpoints.Global, "Endpoint.Global" },
		{ DmdataV2SocketEndpoints.Tokyo, "Endpoint.Tokyo" },
		{ DmdataV2SocketEndpoints.Osaka, "Endpoint.Osaka" },
		{ DmdataV2SocketEndpoints.Apne1Az4, "apne1-az4" },
		{ DmdataV2SocketEndpoints.Apne1Az1, "apne1-az1" },
		{ DmdataV2SocketEndpoints.Apne3Az3, "apne3-az3" },
		{ DmdataV2SocketEndpoints.Apne3Az1, "apne3-az1" },
	};

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not string[] endpoints)
			return Loc.Get("Common.None");

		if (endpoints.Length == 0)
			return Loc.Get("Common.None");

		var displayNames = endpoints.Select(endpoint =>
			EndpointNameKeys.TryGetValue(endpoint, out var nameKey) ? Loc.Get(nameKey) : endpoint
		);

		return string.Join(", ", displayNames);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}
