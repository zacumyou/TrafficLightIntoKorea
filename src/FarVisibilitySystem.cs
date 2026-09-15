using Game;using Game.Common;using Game.Objects;using Game.Rendering;using Game.Tools;using Unity.Collections;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
internal struct FarVisibilityRetry:IComponentData {internal float Next;internal int Attempts;}
// Our transient far-side displays receive persistent override protection without
// depending on Anarchy or modifying native objects/intentional near-side hiding.
public partial class FarVisibilitySystem:GameSystemBase {
 private EntityQuery _blocked,_far;private bool _ready;private float _next;private int _cursor;
 protected override void OnCreate(){base.OnCreate();
  _far=GetEntityQuery(ComponentType.ReadOnly<FarSignal>(),ComponentType.ReadOnly<KoreanSignalClone>(),ComponentType.ReadOnly<IndividualSignalLink>(),ComponentType.ReadOnly<Transform>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<KoreanRetired>(),ComponentType.Exclude<Temp>());
  _blocked=GetEntityQuery(new EntityQueryDesc{All=new[]{ComponentType.ReadOnly<FarSignal>(),ComponentType.ReadOnly<KoreanSignalClone>(),ComponentType.ReadOnly<IndividualSignalLink>()},Any=new[]{ComponentType.ReadOnly<Overridden>(),ComponentType.ReadOnly<Hidden>()},None=new[]{ComponentType.Exclude<Deleted>(),ComponentType.Exclude<KoreanRetired>(),ComponentType.Exclude<Temp>()}});
 }
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose p,GameMode mode){_ready=false;_next=0;_cursor=0;base.OnGamePreload(p,mode);}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose p,GameMode mode){base.OnGameLoadingComplete(p,mode);_ready=mode==GameMode.Game;}
 private bool Active(Entity e){var link=EntityManager.GetComponentData<IndividualSignalLink>(e);
  if(!link.Far||!EntityManager.Exists(link.Source)||EntityManager.HasComponent<Deleted>(link.Source)||EntityManager.HasComponent<Temp>(link.Source)||!EntityManager.Exists(link.Owner)||EntityManager.HasComponent<Deleted>(link.Owner)||EntityManager.HasComponent<Temp>(link.Owner)||!KoreanJunctionSystem.Selected(EntityManager,link.Owner))return false;
  int? mode=World.GetExistingSystemManaged<IndividualSignalData>()?.Get(link.Owner,link.Source)?.farMode;
  return RenderRecoveryRules.ProtectFar(Mod.Settings.AddFarSignals,mode);
 }
 private void Refresh(Entity e){if(!EntityManager.HasComponent<Updated>(e))EntityManager.AddComponent<Updated>(e);if(!EntityManager.HasComponent<BatchesUpdated>(e))EntityManager.AddComponent<BatchesUpdated>(e);}
 protected override void OnUpdate(){if(!_ready||Mod.Settings==null||!Mod.Settings.Enabled)return;
  using(var blocked=_blocked.ToEntityArray(Allocator.Temp))foreach(var e in blocked){if(!Active(e))continue;
   if(EntityManager.HasComponent<Overridden>(e))EntityManager.RemoveComponent<Overridden>(e);
   if(EntityManager.HasComponent<Hidden>(e))EntityManager.RemoveComponent<Hidden>(e);
   Refresh(e);RuntimeDiagnostics.Event("Far visibility protection restored "+e);
  }
  float now=UnityEngine.Time.realtimeSinceStartup;if(now<_next)return;_next=now+.5f;
  var camera=World.GetExistingSystemManaged<CameraUpdateSystem>();if(camera==null||!camera.TryGetLODParameters(out var parameters)||camera.activeViewer==null)return;
  var batches=World.GetExistingSystemManaged<BatchDataSystem>();var rendering=World.GetExistingSystemManaged<RenderingSystem>();if(batches==null||rendering==null)return;
  var lod=RenderingUtils.CalculateLodParameters(batches.GetLevelOfDetail(rendering.frameLod,camera.activeCameraController),parameters);
  using(var entities=_far.ToEntityArray(Allocator.Temp)){
   if(entities.Length==0){_cursor=0;return;}
   int count=math.min(64,entities.Length);
   for(int i=0;i<count;i++){
    if(_cursor>=entities.Length)_cursor=0;var e=entities[_cursor++];if(!Active(e))continue;
    bool needsRefresh=false;
    if(EntityManager.HasComponent<CullingInfo>(e)){
     var c=EntityManager.GetComponentData<CullingInfo>(e);
     float distance=RenderingUtils.CalculateMinDistance(c.m_Bounds,parameters.cameraPosition,camera.activeViewer.forward,lod);
     needsRefresh=c.m_PassedCulling==0&&RenderingUtils.CalculateLod(distance*distance,lod)>c.m_MinLod;
    }else needsRefresh=math.distancesq(EntityManager.GetComponentData<Transform>(e).m_Position,parameters.cameraPosition)<600f*600f;
    if(!needsRefresh)continue;
    var retry=EntityManager.HasComponent<FarVisibilityRetry>(e)?EntityManager.GetComponentData<FarVisibilityRetry>(e):default;
    if(now<retry.Next)continue;
    retry.Next=now+RenderRecoveryRules.RetryDelay(retry.Attempts);retry.Attempts=math.min(retry.Attempts+1,3);
    if(EntityManager.HasComponent<FarVisibilityRetry>(e))EntityManager.SetComponentData(e,retry);else EntityManager.AddComponentData(e,retry);
    Refresh(e);RuntimeDiagnostics.Event("Far culled-display recovery requested "+e+" stage="+retry.Attempts);
   }
  }
 }
}
}
