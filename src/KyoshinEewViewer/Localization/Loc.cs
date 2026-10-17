using KyoshinEewViewer.Core;
using KyoshinEewViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;

[assembly: NeutralResourcesLanguage("ja")]

namespace KyoshinEewViewer.Localization;

/// <summary>
/// 選択可能な表示言語
/// </summary>
/// <param name="Code">言語コード。空文字はシステムの言語に従う</param>
/// <param name="DisplayName">その言語自身での表示名</param>
public record LanguageOption(string Code, string DisplayName);

/// <summary>
/// 表示言語の管理と文字列リソースの取得を行う。
/// 言語を切り替えると indexer の変更が通知され、<see cref="TrExtension"/> でバインドした UI が即座に更新される。
/// 基底言語 (Strings.resx) は日本語で、訳が存在しない文字列は日本語にフォールバックする
/// </summary>
public sealed class Loc : INotifyPropertyChanged
{
	public static Loc Instance { get; } = new();

	public static IReadOnlyList<LanguageOption> Options { get; } =
	[
		new("", "System / システム / Hệ thống"),
		new("ja", "日本語"),
		new("en", "English"),
		new("vi", "Tiếng Việt"),
	];

	private static readonly string[] SupportedCodes = ["ja", "en", "vi"];

	private readonly ResourceManager resourceManager = new("KyoshinEewViewer.Localization.Strings", typeof(Loc).Assembly);
	private CultureInfo culture = CultureInfo.InvariantCulture;

	/// <summary>
	/// 現在の表示言語のカルチャ。日時などの書式化に使用する
	/// </summary>
	public CultureInfo Culture => culture.Equals(CultureInfo.InvariantCulture) ? JapaneseCulture : culture;

	private static readonly CultureInfo JapaneseCulture = CultureInfo.GetCultureInfo("ja-JP");

	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// 表示言語が切り替わった後に発生する
	/// </summary>
	public event Action? LanguageChanged;

	public string this[string key] => resourceManager.GetString(key, culture) ?? key;

	/// <summary>
	/// 現在の表示言語のコード (ja / en / vi)
	/// </summary>
	public string CurrentCode { get; private set; } = "ja";

	/// <summary>
	/// 指定した言語コードで表示言語を切り替える。空・未対応の場合はシステムの言語に従い、対応していなければ日本語にする
	/// </summary>
	public void SetLanguage(string? code)
	{
		var resolved = Resolve(code);
		if (resolved == CurrentCode)
			return;
		CurrentCode = resolved;
		culture = resolved == "ja" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(resolved);
		// indexer のバインディングは実装により通知名が異なるため、複数の名前で通知する
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
		LanguageChanged?.Invoke();
	}

	/// <summary>
	/// 設定の言語を適用し、以降の変更に追従する
	/// </summary>
	public void Attach(KyoshinEewViewerConfiguration config)
	{
		CoreLocalization.Resolver = key => Instance[key];
		SetLanguage(config.Language);
		config.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(KyoshinEewViewerConfiguration.Language))
				SetLanguage(config.Language);
		};
	}

	/// <summary>
	/// コードビハインドから文字列を取得する。書式引数を渡すと <see cref="string.Format(IFormatProvider, string, object[])"/> で整形する
	/// </summary>
	public static string Get(string key, params object?[] args)
	{
		var text = Instance[key];
		return args.Length == 0 ? text : string.Format(Instance.culture, text, args);
	}

	private static string Resolve(string? code)
	{
		if (!string.IsNullOrEmpty(code) && Array.IndexOf(SupportedCodes, code) >= 0)
			return code;
		var system = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		return Array.IndexOf(SupportedCodes, system) >= 0 ? system : "ja";
	}
}
