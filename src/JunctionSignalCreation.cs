using System.Collections.Generic;using Game.Common;using Game.Net;using Game.Prefabs;using Game.Tools;using Unity.Collections;using Unity.Entities;
namespace TrafficLightIntoKorea {
public partial class KoreanJunctionSystem {
 private readonly Dictionary<Entity,float> _creating=new Dictionary<Entity,float>();
 internal Entity CreationFailed;private float _failureUntil;
 internal bool Creating(Entity node)=>_creating.ContainsKey(node);
 internal bool Failed(Entity node)=>CreationFailed==node&&UnityEngine.Time.realtimeSinceStartup<_failureUntil;
 private void CreationFailure(Entity node,string reason){CreationFailed=node;_failureUntil=UnityEngine.Time.realtimeSinceStartup+4;RuntimeDiagnostics.Event("Native signal creation failed node="+node+" reason="+reason);}
 private bool BeginSignalCreation(Entity node){
  if(_creating.ContainsKey(node))return true;
  if(!SignalNodeTarget.Selectable(EntityManager,node)){CreationFailure(node,"not a junction");return false;}
  var roads=new List<Entity>();bool supported=false;
  using(var edges=EntityManager.GetBuffer<ConnectedEdge>(node,true).ToNativeArray(Allocator.Temp))foreach(var item in edges){
   var edge=item.m_Edge;if(!EntityManager.Exists(edge)||EntityManager.HasComponent<Deleted>(edge)||EntityManager.HasComponent<Temp>(edge)||!EntityManager.HasComponent<Edge>(edge)||!EntityManager.HasComponent<PrefabRef>(edge))continue;
   var prefab=EntityManager.GetComponentData<PrefabRef>(edge).m_Prefab;
   if(!EntityManager.HasComponent<RoadData>(prefab))continue;
   if(EntityManager.HasComponent<NetData>(prefab)&&(EntityManager.GetComponentData<NetData>(prefab).m_GeneralFlagMask&CompositionFlags.General.TrafficLights)!=0)supported=true;
   if(EntityManager.HasComponent<Composition>(edge)){
    var c=EntityManager.GetComponentData<Composition>(edge);var end=EntityManager.GetComponentData<Edge>(edge).m_Start==node?c.m_StartNode:c.m_EndNode;
    if(EntityManager.HasComponent<NetCompositionData>(end)&&(EntityManager.GetComponentData<NetCompositionData>(end).m_Flags.m_General&(CompositionFlags.General.Roundabout|CompositionFlags.General.LevelCrossing))!=0){CreationFailure(node,"unsupported junction type");return false;}
   }
   roads.Add(edge);
  }
  if(!supported||roads.Count<2){CreationFailure(node,"road does not support traffic lights");return false;}
  // Persist the native upgrade, not a fabricated TrafficLights component. Native
  // composition/lane/object systems retain control of validation and generation.
  var upgrade=EntityManager.HasComponent<Upgraded>(node)?EntityManager.GetComponentData<Upgraded>(node):default;
  upgrade.m_Flags.m_General=(upgrade.m_Flags.m_General&~(CompositionFlags.General.RemoveTrafficLights|CompositionFlags.General.AllWayStop))|CompositionFlags.General.TrafficLights;
  if(EntityManager.HasComponent<Upgraded>(node))EntityManager.SetComponentData(node,upgrade);else EntityManager.AddComponentData(node,upgrade);
  if(!EntityManager.HasComponent<Updated>(node))EntityManager.AddComponent<Updated>(node);
  foreach(var edge in roads)if(!EntityManager.HasComponent<Updated>(edge))EntityManager.AddComponent<Updated>(edge);
  _creating[node]=UnityEngine.Time.realtimeSinceStartup;CreationFailed=Entity.Null;
  RuntimeDiagnostics.Event("Native signal creation requested node="+node+" roads="+roads.Count);return true;
 }
 private void TickSignalCreation(){
  if(_creating.Count==0)return;
  foreach(var node in new List<Entity>(_creating.Keys)){
   if(!EntityManager.Exists(node)||EntityManager.HasComponent<Deleted>(node)||EntityManager.HasComponent<Temp>(node)){_creating.Remove(node);continue;}
   // A subsequent native removal wins over this pending style request.
   if(EntityManager.HasComponent<Upgraded>(node)&&(EntityManager.GetComponentData<Upgraded>(node).m_Flags.m_General&CompositionFlags.General.RemoveTrafficLights)!=0){_creating.Remove(node);continue;}
   if(EntityManager.HasComponent<TrafficLights>(node)){
    _creating.Remove(node);World.GetExistingSystemManaged<IndividualSignalData>()?.SetBulkNearOnly(node,false);Select(node,true);PlayBuildSound();RuntimeDiagnostics.Event("Native signal creation completed; Korean style selected node="+node);
   }else if(UnityEngine.Time.realtimeSinceStartup-_creating[node]>=5){_creating.Remove(node);CreationFailure(node,"native generation did not produce traffic lights");}
  }
 }
}
}
