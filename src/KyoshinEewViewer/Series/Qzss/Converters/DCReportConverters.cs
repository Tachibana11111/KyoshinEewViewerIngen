using Avalonia.Controls;
using KyoshinEewViewer.Localization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using KyoshinEewViewer.DCReportParser;
using KyoshinEewViewer.DCReportParser.Jma;
using KyoshinMonitorLib;
using System;
using System.Globalization;

namespace KyoshinEewViewer.Series.Qzss.Converters;

public class DCReportConverters : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> parameter switch
		{
			"ReportClassification" => value switch
			{
				ReportClassification.Maximum => Loc.Get("DC.Class.Maximum"),
				ReportClassification.Priority => Loc.Get("DC.Class.Priority"),
				ReportClassification.Regular => Loc.Get("DC.Class.Regular"),
				ReportClassification.TrainingOrTest => Loc.Get("DC.Class.TrainingShort"),
				_ => Loc.Get("Accuracy.UnknownValue", value)
			},
			"ReportClassificationLong" => value switch
			{
				ReportClassification.Maximum => Loc.Get("DC.Class.Maximum"),
				ReportClassification.Priority => Loc.Get("DC.Class.Priority"),
				ReportClassification.Regular => Loc.Get("DC.Class.Regular"),
				ReportClassification.TrainingOrTest => Loc.Get("DC.Class.TrainingLong"),
				_ => Loc.Get("Accuracy.UnknownValue", value)
			},
			"InformationType" => value switch
			{
				InformationType.Issue => Loc.Get("DC.Info.Issue"),
				InformationType.Cancellation => Loc.Get("DC.Info.Cancel"),
				InformationType.Correction => Loc.Get("DC.Info.Correct"),
				_ => "-",
			},
			"EewSeismicIntensity" => EewSeismicIntensityToJmaIntensity(value),
			"SeismicIntensity" => value switch
			{
				SeismicIntensity.Int4 => JmaIntensity.Int4,
				SeismicIntensity.Int5Lower => JmaIntensity.Int5Lower,
				SeismicIntensity.Int5Upper => JmaIntensity.Int5Upper,
				SeismicIntensity.Int6Lower => JmaIntensity.Int6Lower,
				SeismicIntensity.Int6Upper => JmaIntensity.Int6Upper,
				SeismicIntensity.Int7 => JmaIntensity.Int7,
				_ => JmaIntensity.Unknown,
			},
			"Magnitude" => value switch
			{
				(byte)127 => Loc.Get("Common.Unknown"),
				(byte)101 => Loc.Get("DC.Magnitude.Huge"),
				byte m => $"M{m / 10.0:F1}",
				_ => throw new NotImplementedException($"不明な Magnitude {value}")
			},
			"InformationSerialCode" => value switch
			{
				InformationSerialCode.InvestigatingA => Loc.Get("DC.Nankai.InvestigatingA"),
				InformationSerialCode.InvestigatingB => Loc.Get("DC.Nankai.InvestigatingB"),
				InformationSerialCode.InvestigatingC => Loc.Get("DC.Nankai.InvestigatingC"),
				InformationSerialCode.HugeEarthquakeWarning => Loc.Get("DC.Nankai.Warning"),
				InformationSerialCode.HugeEarthquakeCaution => Loc.Get("DC.Nankai.Advisory"),
				InformationSerialCode.InvestigateEnded => Loc.Get("DC.Nankai.Ended"),
				_ => Loc.Get("DC.Other")
			},
			"TsunamiWarningCode" => value switch
			{
				(byte)1 => Loc.Get("TsunamiView.11"),
				(byte)2 => Loc.Get("TsunamiReport.02"),
				(byte)3 => Loc.Get("TsunamiLevel.Warning"),
				(byte)4 or (byte)5 => Loc.Get("TsunamiLevel.MajorWarning"),
				(byte)15 => Loc.Get("DC.OtherWarning"),
				_ => Loc.Get("DC.OtherValue", value),
			},
			"ReferenceTimeType" => value is ReferenceTimeType referenceTimeType ? GetReferenceTimeTypeText(referenceTimeType, Loc.Get("DC.Information")) : Loc.Get("DC.Information"),
			"Epicenter" => value switch {
				0 => Loc.Get("DC.NoInformation"),
				int v => CsvDictionary.AreaEpicenter.TryGetValue(v, out var area) ? area : Loc.Get("DC.OtherValue", v),
				_ => Loc.Get("Common.Unknown"),
			},
			"Volcano" => value switch
			{
				int v => CsvDictionary.PointVolcano.TryGetValue(v, out var volcano) ? volcano : Loc.Get("DC.OtherValue", v),
				_ => Loc.Get("Common.Unknown"),
			},
			"VolcanicWarningCode" => value switch
			{
				(byte)127 => Loc.Get("DC.Other"),
				byte v => CsvDictionary.VolcanicWarning.TryGetValue(v, out var volcanicWarning) ? volcanicWarning : Loc.Get("DC.OtherValue", v),
				_ => Loc.Get("Common.Unknown"),
			},
			"Marine" => value switch
			{
				10000 => Loc.Get("DC.Other"),
				int v => CsvDictionary.AreaMarineJ.TryGetValue(v, out var marine) ? marine : Loc.Get("DC.OtherValue", v),
				_ => Loc.Get("Common.Unknown"),
			},
			"MarineWarningCode" => value switch
			{
				(byte)31 => Loc.Get("DC.Other"),
				byte v => CsvDictionary.MarineWarning.TryGetValue(v, out var marineWarning) ? marineWarning : Loc.Get("DC.OtherValue", v),
				_ => Loc.Get("Common.Unknown"),
			},
			"EewSeismicIntensityToForegroundColor" => GetColorFromIntensity(EewSeismicIntensityToJmaIntensity(value), true),
			"EewSeismicIntensityToBackgroundColor" => GetColorFromIntensity(EewSeismicIntensityToJmaIntensity(value), false),
			"IntensityToBackgroundColor" => GetColorFromIntensity(value is JmaIntensity i ? i : JmaIntensity.Error, false),
			"IntensityToForegroundColor" => GetColorFromIntensity(value is JmaIntensity i ? i : JmaIntensity.Error, true),
			"TsunamiArea" => value switch
			{
				990 => Loc.Get("DC.Reserved"),
				1000 => Loc.Get("DC.Other"),
				int v => CsvDictionary.AreaTsunami.TryGetValue(v, out var name) ? name : Loc.Get("DC.OtherValue", v),
				_ => Loc.Get("Common.Unknown"),
			},
			"WeatherDisasterSubCategory" => value switch
			{
				(byte)1 => Loc.Get("DC.Special.Blizzard"),
				(byte)2 => Loc.Get("DC.Special.HeavyRain"),
				(byte)3 => Loc.Get("DC.Special.Storm"),
				(byte)4 => Loc.Get("DC.Special.HeavySnow"),
				(byte)5 => Loc.Get("DC.Special.HighWave"),
				(byte)6 => Loc.Get("DC.Special.StormSurge"),
				(byte)7 => Loc.Get("DC.Special.All"),
				(byte)21 => Loc.Get("DC.Weather.ShortRain"),
				(byte)22 => Loc.Get("DC.Weather.Tornado"),
				(byte)23 => Loc.Get("DC.Weather.Landslide"),
				(byte)31 => Loc.Get("DC.Weather.OtherElement"),
				byte b => Loc.Get("DC.OtherValue", b),
				_ => Loc.Get("Common.Unknown"),
			},
			"AreaForecastLocalM" => value switch
			{
				500000 => Loc.Get("DC.Other"),
				int v => CsvDictionary.AreaForecastLocalM.TryGetValue(v, out var name) ? name : Loc.Get("DC.OtherValue", v),
				_ => Loc.Get("Common.Unknown"),
			},
			"TsunamigenicPotential" => value switch
			{
				(byte)0 => Loc.Get("DC.Tsunami.None"),
				(byte)1 => Loc.Get("DC.Tsunami.Wide"),
				(byte)2 => Loc.Get("DC.Tsunami.Regional"),
				(byte)3 => Loc.Get("DC.Tsunami.NearSource"),
				(byte)4 => Loc.Get("DC.Tsunami.LocalUnlikely"),
				byte b => Loc.Get("DC.OtherValue", b),
				_ => Loc.Get("Common.Unknown"),
			},
			"NPTCoastalRegionArea" => value is byte areaCode ? GetNPTCoastalRegionAreaText(areaCode) : Loc.Get("Common.Unknown"),
			"NPTCoastalRegion" => value is byte regionCode ? GetNPTCoastalRegionText(regionCode) : Loc.Get("Common.Unknown"),
			"AreaInformationCity" => value switch 
			{
				199999 => "北海道その他市町村",
				299999 => "青森県その他市町村",
				399999 => "岩手県その他市町村",
				499999 => "宮城県その他市町村",
				599999 => "秋田県その他市町村",
				699999 => "山形県その他市町村",
				799999 => "福島県その他市町村",
				899999 => "茨城県その他市町村",
				999999 => "栃木県その他市町村",
				1099999 => "群馬県その他市町村",
				1199999 => "埼玉県その他市町村",
				1299999 => "千葉県その他市町村",
				1399999 => "東京都その他市町村",
				1499999 => "神奈川県その他市町村",
				1599999 => "新潟県その他市町村",
				1699999 => "富山県その他市町村",
				1799999 => "石川県その他市町村",
				1899999 => "福井県その他市町村",
				1999999 => "山梨県その他市町村",
				2099999 => "長野県その他市町村",
				2199999 => "岐阜県その他市町村",
				2299999 => "静岡県その他市町村",
				2399999 => "愛知県その他市町村",
				2499999 => "三重県その他市町村",
				2599999 => "滋賀県その他市町村",
				2699999 => "京都府その他市町村",
				2799999 => "大阪府その他市町村",
				2899999 => "兵庫県その他市町村",
				2999999 => "奈良県その他市町村",
				3099999 => "和歌山県その他市町村",
				3199999 => "鳥取県その他市町村",
				3299999 => "島根県その他市町村",
				3399999 => "岡山県その他市町村",
				3499999 => "広島県その他市町村",
				3599999 => "山口県その他市町村",
				3699999 => "徳島県その他市町村",
				3799999 => "香川県その他市町村",
				3899999 => "愛媛県その他市町村",
				3999999 => "高知県その他市町村",
				4099999 => "福岡県その他市町村",
				4199999 => "佐賀県その他市町村",
				4299999 => "長崎県その他市町村",
				4399999 => "熊本県その他市町村",
				4499999 => "大分県その他市町村",
				4599999 => "宮崎県その他市町村",
				4699999 => "鹿児島県その他市町村",
				4799999 => "沖縄県その他市町村",
				int i => CsvDictionary.AreaInformationCity.TryGetValue(i, out var name) ? name : Loc.Get("DC.OtherValue", i),
				_ => Loc.Get("Common.Unknown"),
			},
			"AshFallWarningType" => value switch
			{
				(byte)1 => Loc.Get("DC.Flash"),
				(byte)2 => Loc.Get("DC.Detail"),
				byte b => Loc.Get("DC.OtherValue", b),
				_ => Loc.Get("Common.Unknown"),
			},
			"AshFallWarningColor" => value switch
			{
				(byte)1 => GetColorFromKey("AshfallLight"),
				(byte)2 => GetColorFromKey("AshfallModerate"),
				(byte)3 => GetColorFromKey("AshfallHeavy"),
				(byte)4 => GetColorFromKey("SmallVolcanicBombFall"),
				_ => GetColorFromKey("AshfallLight"),
			},
			"AshFallWarningForegroundColor" => value switch
			{
				(byte)1 => GetColorFromKey("AshfallLightForeground"),
				(byte)2 => GetColorFromKey("AshfallModerateForeground"),
				(byte)3 => GetColorFromKey("AshfallHeavyForeground"),
				(byte)4 => GetColorFromKey("SmallVolcanicBombFallForeground"),
				_ => GetColorFromKey("AshfallLightForeground"),
			},
			"AshFallWarningCode" => value switch
			{
				(byte)1 => Loc.Get("DC.Ash.Light"),
				(byte)2 => Loc.Get("DC.Ash.Moderate"),
				(byte)3 => Loc.Get("DC.Ash.Heavy"),
				(byte)4 => Loc.Get("DC.Ash.SmallRocks"),
				byte b => Loc.Get("DC.OtherValue", b),
				_ => Loc.Get("Common.Unknown"),
			},
			"AreaFloodForecast" => value switch 
			{
				19999999999 or 819999999999 => "北海道その他河川",
				29999999999 => "青森県その他河川",
				39999999999 => "岩手県その他河川",
				49999999999 => "宮城県その他河川",
				59999999999 => "秋田県その他河川",
				69999999999 => "山形県その他河川",
				79999999999 => "福島県その他河川",
				89999999999 => "茨城県その他河川",
				99999999999 => "栃木県その他河川",
				109999999999 => "群馬県その他河川",
				119999999999 => "埼玉県その他河川",
				129999999999 => "千葉県その他河川",
				139999999999 => "東京都その他河川",
				149999999999 => "神奈川県その他河川",
				159999999999 => "新潟県その他河川",
				169999999999 => "富山県その他河川",
				179999999999 => "石川県その他河川",
				189999999999 => "福井県その他河川",
				199999999999 => "山梨県その他河川",
				209999999999 => "長野県その他河川",
				219999999999 => "岐阜県その他河川",
				229999999999 => "静岡県その他河川",
				239999999999 => "愛知県その他河川",
				249999999999 => "三重県その他河川",
				259999999999 => "滋賀県その他河川",
				269999999999 => "京都府その他河川",
				279999999999 => "大阪府その他河川",
				289999999999 => "兵庫県その他河川",
				299999999999 => "奈良県その他河川",
				309999999999 => "和歌山県その他河川",
				319999999999 => "鳥取県その他河川",
				329999999999 => "島根県その他河川",
				339999999999 => "岡山県その他河川",
				349999999999 => "広島県その他河川",
				359999999999 => "山口県その他河川",
				369999999999 => "徳島県その他河川",
				379999999999 => "香川県その他河川",
				389999999999 => "愛媛県その他河川",
				399999999999 => "高知県その他河川",
				409999999999 => "福岡県その他河川",
				419999999999 => "佐賀県その他河川",
				429999999999 => "長崎県その他河川",
				439999999999 => "熊本県その他河川",
				449999999999 => "大分県その他河川",
				459999999999 => "宮崎県その他河川",
				469999999999 => "鹿児島県その他河川",
				479999999999 or 809999999999 => "沖縄県その他河川",
				829999999999 => "東北地方のその他河川",
				839999999999 => "関東地方のその他河川",
				849999999999 => "北陸地方のその他河川",
				859999999999 => "中部地方のその他河川",
				869999999999 => "近畿地方のその他河川",
				879999999999 => "中国地方のその他河川",
				889999999999 => "四国地方のその他河川",
				899999999999 => "九州地方のその他河川",
				long i => CsvDictionary.AreaFloodForecast.TryGetValue(i, out var name) ? name : Loc.Get("DC.OtherValue", i),
				_ => Loc.Get("Common.Unknown"),
			},
			"FloodWarningColor" => value switch
			{
				(byte)1 => GetColorFromKey("DockTitleBackgroundColor"),
				(byte)2 => GetColorFromKey("TsunamiAdvisoryColor"),
				(byte)3 => GetColorFromKey("TsunamiWarningColor"),
				(byte)4 => GetColorFromKey("TsunamiMajorWarningColor"),
				byte b => GetColorFromKey("DockTitleBackgroundColor"),
				_ => GetColorFromKey("DockTitleBackgroundColor"),
			},
			"FloodWarningForegroundColor" => value switch
			{
				(byte)1 => GetColorFromKey("SubForegroundColor"),
				(byte)2 => GetColorFromKey("TsunamiAdvisoryForegroundColor"),
				(byte)3 => GetColorFromKey("TsunamiWarningForegroundColor"),
				(byte)4 => GetColorFromKey("TsunamiMajorWarningForegroundColor"),
				byte b => GetColorFromKey("DockTitleForeground"),
				_ => GetColorFromKey("SubForegroundColor"),
			},
			"FloodWarningLevel" => value switch
			{
				(byte)1 => Loc.Get("TsunamiReport.02"),
				(byte)2 => Loc.Get("DC.Flood.Warning"),
				(byte)3 => Loc.Get("DC.Flood.Danger"),
				(byte)4 => Loc.Get("DC.Flood.Occurred"),
				byte b => Loc.Get("DC.OtherValue", b),
				_ => Loc.Get("Common.Unknown"),
			},
			"TyphoonScale" => value switch
			{
				(byte)0 => "-",
				byte b => GetTyphoonScaleText(b) ?? Loc.Get("DC.OtherValue", value),
				_ => Loc.Get("Common.Unknown"),
			},
			"TyphoonIntensity" => value switch
			{
				(byte)0 => "-",
				byte b => GetTyphoonIntensityText(b) ?? Loc.Get("DC.OtherValue", b),
				_ => Loc.Get("Common.Unknown"),
			},
			"TyphoonMaximumWindSpeed" => value switch
			{
				(byte)0 => Loc.Get("Common.Unknown"),
				byte b => b.ToString(),
				_ => Loc.Get("Common.Unknown"),
			},
			"TyphoonMaximumWindSpeedHasValue" => value switch
			{
				(byte)0 => false,
				byte => true,
				_ => false,
			},
			"TyphoonMaximumWindGustSpeed" => value switch
			{
				(byte)0 => Loc.Get("Common.Unknown"),
				byte b => b.ToString(),
				_ => Loc.Get("Common.Unknown"),
			},
			"TyphoonMaximumWindGustSpeedHasValue" => value switch
			{
				(byte)0 => false,
				byte => true,
				_ => false,
			},
			"TyphoonReferenceTimeType" => value is ReferenceTimeType typhoonReferenceTimeType ? GetReferenceTimeTypeText(typhoonReferenceTimeType) : Loc.Get("Common.Unknown"),
			_ => throw new NotImplementedException($"不明な targetType {targetType}")
		};

	/// <summary>
	/// 台風の大きさカテゴリ文字列を取得する。該当なし(0または未知の値)の場合は null を返す
	/// </summary>
	public static string? GetTyphoonScaleText(byte scale) => scale switch
	{
		1 => Loc.Get("DC.Typhoon.Large"),
		2 => Loc.Get("DC.Typhoon.VeryLarge"),
		_ => null,
	};

	/// <summary>
	/// 台風の強さカテゴリ文字列を取得する。該当なし(0または未知の値)の場合は null を返す
	/// </summary>
	public static string? GetTyphoonIntensityText(byte intensity) => intensity switch
	{
		1 => Loc.Get("DC.Typhoon.Strong"),
		2 => Loc.Get("DC.Typhoon.VeryStrong"),
		3 => Loc.Get("DC.Typhoon.Violent"),
		_ => null,
	};

	/// <summary>
	/// 基準時刻種別の文字列を取得する
	/// </summary>
	/// <param name="unknownText">未知の種別の場合に返す文字列</param>
	public static string GetReferenceTimeTypeText(ReferenceTimeType type, string? unknownText = null) => type switch
	{
		ReferenceTimeType.Analysis => Loc.Get("DC.Typhoon.Analysis"),
		ReferenceTimeType.Estimate => Loc.Get("DC.Typhoon.Estimate"),
		ReferenceTimeType.Forecast => Loc.Get("DC.Typhoon.Forecast"),
		_ => unknownText ?? Loc.Get("Common.Unknown"),
	};

	/// <summary>
	/// 北西太平洋津波の予報地点が属する沿岸地域名を取得する
	/// </summary>
	public static string GetNPTCoastalRegionAreaText(byte code) => code switch
	{
		1 or 2 or 66 or 67 => "カムチャツカ半島東海岸",
		3 or 4 => "千島列島",
		5 or 6 or 7 or (>= 68 and <= 70) => "韓国南海岸",
		8 or (>= 71 and <= 74) => "台湾",
		>= 9 and <= 14 => "フィリピン東海岸",
		(>= 15 and <= 20) or 75 => "イリアンジャヤ北海岸",
		(>= 21 and <= 28) or 76 => "パプアニューギニア北海岸",
		29 or 30 => "マリアナ諸島",
		31 => "パラオ",
		>= 32 and <= 35 => "ミクロネシア",
		36 => "マーシャル諸島",
		37 or 38 or 39 or 77 => "ソロモン諸島北海岸",
		40 or 41 or (>= 78 and <= 82) => "ソロモン海",
		83 => "珊瑚海",
		>= 84 and <= 86 => "東シナ海沿岸",
		_ => code.ToString(),
	};

	/// <summary>
	/// 北西太平洋津波の予報地点名を取得する
	/// </summary>
	public static string GetNPTCoastalRegionText(byte code) => code switch
	{
		1 => "ウスト・カムチャツク",
		2 => "ペトロパブロフスク・カムチャツキー",
		3 => "セベロ・クリリスク",
		4 => "ウルップ島",
		5 => "釜山",
		6 => "蘆花島",
		7 => "西帰浦",
		8 => "花蓮",
		9 => "バスコ",
		10 => "パラナン",
		11 => "レガスピ",
		12 => "ラオアン",
		13 => "マドリッド",
		14 => "ダバオ",
		15 => "ベレベレ",
		16 => "パタニ",
		17 => "ソロン",
		18 => "マノクワリ",
		19 => "ワルサ",
		20 => "ジャヤプラ",
		21 => "バニモ",
		22 => "ウェワク",
		23 => "マダン",
		24 => "マヌス諸島",
		25 => "ラバウル",
		26 => "カビエン",
		27 => "キンベ",
		28 => "キエタ",
		29 => "グアム",
		30 => "サイパン",
		31 => "マラカル",
		32 => "ヤップ島",
		33 => "チューク島",
		34 => "ポンペイ島",
		35 => "コスラエ島",
		36 => "エニウェトク島",
		37 => "パンゴー",
		38 => "アウキ",
		39 => "キラキラ",
		40 => "ムンダ",
		41 => "ホニアラ",
		73 or (>= 42 and <= 65) or (>= 87 and <= 96) => Loc.Get("DC.ReservedValue", code),
		66 => "カラギンスキー島",
		67 => "ニコリスコエ",
		68 => "統営",
		69 => "黒山島",
		70 => "済州島",
		71 => "基隆",
		72 => "台東",
		74 => "ホメル",
		75 => "ゲメ",
		76 => "ウラモナ",
		77 => "ガテレ",
		78 => "アムン",
		79 => "ファラマエ",
		80 => "ミシマ島",
		81 => "アロタウ",
		82 => "ラエ",
		83 => "ポートモレスビー",
		84 => "上海",
		85 => "舟山",
		86 => "温州",
		99 => "不明(99)",
		_ => Loc.Get("DC.OtherValue", code),
	};

	private JmaIntensity EewSeismicIntensityToJmaIntensity(object? value)
		=> value switch
		{
			EewSeismicIntensity.Int0 => JmaIntensity.Int0,
			EewSeismicIntensity.Int1 => JmaIntensity.Int1,
			EewSeismicIntensity.Int2 => JmaIntensity.Int2,
			EewSeismicIntensity.Int3 => JmaIntensity.Int3,
			EewSeismicIntensity.Int4 => JmaIntensity.Int4,
			EewSeismicIntensity.Int5Lower => JmaIntensity.Int5Lower,
			EewSeismicIntensity.Int5Upper => JmaIntensity.Int5Upper,
			EewSeismicIntensity.Int6Lower => JmaIntensity.Int6Lower,
			EewSeismicIntensity.Int6Upper => JmaIntensity.Int6Upper,
			EewSeismicIntensity.Int7 => JmaIntensity.Int7,
			_ => JmaIntensity.Unknown,
		};

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotImplementedException();

	private static SolidColorBrush GetColorFromIntensity(JmaIntensity intensity, bool isForeground)
	{
		var attr = isForeground ? "Foreground" : "Background";
		return new SolidColorBrush((Color)(KyoshinEewViewerApp.Application?.FindResource($"{intensity}{attr}") ?? throw new NullReferenceException("震度色リソースを取得できません")));
	}
	private static SolidColorBrush GetColorFromKey(string key)
		=> new SolidColorBrush((Color)(KyoshinEewViewerApp.Application?.FindResource(key) ?? throw new NullReferenceException($"リソース {key} を取得できません")));
}
