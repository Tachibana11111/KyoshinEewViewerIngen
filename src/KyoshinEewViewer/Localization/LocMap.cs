using System.Collections.Generic;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// ComboBox などに表示する「値 → 表示名」の一覧の 1 項目。
/// 表示言語が切り替わると <see cref="Value"/> の変更が通知される
/// </summary>
public sealed class LocItem<TKey>(TKey key, string resourceKey) : LocalizedObservableObject
{
	public TKey Key { get; } = key;

	/// <summary>
	/// 現在の表示言語での表示名
	/// </summary>
	public string Value => Loc.Instance[resourceKey];
}

/// <summary>
/// 値と言語リソースのキーの組を保持する、言語切り替えに追従する一覧。
/// コレクション初期化子で <c>{ 値, "リソースキー" }</c> のように追加する
/// </summary>
public sealed class LocMap<TKey> : List<LocItem<TKey>>
{
	public void Add(TKey key, string resourceKey)
		=> Add(new LocItem<TKey>(key, resourceKey));
}
