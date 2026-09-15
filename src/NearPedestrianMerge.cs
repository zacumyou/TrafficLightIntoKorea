using System.Collections.Generic;
using Game.Common;using Game.Objects;using Game.Prefabs;using Game.Tools;
using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
public partial class ApproachSignalSystem {
 private readonly Dictionary<Entity,Entry> _mergedPedestrians=new Dictionary<Entity,Entry>();
 internal bool ClaimsPedestrian(Entity e)=>_mergedPedestrians.ContainsKey(e);
 private bool MergeReady(Entry entry)=>Live(entry.Clone)&&Live(entry.Pedestrian)&&EntityManager.HasComponent<TrafficLight>(entry.Pedestrian)&&EntityManager.GetComponentData<TrafficLight>(entry.Pedestrian).m_GroupMask1==entry.PedestrianMask&&SignalRenderReadiness.MainReady(EntityManager,entry.Clone)&&!EntityManager.HasComponent<Hidden>(entry.Clone);
 private bool MergedPedestrianHidden(Entity e)=>_mergedPedestrians.TryGetValue(e,out var entry)&&MergeReady(entry);
 private void ReleasePedestrianMerge(Entry entry){
  var e=entry.Pedestrian;if(e!=Entity.Null&&_mergedPedestrians.TryGetValue(e,out var linked)&&linked==entry)_mergedPedestrians.Remove(e);
  if(entry.OwnsPedestrianHidden&&Live(e)&&EntityManager.HasComponent<Hidden>(e)){EntityManager.RemoveComponent<Hidden>(e);Mark(e);}
  entry.OwnsPedestrianHidden=false;entry.Pedestrian=Entity.Null;
 }
 // Called only by the existing budgeted junction rebuild, never by the frame sync.
 private void RebuildPedestrianMerges(Entity owner){
  var local=new List<KeyValuePair<Entity,Entry>>();foreach(var pair in _entries)if(pair.Value.Owner==owner)local.Add(pair);
  foreach(var pair in local)ReleasePedestrianMerge(pair.Value);
  local.Sort((a,b)=>a.Key.Index.CompareTo(b.Key.Index));
  var far=World.GetExistingSystemManaged<FarSignalSystem>();
  foreach(var pair in local){var source=pair.Key;var entry=pair.Value;if(!Live(source)||!IsCombined(entry.Prefab)||!EntityManager.HasComponent<TrafficLight>(source)||!EntityManager.HasComponent<Transform>(source))continue;
   var vehicle=EntityManager.GetComponentData<TrafficLight>(source);var origin=EntityManager.GetComponentData<Transform>(source).m_Position;Entity best=Entity.Null;float score=float.MaxValue;int mask=0;bool ambiguous=false;
   foreach(var item in EntityManager.GetBuffer<Game.Objects.SubObject>(owner,true)){
    var e=item.m_SubObject;if(e==source||ClaimsPedestrian(e)||(far?.ClaimsPedestrian(e)??false)||!Live(e)||EntityManager.HasComponent<Temp>(e)||EntityManager.HasComponent<KoreanSignalClone>(e)||!EntityManager.HasComponent<TrafficLight>(e)||!EntityManager.HasComponent<Transform>(e)||!EntityManager.HasComponent<PrefabRef>(e))continue;
    var prefab=EntityManager.GetComponentData<PrefabRef>(e).m_Prefab;if(!EntityManager.HasComponent<TrafficLightData>(prefab))continue;int type=(int)EntityManager.GetComponentData<TrafficLightData>(prefab).m_Type;if((type&3)!=0||(type&12)==0)continue;
    var pedestrian=EntityManager.GetComponentData<TrafficLight>(e);if(pedestrian.m_GroupMask0!=0)continue;
    var position=EntityManager.GetComponentData<Transform>(e).m_Position;float distance=math.distancesq(origin.xz,position.xz);
    if(!NearPedestrianMergeRules.Compatible(vehicle.m_GroupMask0,vehicle.m_GroupMask1,pedestrian.m_GroupMask1,distance,position.y-origin.y)||!PedestrianMergeGeometry.SameCrosswalkEnd(EntityManager,owner,origin,position,pedestrian.m_GroupMask1))continue;
    if(mask!=0&&mask!=pedestrian.m_GroupMask1)ambiguous=true;mask=pedestrian.m_GroupMask1;
    if(distance<score){score=distance;best=e;}
   }
   if(best==Entity.Null||ambiguous)continue;entry.Pedestrian=best;entry.PedestrianMask=EntityManager.GetComponentData<TrafficLight>(best).m_GroupMask1;_mergedPedestrians[best]=entry;
  }
 }
 private TrafficLight MergePedestrianState(Entry entry,TrafficLight native){
  var e=entry.Pedestrian;if(e!=Entity.Null){if(Live(e)&&EntityManager.HasComponent<TrafficLight>(e)){var pedestrian=EntityManager.GetComponentData<TrafficLight>(e);if(pedestrian.m_GroupMask1==entry.PedestrianMask&&(native.m_GroupMask1==0||native.m_GroupMask1==entry.PedestrianMask))return CombinedSignalRules.Merge(native,pedestrian);}ReleasePedestrianMerge(entry);Queue(entry.Owner);}
  return native;
 }
 private void SyncPedestrianMerge(Entry entry){
  var e=entry.Pedestrian;if(e==Entity.Null)return;bool hide=MergeReady(entry);
  if(hide&&!EntityManager.HasComponent<Hidden>(e)){EntityManager.AddComponent<Hidden>(e);entry.OwnsPedestrianHidden=true;Mark(e);}
  else if(!hide&&entry.OwnsPedestrianHidden){if(Live(e)&&EntityManager.HasComponent<Hidden>(e)){EntityManager.RemoveComponent<Hidden>(e);Mark(e);}entry.OwnsPedestrianHidden=false;}
 }
 private void SuspendPedestrianMerges(){foreach(var entry in _entries.Values){var e=entry.Pedestrian;if(entry.OwnsPedestrianHidden&&Live(e)&&EntityManager.HasComponent<Hidden>(e)){EntityManager.RemoveComponent<Hidden>(e);Mark(e);}entry.OwnsPedestrianHidden=false;}}
}
}
