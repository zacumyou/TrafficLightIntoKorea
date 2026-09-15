using System;using System.Collections.Generic;using Game.Settings;
namespace TrafficLightIntoKorea {
internal static class DirectionalTargetNames {
 internal static List<string> Parse(string text){var result=new List<string>();foreach(var name in (text??"").Split(new[]{'\n','\r'},StringSplitOptions.RemoveEmptyEntries))if(!result.Contains(name))result.Add(name);return result;}
 internal static string Add(string text,string name){var items=Parse(text);if(!string.IsNullOrEmpty(name)&&!items.Contains(name))items.Add(name);return string.Join("\n",items);}
 internal static string Remove(string text,string name){var items=Parse(text);items.Remove(name);return string.Join("\n",items);}
}
public sealed partial class Settings {
 [SettingsUISection("Advanced","DirectionalControl")] public bool KoreanTransitions {get;set;}=true;
 [SettingsUISection("Advanced","DirectionalControl"),SettingsUISlider(min=.25f,max=3,step=.25f)] public float AmberSeconds {get;set;}=1f;
 
 [SettingsUIHidden] public bool DirectionalTargetsMigrated {get;set;}
 internal void MigrateDirectionalTargets(){if(DirectionalTargetsMigrated)return;DirectionalTargets=DirectionalTargetNames.Add(DirectionalTargets,DirectionalMesh);DirectionalTargetsMigrated=true;}
 [SettingsUIHidden] public bool AddDirectionalTarget {set{DirectionalTargets=DirectionalTargetNames.Add(DirectionalTargets,DirectionalMesh);ApplyAndSave();}}
 [SettingsUIHidden] public bool RemoveDirectionalTarget {set{DirectionalTargets=DirectionalTargetNames.Remove(DirectionalTargets,DirectionalMesh);ApplyAndSave();}}
 [SettingsUIHidden] public string ActiveDirectionalTargets=>string.Join(" / ",DirectionalTargetNames.Parse(DirectionalTargets));
}
}
