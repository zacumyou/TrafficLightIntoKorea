using System;
namespace Game.Settings {public class SettingsUIHiddenAttribute:Attribute {}}
namespace Newtonsoft.Json {public class JsonIgnoreAttribute:Attribute {}}
class Program {
 static void Main(){var s=new TrafficLightIntoKorea.Settings();int count=0;
 foreach(var p in typeof(TrafficLightIntoKorea.Settings).GetProperties()){
 var before=p.GetValue(s);p.SetValue(s,p.PropertyType==typeof(bool)?!(bool)before:p.PropertyType==typeof(int)?-999:"unapproved asset");
 if(!Equals(before,p.GetValue(s)))throw new Exception("Mutable: "+p.Name);
 if(!Attribute.IsDefined(p,typeof(Game.Settings.SettingsUIHiddenAttribute))||!Attribute.IsDefined(p,typeof(Newtonsoft.Json.JsonIgnoreAttribute)))throw new Exception("Exposed: "+p.Name);count++;
 }
 if(count!=15||!s.UseStraightLeftAssets||s.SecondHead||s.IndividualAssets.Split('\n').Length!=4||s.DirectionalTargets.Split('\n').Length!=5||s.ArrowId!=50||s.ArrowId2!=50)throw new Exception("Fixed data mismatch");
 Console.WriteLine("PASS: 15 fixed properties reject writes and are hidden/JSON-ignored; 4 assets, 5 meshes, fixed toggles and IDs.");}
}
