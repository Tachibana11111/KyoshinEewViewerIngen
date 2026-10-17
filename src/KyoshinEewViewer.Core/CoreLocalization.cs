using System;

namespace KyoshinEewViewer.Core;

/// <summary>
/// Core から表示用の文字列を取得するための仕組み。
/// 言語リソースを持つアプリ本体が <see cref="Resolver"/> を設定する。未設定のときは既定の文字列(日本語)を返す
/// </summary>
public static class CoreLocalization
{
	public static Func<string, string>? Resolver { get; set; }

	public static string Get(string key, string defaultText)
		=> Resolver?.Invoke(key) is { } text && text != key ? text : defaultText;
}
