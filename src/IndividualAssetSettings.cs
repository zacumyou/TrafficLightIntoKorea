using System;
using System.Collections.Generic;
using Game.Settings;
using Game.UI.Widgets;
namespace TrafficLightIntoKorea {
public sealed partial class Settings {
 
 [SettingsUIHidden] public string IndividualAssetPicker {get;set;}="";
 public DropdownItem<string>[] IndividualAssetItems(){
  var names=new SortedSet<string>(Catalog.Vehicles,StringComparer.Ordinal);names.UnionWith(Catalog.Pedestrians);names.UnionWith(DirectionalTargetNames.Parse(IndividualAssets));
  var items=new List<DropdownItem<string>>{new DropdownItem<string>{value="",displayName=GetOptionLabelLocaleID("IndividualAssetEmpty")}};
  foreach(var name in names)if(!name.StartsWith("NA_")&&!name.StartsWith("EU_"))items.Add(new DropdownItem<string>{value=name,displayName=name});return items.ToArray();
 }
 [SettingsUIHidden] public bool AddIndividualAsset {set{if(Catalog.Entries.ContainsKey(IndividualAssetPicker??"")&&!IndividualAssetPicker.StartsWith("NA_")&&!IndividualAssetPicker.StartsWith("EU_")){IndividualAssets=DirectionalTargetNames.Add(IndividualAssets,IndividualAssetPicker);ApplyAndSave();}}}
 [SettingsUIHidden] public bool RemoveIndividualAsset {set{IndividualAssets=DirectionalTargetNames.Remove(IndividualAssets,IndividualAssetPicker);ApplyAndSave();}}
 [SettingsUIHidden] public string ActiveIndividualAssets=>string.Join(" / ",DirectionalTargetNames.Parse(IndividualAssets));
}
}
