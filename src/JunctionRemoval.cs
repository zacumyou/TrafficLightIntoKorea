using System.Collections.Generic;using Game.Common;using Game.Objects;using Game.Tools;using Unity.Collections;using Unity.Entities;
namespace TrafficLightIntoKorea {
public partial class KoreanJunctionSystem {
 private readonly SignQueue<Entity> _nativeRemoved=new SignQueue<Entity>();
 internal void NativeSignalsRemoved(Entity node){
  if(!_ready||!SignalSelectionRules.NativeRemoval(Selected(EntityManager,node),EntityManager.HasComponent<Game.Net.Node>(node),EntityManager.HasComponent<Game.Net.TrafficLights>(node),EntityManager.HasComponent<Temp>(node)))return;
  // Clear the persisted selection immediately, even if the native tool is toggled on again.
  World.GetExistingSystemManaged<IndividualSignalData>()?.ForgetNode(node);
  EntityManager.RemoveComponent<KoreanJunction>(node);_deferred.Remove(node);_nativeRemoved.Enqueue(node);
  RuntimeDiagnostics.Event("Native signals removed; Korean selection cleared node="+node);
 }
 private void RestoreOwnerVisuals(Entity node){
  var native=new HashSet<Entity>();var clones=new List<Entity>();
  // Include untracked displays, e.g. after an interrupted rebuild. Only this owner's displays retire.
  using(var query=EntityManager.CreateEntityQuery(ComponentType.ReadOnly<KoreanSignalClone>(),ComponentType.ReadOnly<IndividualSignalLink>(),ComponentType.Exclude<Deleted>()))
  using(var entities=query.ToEntityArray(Allocator.Temp))foreach(var entity in entities){var link=EntityManager.GetComponentData<IndividualSignalLink>(entity);if(link.Owner!=node)continue;clones.Add(entity);native.Add(link.Source);}
  var lifecycle=World.GetOrCreateSystemManaged<KoreanVisualLifecycleSystem>();
  foreach(var entity in clones){World.GetExistingSystemManaged<TrafficSignSystem>()?.Forget(entity);World.GetExistingSystemManaged<DirectionalLightSystem>()?.Unlink(entity);lifecycle.Retire(entity);}
  if(EntityManager.Exists(node)&&EntityManager.HasBuffer<SubObject>(node))foreach(var sub in EntityManager.GetBuffer<SubObject>(node,true))native.Add(sub.m_SubObject);
  var seen=new HashSet<Entity>();foreach(var source in native)RestoreNativeTree(source,seen,0);
  RuntimeDiagnostics.Event("Junction default restored node="+node+" remainingSelection="+Selected(EntityManager,node)+" displaysRetired="+clones.Count);
 }
 private void RestoreNativeTree(Entity entity,HashSet<Entity> seen,int depth){
  if(depth>=8||!seen.Add(entity)||!EntityManager.Exists(entity)||EntityManager.HasComponent<Deleted>(entity)||EntityManager.HasComponent<Temp>(entity)||EntityManager.HasComponent<KoreanSignalClone>(entity)||EntityManager.HasComponent<KoreanSignVisual>(entity))return;
  var children=new List<Entity>();if(EntityManager.HasBuffer<SubObject>(entity))foreach(var sub in EntityManager.GetBuffer<SubObject>(entity,true))children.Add(sub.m_SubObject);
  // Release only TLIK-owned hiding; never undo another mod's intentional hides.
  if(EntityManager.HasComponent<KoreanPersistentHidden>(entity)){PersistentSignalVisibility.Show(EntityManager,entity);if(!EntityManager.HasComponent<BatchesUpdated>(entity))EntityManager.AddComponent<BatchesUpdated>(entity);}
  foreach(var child in children)RestoreNativeTree(child,seen,depth+1);
 }
}
}
