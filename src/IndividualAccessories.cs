using System;using System.Collections.Generic;using System.Linq;using Game.Prefabs;using Unity.Entities;
namespace TrafficLightIntoKorea {
public partial class IndividualSignalData {
 internal bool AccessoryEnabled(Entity owner,Entity source,bool far,string key){var c=Get(owner,source);var disabled=far?c?.farProps:c?.nearProps;return disabled==null||Array.IndexOf(disabled,key)<0;}
 private Entity ApplyAccessories(Entity owner,Entity source,Entity prefab,bool far){if(prefab==Entity.Null)return prefab;var c=Get(owner,source);return World.GetExistingSystemManaged<OverrideSystem>().AccessoryTarget(owner,prefab,far?c?.farProps:c?.nearProps,far?c?.farRoadName:c?.nearRoadName);}
 internal bool SetAccessory(Entity display,IndividualSignalLink link,string slot,bool enabled){
  if(!_ready||!EntityManager.Exists(display)||!EntityManager.HasComponent<PrefabRef>(display))return false;
  var over=World.GetExistingSystemManaged<OverrideSystem>();var prefab=EntityManager.GetComponentData<PrefabRef>(display).m_Prefab;
  if(!over.Accessories(prefab).Any(item=>item.Key==slot))return false;
  var key=Key(link.Owner,link.Source);if(key==null)return false;
  var c=Get(link.Owner,link.Source);if(c==null)c=new SignalChoice{key=key};else if(c.key!=key)c=new SignalChoice{key=key,nearRoadName=c.nearRoadName,farRoadName=c.farRoadName,nearAsset=c.nearAsset,farAsset=c.farAsset,farMode=c.farMode,nearHidden=c.nearHidden,nearSigns=c.nearSigns,farSigns=c.farSigns,nearProps=c.nearProps,farOffset=c.farOffset,farRotation=c.farRotation,farLateral=c.farLateral,farProps=c.farProps};
  var disabled=new HashSet<string>((link.Far?c.farProps:c.nearProps)??new string[0],StringComparer.Ordinal);if(enabled)disabled.Remove(slot);else disabled.Add(slot);
  if(link.Far)c.farProps=disabled.OrderBy(x=>x,StringComparer.Ordinal).ToArray();else c.nearProps=disabled.OrderBy(x=>x,StringComparer.Ordinal).ToArray();_choices[key]=c;
  World.GetExistingSystemManaged<ApproachSignalSystem>()?.RefreshOwner(link.Owner);World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshOwner(link.Owner);return true;
 }
}
}
