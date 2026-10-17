using System;
using System.Text.Json.Serialization;

namespace KyoshinEewViewer.Core;

public enum ThemeType
{
	BuiltIn,
	ExternalFile,
	Temporary,
}
public record ThemeMeta(ThemeType Type, string Identifier)
{
	[JsonIgnore]
	public string DisplayName => Type switch
	{
		ThemeType.BuiltIn => CoreLocalization.Get("Theme.BuiltIn", "組み込みテーマ"),
		ThemeType.ExternalFile => string.Format(CoreLocalization.Get("Theme.External", "外部テーマ: {0}"), Identifier),
		ThemeType.Temporary => CoreLocalization.Get("Theme.Temporary", "編集中のテーマ"),
		_ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null),
	};
}
