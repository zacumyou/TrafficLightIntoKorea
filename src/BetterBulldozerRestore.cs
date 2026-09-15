using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Common;
using Game.Objects;
using Game.Prefabs;
using Game.Tools;
using Unity.Entities;
using SubObject = Game.Objects.SubObject;

namespace TrafficLightIntoKorea {
// Optional adapter: only explicit default-restoration commands invoke this code.
internal static class BetterBulldozerRestore {
 private static Func<EntityManager,Entity,int> _release;
 private static Action<EntityManager,Entity> _cancel;
 private static bool _initialized;
 internal static bool Available {get {Initialize();return _release!=null;}}
 private static void Initialize(){if(_initialized)return;_initialized=true;
  foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()){
   var type=assembly.GetType("Better_Bulldozer.Components.PermanentlyRemovedSubElementPrefab",false);
   if(type==null)continue;
   var field=type.GetField("m_RecordEntity");
   if(!typeof(IBufferElementData).IsAssignableFrom(type)||field?.FieldType!=typeof(Entity))continue;
   try {
    _release=(Func<EntityManager,Entity,int>)typeof(BetterBulldozerRestore).GetMethod(nameof(CreateRelease),BindingFlags.NonPublic|BindingFlags.Static).MakeGenericMethod(type).Invoke(null,new object[]{field});
    var timer=assembly.GetType("Better_Bulldozer.Components.DeleteInXFrames",false);
    if(timer!=null&&typeof(IComponentData).IsAssignableFrom(timer))_cancel=(Action<EntityManager,Entity>)typeof(BetterBulldozerRestore).GetMethod(nameof(CreateCancel),BindingFlags.NonPublic|BindingFlags.Static).MakeGenericMethod(timer).Invoke(null,null);
    RuntimeDiagnostics.Event("Better Bulldozer restore adapter "+assembly.GetName().Version);
   }catch(Exception ex){_release=null;_cancel=null;RuntimeDiagnostics.Event("Better Bulldozer adapter unavailable: "+ex.GetType().Name);}
   return;
  }
 }
 internal static bool SignalPrefab(EntityManager em,Entity prefab)=>em.Exists(prefab)&&em.HasComponent<TrafficLightData>(prefab);
 private static Func<EntityManager,Entity,int> CreateRelease<T>(FieldInfo field) where T:unmanaged,IBufferElementData {
  return (em,owner)=>{
   if(!em.HasBuffer<T>(owner))return 0;
   var records=new List<Entity>();var buffer=em.GetBuffer<T>(owner);
   for(int i=buffer.Length-1;i>=0;i--){var record=(Entity)field.GetValue(buffer[i]);
    if(!em.Exists(record)||!em.HasComponent<PrefabRef>(record)||!SignalPrefab(em,em.GetComponentData<PrefabRef>(record).m_Prefab))continue;
    records.Add(record);buffer.RemoveAt(i);
   }
   // Finish buffer access before structural changes invalidate it.
   bool empty=buffer.Length==0;
   if(records.Count>0&&empty)em.RemoveComponent<T>(owner);
   foreach(var record in records)if(!em.HasComponent<Deleted>(record))em.AddComponent<Deleted>(record);
   return records.Count;
  };
 }
 private static Action<EntityManager,Entity> CreateCancel<T>() where T:unmanaged,IComponentData {
  return (em,e)=>{if(em.HasComponent<T>(e))em.RemoveComponent<T>(e);};
 }
 internal static bool CanRestore(EntityManager em,Entity node)=>em.Exists(node)&&!em.HasComponent<Deleted>(node)&&!em.HasComponent<Temp>(node)&&em.HasComponent<Game.Net.Node>(node)&&em.HasComponent<Game.Net.TrafficLights>(node)&&em.GetComponentData<Game.Net.TrafficLights>(node).m_Flags==0&&em.HasBuffer<Game.Net.ConnectedEdge>(node)&&em.GetBuffer<Game.Net.ConnectedEdge>(node,true).Length>=3;
 internal static bool Restore(EntityManager em,Entity node){
  if(!Available||!CanRestore(em,node))return false;
  em.CompleteAllTrackedJobs();
  int removed=_release(em,node);
  // BB may already have scheduled deletion of a surviving signal and its children.
  // Copy entity IDs first; cancelling components invalidates dynamic buffers.
  var signals=new List<Entity>();
  if(em.HasBuffer<SubObject>(node))foreach(var sub in em.GetBuffer<SubObject>(node,true)){
   var e=sub.m_SubObject;if(em.Exists(e)&&em.HasComponent<PrefabRef>(e)&&SignalPrefab(em,em.GetComponentData<PrefabRef>(e).m_Prefab))signals.Add(e);
  }
  foreach(var signal in signals)CancelTree(em,signal,0);
  if(!em.HasBuffer<SubObject>(node))em.AddBuffer<SubObject>(node);
  // Native subobject event regenerates missing props without toggling traffic
  // lights or marking the road/lane topology Updated.
  var request=em.CreateEntity(typeof(Event),typeof(SubObjectsUpdated));
  em.SetComponentData(request,new SubObjectsUpdated(node));
  if(removed>0)RuntimeDiagnostics.Event("BB signal records released="+removed+" node="+node);
  return true;
 }
 private static void CancelTree(EntityManager em,Entity e,int depth){
  if(depth>8||!em.Exists(e)||em.HasComponent<Deleted>(e)||em.HasComponent<Temp>(e))return;
  _cancel?.Invoke(em,e);
  var children=new List<Entity>();if(em.HasBuffer<SubObject>(e))foreach(var sub in em.GetBuffer<SubObject>(e,true))children.Add(sub.m_SubObject);
  foreach(var child in children)CancelTree(em,child,depth+1);
 }
}
}

