using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// 値を翻訳済みの書式文字列に埋め込んで表示する。
/// 例: <c>Text="{loc:TrFormat Path=Count, Key=Eew.AreaCount}"</c> (リソースの値は <c>{0}地域</c> のような複合書式)。
/// 書式文字列は <see cref="Loc"/> にバインドされるため、言語を切り替えると即座に更新される
/// </summary>
public class TrFormatExtension : MarkupExtension
{
	public TrFormatExtension()
	{
	}

	public TrFormatExtension(string key)
	{
		Key = key;
	}

	public string Key { get; set; } = "";

	/// <summary>
	/// 埋め込む値のバインディングパス
	/// </summary>
	public string Path { get; set; } = ".";

	/// <summary>
	/// 値に先に適用するコンバーター
	/// </summary>
	public IValueConverter? Converter { get; set; }

	public object? ConverterParameter { get; set; }

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		var valueBinding = new ReflectionBindingExtension(Path)
		{
			Converter = Converter,
			ConverterParameter = ConverterParameter,
			Mode = BindingMode.OneWay,
		}.ProvideValue(serviceProvider);
		var formatBinding = new ReflectionBindingExtension($"[{Key}]")
		{
			Source = Loc.Instance,
			Mode = BindingMode.OneWay,
		}.ProvideValue(serviceProvider);

		return new MultiBinding
		{
			Converter = LocFormatConverter.Instance,
			Bindings = [valueBinding, formatBinding],
		};
	}
}

/// <summary>
/// [0]=値, [1]=複合書式文字列 として文字列を整形する
/// </summary>
public sealed class LocFormatConverter : IMultiValueConverter
{
	public static LocFormatConverter Instance { get; } = new();

	public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
	{
		if (values.Count < 2 || values[1] is not string format)
			return null;
		var value = values[0];
		if (value is BindingNotification || ReferenceEquals(value, AvaloniaProperty.UnsetValue))
			value = null;
		try
		{
			return string.Format(Loc.Instance.Culture, format, value);
		}
		catch (FormatException)
		{
			return value?.ToString();
		}
	}
}
