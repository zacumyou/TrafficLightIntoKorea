using Game;
using Game.Common;
using Game.Objects;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
namespace TrafficLightIntoKorea {
// Finite startup reconciliation of missing batches. Never marks road nodes Updated,
// changes native prefab identity, deletes objects, or touches rendering allocations.
public partial class SignalRenderRecoverySystem:GameSystemBase {
 private EntityQuery _query;private NativeArray<Entity> _snapshot;private int _cursor,_pass,_reported;private float _next;private bool _ready;
 protected override void OnCreate(){base.OnCreate();_query=GetEntityQuery(ComponentType.ReadOnly<TrafficLight>(),ComponentType.ReadOnly<PrefabRef>(),ComponentType.ReadOnly<Transform>(),ComponentType.ReadOnly<MeshBatch>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>());}
 private void DisposeSnapshot(){if(_snapshot.IsCreated)_snapshot.Dispose();}
 protected override void OnDestroy(){DisposeSnapshot();base.OnDestroy();}
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose p,GameMode m){DisposeSnapshot();_ready=false;_cursor=_pass=_reported=0;base.OnGamePreload(p,m);}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose p,GameMode m){base.OnGameLoadingComplete(p,m);_ready=m==GameMode.Game;_next=UnityEngine.Time.realtimeSinceStartup+3;}
 protected override void OnUpdate(){if(!_ready||Mod.Settings==null||!Mod.Settings.Enabled||_pass>=3)return;float now=UnityEngine.Time.realtimeSinceStartup;if(now<_next)return;
  using(RuntimeDiagnostics.Measure("SignalRenderRecoverySystem")){
   if(!_snapshot.IsCreated){_snapshot=_query.ToEntityArray(Allocator.Persistent);_cursor=0;}
   for(int i=0;i<32&&_cursor<_snapshot.Length;i++){
    var e=_snapshot[_cursor++];if(!EntityManager.Exists(e)||EntityManager.HasComponent<Deleted>(e)||EntityManager.HasComponent<Temp>(e)||!EntityManager.HasBuffer<MeshBatch>(e)||!EntityManager.HasComponent<PrefabRef>(e))continue;
    var prefab=EntityManager.GetComponentData<PrefabRef>(e).m_Prefab;
    if(!World.GetExistingSystemManaged<OverrideSystem>().IsOverridden(prefab)&&!EntityManager.HasComponent<ApproachVisual>(e)&&!EntityManager.HasComponent<FarSignal>(e))continue;
    if(EntityManager.GetBuffer<MeshBatch>(e,true).Length!=0||EntityManager.HasComponent<Hidden>(e)||!EntityManager.HasBuffer<SubMesh>(prefab)||EntityManager.GetBuffer<SubMesh>(prefab,true).Length==0)continue;
    if(_reported++<64){var c=EntityManager.HasComponent<CullingInfo>(e)?EntityManager.GetComponentData<CullingInfo>(e):default;RuntimeDiagnostics.Event("missing-body pass="+_pass+" entity="+e+" prefab="+prefab+" cullingIndex="+c.m_CullingIndex+" bounds="+c.m_Bounds+" created="+EntityManager.HasComponent<Created>(e));}
    if(!EntityManager.HasComponent<BatchesUpdated>(e))EntityManager.AddComponent<BatchesUpdated>(e);
   }
   if(_cursor>=_snapshot.Length){DisposeSnapshot();_pass++;_next=now+10;}
  }
 }
}
}
