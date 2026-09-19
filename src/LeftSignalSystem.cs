using System.Collections.Generic;
using Game;using Game.Common;using Game.Objects;using Game.Prefabs;using Game.Tools;using Game.Rendering;
using Unity.Entities;using Unity.Collections;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
public partial class LeftSignalSystem:GameSystemBase {
 private EntityQuery _signals,_changedOwners,_changedSignals;
 private readonly HashSet<Entity> _owned=new HashSet<Entity>(),_queued=new HashSet<Entity>();
 private readonly List<Entity> _queue=new List<Entity>();private int _head;
 private readonly List<Entity> _tracked=new List<Entity>();private int _repairCursor;
 private bool _active,_ready;
 internal static int Examined,HiddenCount;
 protected override void OnCreate(){base.OnCreate();
 _signals=GetEntityQuery(ComponentType.ReadOnly<Game.Objects.TrafficLight>(),ComponentType.ReadOnly<Transform>(),ComponentType.ReadOnly<PrefabRef>(),ComponentType.Exclude<ApproachVisual>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>());
 _changedSignals=GetEntityQuery(new EntityQueryDesc{All=new[]{ComponentType.ReadOnly<Game.Objects.TrafficLight>(),ComponentType.ReadOnly<Owner>()},Any=new[]{ComponentType.ReadOnly<Updated>(),ComponentType.ReadOnly<Created>()},None=new[]{ComponentType.ReadOnly<ApproachVisual>(),ComponentType.ReadOnly<Deleted>(),ComponentType.ReadOnly<Temp>()}});
 _changedOwners=GetEntityQuery(ComponentType.ReadOnly<Game.Net.SubLane>(),ComponentType.ReadOnly<Game.Objects.SubObject>(),ComponentType.ReadOnly<Updated>(),ComponentType.Exclude<ApproachVisual>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>());
 }
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose,GameMode mode){Reset();_ready=false;base.OnGamePreload(purpose,mode);}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose,GameMode mode){base.OnGameLoadingComplete(purpose,mode);_ready=mode==GameMode.Game;_active=false;Examined=0;HiddenCount=0;}
 internal void CollectOwners(HashSet<Entity> owners){foreach(var e in _owned){var owner=LightOwner(e);if(owner!=Entity.Null)owners.Add(owner);}}
 internal void RemoveOwner(Entity owner){var remove=new List<Entity>();foreach(var e in _owned)if(LightOwner(e)==owner)remove.Add(e);foreach(var e in remove){Show(e);_owned.Remove(e);}}
 internal void RefreshOwner(Entity e){Enqueue(e);}
 internal bool IsReplaced(Entity e)=>EntityManager.HasComponent<PrefabRef>(e)&&((World.GetExistingSystemManaged<OverrideSystem>()?.IsOverridden(EntityManager.GetComponentData<PrefabRef>(e).m_Prefab)??false)||(World.GetExistingSystemManaged<ApproachSignalSystem>()?.IsVisualSource(e)??false));
 internal bool IsHiddenTarget(Entity e)=>_active&&_owned.Contains(e);
 internal void Reset(){foreach(var e in _owned)Show(e);_owned.Clear();_tracked.Clear();_repairCursor=0;_queue.Clear();_head=0;_queued.Clear();_active=false;Examined=0;HiddenCount=0;}
 private void Mark(Entity e){if(!EntityManager.HasComponent<BatchesUpdated>(e))EntityManager.AddComponent<BatchesUpdated>(e);World.GetExistingSystemManaged<TrafficSignSystem>()?.SyncParent(e);}
 private void Show(Entity e){if(EntityManager.Exists(e)&&PersistentSignalVisibility.HiddenOrOwned(EntityManager,e)){PersistentSignalVisibility.Show(EntityManager,e);Mark(e);}}
 private void Enqueue(Entity e){if(e!=Entity.Null&&_queued.Add(e))_queue.Add(e);}
 protected override void OnUpdate(){using(RuntimeDiagnostics.Measure("LeftSignalSystem.OnUpdate")){
 bool enabled=_ready&&Mod.Settings!=null&&Mod.Settings.Enabled&&Mod.Settings.HideLeftReplacedLights&&!World.GetOrCreateSystemManaged<Game.City.CityConfigurationSystem>().leftHandTraffic;
 if(!enabled){if(_active)Reset();return;}
 _active=true;
 // Bound work to sixteen junctions per update. No recurring full-city scan.
 for(int i=0;i<16&&_head<_queue.Count;i++){var owner=_queue[_head++];_queued.Remove(owner);Evaluate(owner);}if(_head==_queue.Count){_queue.Clear();_head=0;}
 RepairHidden();

 }}
 // Tools can clear Hidden without adding Updated to the original signal.
 // Inspect only our tracked targets, at most 64 each frame; never scan the city here.
 private void RepairHidden(){
 int count=System.Math.Min(64,_tracked.Count);
 for(int i=0;i<count&&_tracked.Count>0;i++){
  if(_repairCursor>=_tracked.Count)_repairCursor=0;
  var e=_tracked[_repairCursor];
  if(!_owned.Contains(e)||!EntityManager.Exists(e)||EntityManager.HasComponent<Deleted>(e)){
   _owned.Remove(e);_tracked[_repairCursor]=_tracked[_tracked.Count-1];_tracked.RemoveAt(_tracked.Count-1);continue;
  }
  _repairCursor++;
  if(EntityManager.HasComponent<Hidden>(e))continue;
  // Recheck the current pair before reinstating a flag that another tool cleared.
  var overrides=World.GetExistingSystemManaged<OverrideSystem>();
  bool valid=EntityManager.HasComponent<PrefabRef>(e)&&EntityManager.HasComponent<Transform>(e)&&!EntityManager.HasComponent<Temp>(e)&&IsReplaced(e)&&HasRightReplacement(e);
  if(valid){PersistentSignalVisibility.Hide(EntityManager,e);Mark(e);}else _owned.Remove(e);
 }
 HiddenCount=_owned.Count;
 }
 private void Evaluate(Entity owner){if(!KoreanJunctionSystem.Selected(EntityManager,owner)){RemoveOwner(owner);return;}
 if(!EntityManager.Exists(owner)||!EntityManager.HasBuffer<Game.Objects.SubObject>(owner))return;
 Entity[] children;using(var a=EntityManager.GetBuffer<Game.Objects.SubObject>(owner,true).ToNativeArray(Allocator.Temp)){children=new Entity[a.Length];for(int i=0;i<a.Length;i++)children[i]=a[i].m_SubObject;}
 var overrides=World.GetExistingSystemManaged<OverrideSystem>();
 foreach(var e in children){if(!EntityManager.Exists(e))continue;
 if(EntityManager.HasComponent<Game.Objects.TrafficLight>(e))Examined++;
 bool hide=!EntityManager.HasComponent<Deleted>(e)&&!EntityManager.HasComponent<Temp>(e)&&EntityManager.HasComponent<Transform>(e)&&EntityManager.HasComponent<PrefabRef>(e)&&EntityManager.HasComponent<Game.Objects.TrafficLight>(e)&&IsReplaced(e)&&HasRightReplacement(e);
 if(hide){if(_owned.Add(e))_tracked.Add(e);if(!EntityManager.HasComponent<Hidden>(e)){PersistentSignalVisibility.Hide(EntityManager,e);Mark(e);}}
 else if(_owned.Remove(e))Show(e);
 HiddenCount=_owned.Count;
 }
 }
 private Entity LightOwner(Entity e) {
  for(int i=0;i<4 && EntityManager.HasComponent<Owner>(e);i++) {
   e=EntityManager.GetComponentData<Owner>(e).m_Owner;
   if(EntityManager.HasBuffer<Game.Net.SubLane>(e) && EntityManager.HasBuffer<Game.Objects.SubObject>(e))return e;
  }return Entity.Null;
 }
 internal bool HasRightReplacement(Entity instance) {
  if(!EntityManager.HasComponent<Game.Objects.TrafficLight>(instance))return false;
  var source=EntityManager.GetComponentData<PrefabRef>(instance).m_Prefab;
  if(!EntityManager.HasComponent<TrafficLightData>(source)||((int)EntityManager.GetComponentData<TrafficLightData>(source).m_Type&3)==0)return false;
  var owner=LightOwner(instance);if(owner==Entity.Null)return false;
  var t=EntityManager.GetComponentData<Transform>(instance);
  int mask=EntityManager.GetComponentData<Game.Objects.TrafficLight>(instance).m_GroupMask0;
  if(!SignalApproach.Try(EntityManager,owner,t,out int ownRoad,out var direction))return false;
  var right=new float2(direction.y,-direction.x);

  // Read-only pair evaluation; structural changes happen after snapshots are disposed.
  using(var children=EntityManager.GetBuffer<Game.Objects.SubObject>(owner,true).ToNativeArray(Allocator.Temp))foreach(var child in children) {
   var other=child.m_SubObject;if(EntityManager.HasComponent<Hidden>(other)&&!_owned.Contains(other)&&!(World.GetExistingSystemManaged<ApproachSignalSystem>()?.IsVisualSource(other)??false))continue;if(other==instance || EntityManager.HasComponent<Deleted>(other) || EntityManager.HasComponent<Temp>(other))continue;
   if(!EntityManager.HasComponent<Game.Objects.TrafficLight>(other) || !EntityManager.HasComponent<Transform>(other) || !EntityManager.HasComponent<PrefabRef>(other))continue;
   var ot=EntityManager.GetComponentData<Transform>(other);var delta=ot.m_Position-t.m_Position;
   int otherMask=EntityManager.GetComponentData<Game.Objects.TrafficLight>(other).m_GroupMask0;
   if(!SignalApproach.Try(EntityManager,owner,ot,out int otherRoad,out var otherDirection)||!SignalApproachRules.SameRoad(ownRoad,otherRoad))continue;
   if(!LightPairRules.RightPartner(math.dot(delta.xz,right),math.dot(delta.xz,direction),delta.y,math.dot(direction,otherDirection),1,1))continue;
   var original=EntityManager.GetComponentData<PrefabRef>(other).m_Prefab;
   if(!IsReplaced(other))continue;
   return true;
  }
  return false;
 }
 internal bool TryApproach(Entity owner,float3 position,int mask,out float2 direction){

  float best=6400f;direction=0;bool ambiguous=false;
  foreach(var item in EntityManager.GetBuffer<Game.Net.SubLane>(owner,true)) {
   var lane=item.m_SubLane;
   if(EntityManager.HasComponent<Deleted>(lane)||EntityManager.HasComponent<Temp>(lane)||!EntityManager.HasComponent<Game.Net.CarLane>(lane) || !EntityManager.HasComponent<Game.Net.LaneSignal>(lane) || !EntityManager.HasComponent<Game.Net.Curve>(lane))continue;
   if((EntityManager.GetComponentData<Game.Net.LaneSignal>(lane).m_GroupMask & mask)==0)continue;
   var curve=EntityManager.GetComponentData<Game.Net.Curve>(lane).m_Bezier;
   var dir=math.normalizesafe((curve.b-curve.a).xz);if(math.lengthsq(dir)<0.9f)continue;
   float distance=math.distancesq(curve.a.xz,position.xz);
   if(distance<best-0.01f){best=distance;direction=dir;ambiguous=false;}
   else if(math.abs(distance-best)<0.01f && math.dot(direction,dir)<0.98f)ambiguous=true;
  }
  return !ambiguous&&math.lengthsq(direction)>=0.9f;
 }

}
}


