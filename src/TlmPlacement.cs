using System.Collections.Generic;using Game;using Game.Common;using Game.Net;using Game.Prefabs;using Game.Tools;using Unity.Entities;using Unity.Collections;
using SubLane=Game.Net.SubLane;using CarLane=Game.Net.CarLane;
namespace TrafficLightIntoKorea {
public partial class TlmCompatibilitySystem {
 private readonly Dictionary<Entity,LaneSignal> _placementSignals=new Dictionary<Entity,LaneSignal>();
 private readonly HashSet<Entity> _placementOwners=new HashSet<Entity>();
 // Only surrounds SecondaryObjectSystem. TLM/TTE initialization has already completed;
 // simulation never sees the temporary placement eligibility mask.
 internal void BeginPlacement(){
  if(!_ready||!_available||Mod.Settings==null||!Mod.Settings.Enabled)return;
  EndPlacement();EntityManager.CompleteDependencyBeforeRW<LaneSignal>();
  try{using(var nodes=_changed.ToEntityArray(Allocator.Temp))foreach(var owner in nodes){
   foreach(var item in EntityManager.GetBuffer<SubLane>(owner,true)){
    var lane=item.m_SubLane;if(!EntityManager.Exists(lane)||EntityManager.HasComponent<Deleted>(lane)||EntityManager.HasComponent<Temp>(lane)||EntityManager.HasComponent<MasterLane>(lane)||!EntityManager.HasComponent<CarLane>(lane)||!EntityManager.HasComponent<LaneSignal>(lane)||(EntityManager.GetComponentData<CarLane>(lane).m_Flags&CarLaneFlags.Forbidden)!=0)continue;
    var original=EntityManager.GetComponentData<LaneSignal>(lane);if(original.m_GroupMask!=0)continue;
    _placementSignals[lane]=original;var placement=original;placement.m_GroupMask=TlmPlacementRules.PresenceMask(original.m_GroupMask);EntityManager.SetComponentData(lane,placement);_placementOwners.Add(owner);
   }
  }}catch{EndPlacement();throw;}
 }
 internal void EndPlacement(){
  if(_placementSignals.Count==0)return;
  EntityManager.CompleteDependencyBeforeRW<LaneSignal>();
  foreach(var pair in _placementSignals)if(EntityManager.Exists(pair.Key)&&EntityManager.HasComponent<LaneSignal>(pair.Key))EntityManager.SetComponentData(pair.Key,pair.Value);
  _placementSignals.Clear();
 }
 // SecondaryObjectSystem records its new objects in ModificationBarrier4B.
 // Repair their DISPLAY masks in Modification5, after that barrier has played back.
 internal void RepairPlacement(){
  EndPlacement();if(_placementOwners.Count==0)return;
  EntityManager.CompleteDependencyBeforeRW<Game.Objects.TrafficLight>();
  foreach(var owner in _placementOwners){
   if(!EntityManager.Exists(owner)||EntityManager.HasComponent<Deleted>(owner)||!EntityManager.HasComponent<TrafficLights>(owner)||!EntityManager.HasBuffer<Game.Objects.SubObject>(owner))continue;
   var phase=EntityManager.GetComponentData<TrafficLights>(owner);
   foreach(var item in EntityManager.GetBuffer<Game.Objects.SubObject>(owner,true)){
    var e=item.m_SubObject;if(!EntityManager.Exists(e)||EntityManager.HasComponent<Deleted>(e)||EntityManager.HasComponent<Temp>(e)||!EntityManager.HasComponent<Game.Objects.TrafficLight>(e)||!EntityManager.HasComponent<Game.Objects.Transform>(e)||!EntityManager.HasComponent<PrefabRef>(e))continue;
    var prefab=EntityManager.GetComponentData<PrefabRef>(e).m_Prefab;if(!EntityManager.HasComponent<TrafficLightData>(prefab)||((int)EntityManager.GetComponentData<TrafficLightData>(prefab).m_Type&3)==0)continue;
    ushort mask=0;
    if(SignalApproach.Try(EntityManager,owner,EntityManager.GetComponentData<Game.Objects.Transform>(e),out int road,out var direction))foreach(var sub in EntityManager.GetBuffer<SubLane>(owner,true)){
     var lane=sub.m_SubLane;if(!EntityManager.Exists(lane)||EntityManager.HasComponent<Deleted>(lane)||EntityManager.HasComponent<Temp>(lane)||EntityManager.HasComponent<MasterLane>(lane)||!EntityManager.HasComponent<Lane>(lane)||!EntityManager.HasComponent<CarLane>(lane)||!EntityManager.HasComponent<LaneSignal>(lane)||(EntityManager.GetComponentData<CarLane>(lane).m_Flags&CarLaneFlags.Forbidden)!=0)continue;
     if(EntityManager.GetComponentData<Lane>(lane).m_StartNode.GetOwnerIndex()==road)mask|=EntityManager.GetComponentData<LaneSignal>(lane).m_GroupMask;
    }
    var state=EntityManager.GetComponentData<Game.Objects.TrafficLight>(e);state.m_GroupMask0=mask;Game.Simulation.TrafficLightSystem.UpdateTrafficLightState(phase,ref state);state.m_State=(Game.Objects.TrafficLightState)TlmPlacementRules.DisplayState(mask,(int)state.m_State);EntityManager.SetComponentData(e,state);
   }
   World.GetExistingSystemManaged<ApproachSignalSystem>()?.RefreshOwner(owner);World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshOwner(owner);World.GetExistingSystemManaged<LeftSignalSystem>()?.RefreshOwner(owner);World.GetExistingSystemManaged<DirectionalLightSystem>()?.RefreshOwner(owner);
  }
  _placementOwners.Clear();
 }
 protected override void OnDestroy(){EndPlacement();base.OnDestroy();}
}
internal static class TlmPlacementRules {
 internal static ushort PresenceMask(ushort original)=>original==0?ushort.MaxValue:original;
 internal static int DisplayState(ushort actualMask,int state)=>actualMask==0?(state&~15)|1:state;
}
public partial class TlmPlacementRepairSystem:GameSystemBase {protected override void OnUpdate(){World.GetExistingSystemManaged<TlmCompatibilitySystem>()?.RepairPlacement();}}
}
