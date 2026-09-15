using System;using System.Collections.Generic;using Game.Prefabs;using Unity.Entities;
namespace TrafficLightIntoKorea {
public partial class OverrideSystem {
 private sealed class Variant {internal Entity Source;internal string RoadName;internal HashSet<string> Disabled;internal ApproachProxies Proxies;internal bool Ready;internal readonly HashSet<Entity> WaitingOwners=new HashSet<Entity>();}
 private readonly Dictionary<string,Variant> _accessoryVariants=new Dictionary<string,Variant>();
 private readonly Dictionary<Entity,Variant> _accessoryVisuals=new Dictionary<Entity,Variant>();
 private readonly AccessoryLayout _accessoryLayouts=new AccessoryLayout();private int _accessoryGeneration;
 internal AccessorySlot[] Accessories(Entity visual){var source=VisualSource(visual);return _accessoryLayouts.Get(source==Entity.Null?visual:source,_prefabs,EntityManager);}
 internal string RoadNameFor(Entity visual)=>_accessoryVisuals.TryGetValue(visual,out var v)?v.RoadName:"";
 internal bool AccessoryVisible(Entity visual,string key)=>!_accessoryVisuals.TryGetValue(visual,out var v)||!v.Disabled.Contains(key);
 private Entity AccessorySource(Entity visual)=>_accessoryVisuals.TryGetValue(visual,out var v)?v.Source:Entity.Null;
 internal Entity AccessoryTarget(Entity owner,Entity visual,IEnumerable<string> disabled,string roadName=null){
  if(visual==Entity.Null)return visual;var source=VisualSource(visual);if(source==Entity.Null)return visual;
  var allowed=new HashSet<string>(StringComparer.Ordinal);foreach(var slot in Accessories(visual))allowed.Add(slot.Key);
  var hidden=new HashSet<string>(disabled??new string[0],StringComparer.Ordinal);hidden.IntersectWith(allowed);if(!RoadNameRules.Valid(roadName)||!Array.Exists(Accessories(visual),slot=>RoadNameRules.Valid(slot.Name))||RoadNamePrefab(roadName)==Entity.Null)roadName="";if(hidden.Count==0&&string.IsNullOrEmpty(roadName))return visual;
  string key=source+":"+AccessoryKeys.Signature(hidden)+":"+roadName;if(!_accessoryVariants.TryGetValue(key,out var variant)){
   variant=new Variant{Source=source,RoadName=roadName,Disabled=hidden,Proxies=new ApproachProxies("props_"+_accessoryGeneration+"_"+AccessoryKeys.Signature(hidden)+"_"+roadName,hidden,RoadNamePrefab(roadName))};_accessoryVariants.Add(key,variant);
  }
  var result=variant.Proxies.Resolve(source,_prefabs,EntityManager);if(result==Entity.Null){variant.WaitingOwners.Add(owner);variant.Proxies.Prepare(source,this,_prefabs,EntityManager);return visual;}
  _accessoryVisuals[result]=variant;Mounts.Alias(result,source);return result;
 }
 private void PrepareAccessoryVariants(){int budget=2;foreach(var variant in _accessoryVariants.Values){if(variant.Ready)continue;if(budget--<=0)break;variant.Proxies.Process(this,_prefabs,EntityManager);var e=variant.Proxies.Resolve(variant.Source,_prefabs,EntityManager);if(e==Entity.Null)continue;variant.Ready=true;_accessoryVisuals[e]=variant;Mounts.Alias(e,variant.Source);foreach(var owner in variant.WaitingOwners){World.GetExistingSystemManaged<ApproachSignalSystem>()?.RefreshOwner(owner);World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshOwner(owner);}variant.WaitingOwners.Clear();}}
 private void InvalidateAccessories(){_accessoryLayouts.Clear();_accessoryVariants.Clear();_accessoryGeneration++;}
}
}
