using System.Collections.Generic;using Game;using Game.Common;using Game.Tools;using Unity.Entities;
namespace TrafficLightIntoKorea {
public struct KoreanRetired:IComponentData {}
// Rendering callbacks may discover stale sources, but entity deletion belongs before
// native object search/cleanup, not inside rendering or culling callbacks.
public partial class KoreanVisualLifecycleSystem:GameSystemBase {
 private readonly SignQueue<Entity> _pending=new SignQueue<Entity>(),_paced=new SignQueue<Entity>();private readonly HashSet<Entity> _queued=new HashSet<Entity>();private readonly List<Entity> _saveTemps=new List<Entity>();
 internal void Retire(Entity e,bool paced=false){if(e==Entity.Null||!EntityManager.Exists(e)||!_queued.Add(e))return;if(!EntityManager.HasComponent<KoreanRetired>(e))EntityManager.AddComponent<KoreanRetired>(e);
  // Native signal removal already rebuilds the road. Do not also request a hidden
  // render batch for every display immediately before deleting those same batches.
  if(paced){_paced.Enqueue(e);return;}
  if(!EntityManager.HasComponent<Hidden>(e))EntityManager.AddComponent<Hidden>(e);if(!EntityManager.HasComponent<BatchesUpdated>(e))EntityManager.AddComponent<BatchesUpdated>(e);_pending.Enqueue(e);}
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose p,GameMode m){_pending.Clear();_paced.Clear();_queued.Clear();_saveTemps.Clear();base.OnGamePreload(p,m);}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose p,GameMode m){_pending.Clear();_paced.Clear();_queued.Clear();_saveTemps.Clear();base.OnGameLoadingComplete(p,m);}
 protected override void OnUpdate(){for(int i=0;i<8&&_pending.Count>0;i++){var e=_pending.Dequeue();_queued.Remove(e);if(EntityManager.Exists(e)&&!EntityManager.HasComponent<Deleted>(e))EntityManager.AddComponent<Deleted>(e);}
  for(int i=0;i<8&&_paced.Count>0;i++){var e=_paced.Dequeue();_queued.Remove(e);if(!EntityManager.Exists(e)||EntityManager.HasComponent<Deleted>(e))continue;EntityManager.AddComponent<Deleted>(e);RuntimeDiagnostics.Count("nativeRemoval.displayDeleted");break;}}
 internal void BeforeSave(){foreach(var e in _queued)if(EntityManager.Exists(e)&&!EntityManager.HasComponent<Temp>(e)){EntityManager.AddComponent<Temp>(e);_saveTemps.Add(e);}}
 internal void AfterSave(){foreach(var e in _saveTemps)if(EntityManager.Exists(e)&&EntityManager.HasComponent<Temp>(e))EntityManager.RemoveComponent<Temp>(e);_saveTemps.Clear();}
}
}
