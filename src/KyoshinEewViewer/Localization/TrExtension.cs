using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// AXAML から翻訳済みの文字列をバインドする。例: <c>Text="{loc:Tr General.Appearance}"</c>
/// 表示言語を切り替えると自動で更新される
/// </summary>
public class TrExtension : MarkupExtension
{
	public TrExtension()
	{
	}

	public TrExtension(string key)
	{
		Key = key;
	}

	public string Key { get; set; } = "";

	public override object ProvideValue(IServiceProvider serviceProvider)
		=> new ReflectionBindingExtension($"[{Key}]")
		{
			Source = Loc.Instance,
			Mode = BindingMode.OneWay,
		}.ProvideValue(serviceProvider);
}
