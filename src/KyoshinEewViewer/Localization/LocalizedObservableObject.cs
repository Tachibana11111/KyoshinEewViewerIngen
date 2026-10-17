using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace KyoshinEewViewer.Localization;

/// <summary>
/// 表示言語が切り替わったときにすべてのプロパティの変更を通知する <see cref="ObservableObject"/>。
/// コードから <see cref="Loc"/> で生成した文字列を公開するプロパティが、言語切り替えに追従できるようにする。
/// イベントは弱参照で購読するため、インスタンスの寿命には影響しない
/// </summary>
public abstract class LocalizedObservableObject : ObservableObject
{
	protected LocalizedObservableObject()
	{
		var weak = new WeakReference<LocalizedObservableObject>(this);
		Action? handler = null;
		handler = () =>
		{
			if (weak.TryGetTarget(out var target))
				target.OnLanguageChanged();
			else
				Loc.Instance.LanguageChanged -= handler;
		};
		Loc.Instance.LanguageChanged += handler;
	}

	/// <summary>
	/// 表示言語が切り替わったときに呼ばれる。既定ではすべてのプロパティの変更を通知する
	/// </summary>
	protected virtual void OnLanguageChanged() => OnPropertyChanged(string.Empty);
}
