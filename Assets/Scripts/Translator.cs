using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translator : MonoBehaviour {

	public enum TargetLanguage {
		ChineseTraditional,
		Spanish
	}

	Dictionary<TargetLanguage, Dictionary<string, string>> wordMap;

	public void Init(TargetLanguage targetLanguage) {
		wordMap = new Dictionary<TargetLanguage, Dictionary<string, string>>
		{
			{TargetLanguage.Spanish, new Dictionary<string, string> 
				{
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
				}
			}
		};
	}

	public string translate(string word, TargetLanguage tl) {
		string translatedWord = wordMap[tl][word];
		if (translatedWord == null)
			translatedWord = word;
		return translatedWord;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
