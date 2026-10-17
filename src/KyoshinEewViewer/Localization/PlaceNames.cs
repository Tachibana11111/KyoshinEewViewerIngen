using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// 気象庁が配信する地名 (震央地名・都道府県・市区町村・津波予報区など) を表示言語に合わせて変換する。
/// 変換表 (Assets/place-names.json) は気象庁の公開データと、読みから生成したローマ字表記を元にしている。
/// 表にない名称はそのまま (日本語で) 表示する
/// </summary>
public static class PlaceNames
{
	private sealed record Data(Dictionary<string, string[]> Places, Dictionary<string, string[]> Phrases);

	private static readonly Lazy<Data> Table = new(Load);

	private static Data Load()
	{
		try
		{
			using var stream = AssetLoader.Open(new Uri("avares://KyoshinEewViewer/Assets/place-names.json"));
			var doc = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string[]>>>(stream);
			return new Data(
				doc?.GetValueOrDefault("places") ?? [],
				doc?.GetValueOrDefault("phrases") ?? []);
		}
		catch (Exception)
		{
			return new Data([], []);
		}
	}

	private static string Pick(string[] names, string code)
		=> names.Length >= 2 ? (code == "vi" ? names[1] : names[0]) : names[0];

	/// <summary>
	/// 地名や気象庁の定型文を現在の表示言語に変換する。日本語、または対応する訳がない場合は元の文字列を返す
	/// </summary>
	public static string Localize(string? name)
	{
		if (string.IsNullOrEmpty(name))
			return name ?? "";
		var code = Loc.Instance.CurrentCode;
		if (code == "ja")
			return name;
		var data = Table.Value;
		if (data.Places.TryGetValue(name, out var place))
			return Pick(place, code);
		if (data.Phrases.TryGetValue(name, out var phrase))
			return Pick(phrase, code);
		// 複数の定型文が連結されている場合は、含まれる定型文ごとに置き換える
		var result = name;
		foreach (var (ja, names) in data.Phrases)
			if (result.Contains(ja, StringComparison.Ordinal))
				result = result.Replace(ja, Pick(names, code), StringComparison.Ordinal);
		return result;
	}
}

/// <summary>
/// 地名を表示言語に合わせて変換するコンバーター。[0]=地名, [1]=現在の言語コード (言語切り替えの通知を受けるためのもの)
/// </summary>
public sealed class PlaceNameConverter : IMultiValueConverter
{
	public static PlaceNameConverter Instance { get; } = new();

	public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
		=> values.Count > 0 && values[0] is string name ? PlaceNames.Localize(name) : values.Count > 0 ? values[0]?.ToString() : null;
}

/// <summary>
/// AXAML から地名を表示言語に合わせてバインドする。例: <c>Text="{loc:Place Path=CurrentEvent.Place}"</c>
/// </summary>
public class PlaceExtension : MarkupExtension
{
	public PlaceExtension()
	{
	}

	public PlaceExtension(string path)
	{
		Path = path;
	}

	public string Path { get; set; } = ".";

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		var valueBinding = new ReflectionBindingExtension(Path) { Mode = BindingMode.OneWay }.ProvideValue(serviceProvider);
		var languageBinding = new ReflectionBindingExtension(nameof(Loc.CurrentCode)) { Source = Loc.Instance, Mode = BindingMode.OneWay }.ProvideValue(serviceProvider);
		return new MultiBinding
		{
			Converter = PlaceNameConverter.Instance,
			Bindings = [valueBinding, languageBinding],
		};
	}
}
