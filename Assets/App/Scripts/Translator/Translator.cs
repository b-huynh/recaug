using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Translator {
	public enum TargetLanguage {
		English,
		Spanish,
		Japanese
	}

	private static Dictionary<TargetLanguage, Dictionary<string, string>> wordMap
		= new Dictionary<TargetLanguage, Dictionary<string, string>> {
		{
			TargetLanguage.Spanish, new Dictionary<string, string> {
				{"person", "persona"},
				{"bicycle", "bicicleta"}, 
				{"car", "coche"},
				{"motorcycle", "motocicleta"},
				{"airplane", "avión"},
				{"bus", "autobús"},
				{"train", "tren"},
				{"truck", "camión"},
				{"boat", "barco"},
				{"traffic light", "semáforo"},
				{"fire hydrant", "boca de incendio"},
				{"street sign", "cartel de la calle"},
				{"stop sign", "señal de stop"},
				{"parking meter", "parquímetro"},
				{"bench", "escaño"},
				{"bird", "pájaro"},
				{"cat", "gato"},
				{"dog", "perro"},
				{"horse", "caballo"},
				{"sheep", "oveja"},
				{"cow", "vaca"},
				{"elephant", "elefante"},
				{"bear", "oso"},
				{"zebra", "cebra"},
				{"giraffe", "jirafa"},
				{"hat", "sombrero"},
				{"backpack", "mochila"},
				{"umbrella", "paraguas"},
				{"shoe", "zapato"},
				{"eye glasses", "lentes"},
				{"handbag", "bolso"},
				{"tie", "corbata"},
				{"suitcase", "maleta"},
				{"frisbee", "disco volador"},
				{"skis", "esquí"},
				{"snowboard", "tabla de snowboard"},
				{"sports ball", "pelota deportiva"},
				{"kite", "cometa"},
				{"baseball bat", "bate de béisbol"},
				{"baseball glove", "guante de béisbol"},
				{"skateboard", "patineta"},
				{"surfboard", "tabla de surf"},
				{"tennis racket", "raqueta de tenis"},
				{"bottle", "botella"},
				{"plate", "plato"},
				{"wine glass", "copa de vino"},
				{"cup", "vaso"},
				{"fork", "tenedor"},
				{"knife", "cuchillo"},
				{"spoon", "cuchara"},
				{"bowl", "cuenco"},
				{"banana", "plátano"},
				{"apple", "manzana"},
				{"sandwich", "sandwich"},
				{"orange", "naranja"},
				{"broccoli", "brócoli"},
				{"carrot", "zanahoria"},
				{"hot dog", "perro caliente"},
				{"pizza", "pizza"},
				{"donut", "rosquilla"},
				{"cake", "pastel"},
				{"chair", "silla"},
				{"couch", "sofá"},
				{"potted plant", "planta en maceta"},
				{"bed", "cama"},
				{"mirror", "espejo"},
				{"dining table", "comedor"},
				{"window", "ventana"},
				{"desk", "escritorio"},
				{"toilet", "baño"},
				{"door", "puerta"},
				{"tv", "televisión"},
				{"laptop", "ordenador portátil"},
				{"mouse", "ratón"},
				{"remote", "control remoto"},
				{"keyboard", "teclado"},
				{"cell phone", "teléfono móvil"},
				{"microwave", "microonda"},
				{"oven", "horno"},
				{"toaster", "tostadora"},
				{"book", "libro"},
				{"smartphone", "teléfono móvil"},
			}
		}, 
		{
			TargetLanguage.Japanese, new Dictionary<string, string> {
				{"person", "人"},
				{"bicycle", "自転車"}, 
				{"car", "車"},
				{"motorcycle", "オートバイ"},
				{"airplane", "飛行機"},
				{"bus", "バス"},
				{"train", "電車"},
				{"truck", "トラック"},
				{"boat", "船"},
				{"traffic light", "信号機"},
				{"fire hydrant", "消火栓"},
				{"street sign", "道路標識"},
				{"stop sign", "一時停止"},
				{"parking meter", "パーキングメーター"},
				{"bench", "ベンチ"},
				{"bird", "鳥"},
				{"cat", "猫"},
				{"dog", "犬"},
				{"horse", "馬"},
				{"sheep", "羊"},
				{"cow", "牛"},
				{"elephant", "象"},
				{"bear", "熊"},
				{"zebra", "縞馬"},
				{"giraffe", "麒麟"},
				{"hat", "帽子"},
				{"backpack", "バックパック"},
				{"umbrella", "傘"},
				{"shoe", "靴"},
				{"eye glasses", "眼鏡"},
				{"handbag", "ハンドバッグ"},
				{"tie", "ネクタイ"},
				{"suitcase", "スーツケース"},
				{"frisbee", "フリスビー"},
				{"skis", "スキー板"},
				{"snowboard", "スノーボード"},
				{"sports ball", "スポーツボール"},
				{"kite", "凧"},
				{"baseball bat", "野球バット"},
				{"baseball glove", "野球グローブ"},
				{"skateboard", "スケートボード"},
				{"surfboard", "サーフボード"},
				{"tennis racket", "テニスラケット"},
				{"bottle", "ボトル"},
				{"plate", "皿"},
				{"wine glass", "ワイングラス"},
				{"cup", "カップ"},
				{"fork", "フォーク"},
				{"knife", "ナイフ"},
				{"spoon", "スプーン"},
				{"bowl", "ボウル"},
				{"banana", "バナナ"},
				{"apple", "林檎"},
				{"sandwich", "サンドイッチ"},
				{"orange", "オレンジ"},
				{"broccoli", "ブロコリ"},
				{"carrot", "人参"},
				{"hot dog", "ホットドッグ"},
				{"pizza", "ピザ"},
				{"donut", "ドーナツ"},
				{"cake", "ケーキ"},
				{"chair", "椅子"},
				{"couch", "ソファー"},
				{"potted plant", "鉢植え"},
				{"bed", "ベッド"},
				{"mirror", "鏡"},
				{"dining table", "ダイニングテーブル"},
				{"window", "窓"},
				{"desk", "机"},
				{"toilet", "トイレット"},
				{"door", "扉"},
				{"tv", "テレビ"},
				{"laptop", "パソコン"},
				{"mouse", "マウス"},
				{"remote", "リモコン"},
				{"keyboard", "キーボード"},
				{"cell phone", "携帯電話"},
				{"microwave", "電子レンジ"},
				{"oven", "オーブン"},
				{"toaster", "トースター"},
				{"book", "本"},
				{"smartphone", "スマートフォン"},
			}
		}
	};

	// Translates English word to a target language.
	public static string Translate(string word, TargetLanguage tl)
	{
		// Do nothing! Input is assumed to be English.
		if (tl == TargetLanguage.English)
		{
			return word;
		}

		if (!wordMap.ContainsKey(tl))
		{
			throw new System.ArgumentException(string.Format(
				"Cannot translate unknown language {0}", tl.ToString()));
		}

		try
		{
			return wordMap[tl][word];
		}
		catch
		{
			throw new System.ArgumentException(string.Format(
				"Cannot translate unknown word {0}", word));
		}
	}
}
