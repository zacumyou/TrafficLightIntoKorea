using System.Collections.Generic;using System.IO;using Newtonsoft.Json;using Game.SceneFlow;
namespace TrafficLightIntoKorea {
internal static class IndividualLocalization {
 static Dictionary<string,string> Read(string lang){using(var stream=typeof(IndividualLocalization).Assembly.GetManifestResourceStream("TrafficLightIntoKorea.lang."+lang+".json"))using(var reader=new StreamReader(stream))return JsonConvert.DeserializeObject<Dictionary<string,string>>(reader.ReadToEnd());}
 static readonly Dictionary<string,string> Korean=Read("ko-KR"),English=Read("en-US");
 internal static void Add(Dictionary<string,string> target,string lang){foreach(var item in lang=="ko-KR"?Korean:English)target[item.Key]=item.Value;}
 internal static Dictionary<string,string> Texts(){var result=new Dictionary<string,string>();var dictionary=GameManager.instance.localizationManager.activeDictionary;foreach(var item in Korean)result[item.Value]=dictionary.TryGetValue(item.Key,out var translated)?translated:English[item.Key];return result;}
 internal static string Translate(string text){if(string.IsNullOrEmpty(text))return text;var texts=Texts();return texts.TryGetValue(text,out var translated)?translated:text;}
}
}
