using KyoshinMonitorLib;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// 通知テンプレートを表示言語に合わせて切り替える。
/// 日本語 (およびリソースが存在しない言語) では呼び出し側が持つ既定のテンプレートをそのまま返す
/// </summary>
public static class TemplateLocalizer
{
	public static string Get(string key, string japaneseTemplate)
	{
		if (Loc.Instance.CurrentCode == "ja")
			return japaneseTemplate;
		var localized = Loc.Instance[key];
		return localized == key ? japaneseTemplate : localized;
	}
}

/// <summary>
/// 震度の表示名を表示言語に合わせて取得する
/// </summary>
public static class IntensityNames
{
	/// <summary>
	/// 「震度5強」のような長い表記を取得する。日本語では従来の表記をそのまま使う
	/// </summary>
	public static string GetLongName(JmaIntensity intensity)
	{
		if (Loc.Instance.CurrentCode == "ja")
			return intensity.ToLongString();
		var key = "ShindoName." + intensity;
		var name = Loc.Instance[key];
		return name == key ? intensity.ToLongString() : name;
	}
}
