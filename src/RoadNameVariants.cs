using System;using System.Linq;using Game.Prefabs;using Unity.Entities;
namespace TrafficLightIntoKorea {
internal static class RoadNameRules {
 internal static readonly string[] Names={"CSKRRoadNameSignBoth","CSKRRoadNameSignLeft","CSKRRoadNameSignRight"};
 internal static bool Valid(string name)=>Array.IndexOf(Names,name)>=0;
}
public partial class OverrideSystem {
 internal Entity RoadNamePrefab(string name){if(!RoadNameRules.Valid(name)||!_prefabs.TryGetPrefab(new PrefabID(nameof(StaticObjectPrefab),name),out var p)||!_prefabs.TryGetEntity(p,out var entity))return Entity.Null;return EntityManager.HasBuffer<SubMesh>(entity)?entity:Entity.Null;}
}
public partial class IndividualSignalData {
 internal bool SetRoadName(Entity display,IndividualSignalLink link,string name){
  if(!_ready||!EntityManager.Exists(display)||!EntityManager.HasComponent<PrefabRef>(display))return false;
  var over=World.GetExistingSystemManaged<OverrideSystem>();var prefab=EntityManager.GetComponentData<PrefabRef>(display).m_Prefab;
  if(!over.Accessories(prefab).Any(slot=>RoadNameRules.Valid(slot.Name))||(name!=""&&over.RoadNamePrefab(name)==Entity.Null))return false;
  var key=Key(link.Owner,link.Source);if(key==null)return false;
  var choice=Get(link.Owner,link.Source);if(choice==null)choice=new SignalChoice();
  // Clone all fields for legacy-key migration; keep independent near/far choices.
  choice=Newtonsoft.Json.JsonConvert.DeserializeObject<SignalChoice>(Newtonsoft.Json.JsonConvert.SerializeObject(choice));choice.key=key;
  if(link.Far)choice.farRoadName=name;else choice.nearRoadName=name;
  _choices[key]=choice;World.GetExistingSystemManaged<ApproachSignalSystem>()?.RefreshOwner(link.Owner);World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshOwner(link.Owner);return true;
 }
}
}
