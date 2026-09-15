using System;using System.Collections.Generic;using Game.UI.Widgets;using Game.SceneFlow;using Unity.Entities;
namespace TrafficLightIntoKorea {
internal static class Catalog {
 internal static readonly Dictionary<string,Entity> Entries=new Dictionary<string,Entity>(StringComparer.Ordinal);
 internal static readonly HashSet<string> Vehicles=new HashSet<string>(),Pedestrians=new HashSet<string>();
 internal static int Version,Applied,Unavailable;internal static bool Locked;
 internal static string Alias;
 internal static DropdownItem<string>[] Items(string selected,Settings s,bool pedestrian=false){var a=new List<DropdownItem<string>>{new DropdownItem<string>{value="",displayName=s.GetOptionLabelLocaleID("Original")}};var names=new List<string>(pedestrian?Pedestrians:Vehicles);names.Sort(StringComparer.Ordinal);foreach(var n in names)a.Add(new DropdownItem<string>{value=n,displayName=n});if(!string.IsNullOrEmpty(selected)&&!Entries.ContainsKey(selected))a.Add(new DropdownItem<string>{value=selected,displayName=selected+" [—]"});return a.ToArray();}
 internal static string Status(Settings s){string key=s.GetOptionLabelLocaleID(Locked?"Locked":"Preparing");var gm=GameManager.instance;string label=gm!=null&&gm.localizationManager.activeDictionary.TryGetValue(key,out var text)?text:key;string counterKey=s.GetOptionLabelLocaleID("PairStatus");string counter=gm!=null&&gm.localizationManager.activeDictionary.TryGetValue(counterKey,out var counterText)?counterText:counterKey;return string.Format(counter,LeftSignalSystem.HiddenCount,LeftSignalSystem.Examined)+" · "+label+" ("+Applied+" / "+SourceNames.All.Length+"; — "+Unavailable+")";}
}
}
