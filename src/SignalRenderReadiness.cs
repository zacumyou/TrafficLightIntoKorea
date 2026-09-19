using Game.Common;using Game.Rendering;using Game.Tools;using Unity.Entities;
namespace TrafficLightIntoKorea {
internal struct KoreanRenderState:IComponentData {internal bool WasReady;internal float MissingSince;}
internal static class SignalRenderReadiness {
 internal static void Protect(EntityManager em,Entity e){if(!em.Exists(e)||(!em.HasComponent<KoreanSignalClone>(e)&&!em.HasComponent<KoreanSignVisual>(e))||em.HasComponent<Deleted>(e)||em.HasComponent<KoreanRetired>(e)||em.HasComponent<Temp>(e))return;if(em.HasComponent<Overridden>(e)){RuntimeDiagnostics.Count("render.overrideRemoved");em.RemoveComponent<Overridden>(e);if(!em.HasComponent<Updated>(e))em.AddComponent<Updated>(e);if(!em.HasComponent<BatchesUpdated>(e))em.AddComponent<BatchesUpdated>(e);RuntimeDiagnostics.Event("Cleared overlap override on Korean display "+e);}}
 internal static bool Allocated(EntityManager em,Entity e){if(!em.Exists(e)||!em.HasBuffer<MeshBatch>(e))return false;foreach(var b in em.GetBuffer<MeshBatch>(e,true))if(b.m_MeshGroup==0&&b.m_MeshIndex==0&&b.m_GroupIndex>=0&&b.m_InstanceIndex>=0)return true;return false;}
 private static bool Visible(EntityManager em,Entity e)=>!em.HasComponent<Hidden>(e)&&em.HasComponent<CullingInfo>(e)&&em.GetComponentData<CullingInfo>(e).m_PassedCulling!=0;
 internal static bool MainReady(EntityManager em,Entity e){if(Allocated(em,e))return true;if(!em.Exists(e)||!em.HasComponent<KoreanRenderState>(e))return false;var state=em.GetComponentData<KoreanRenderState>(e);return RenderRecoveryRules.KeepCustom(state.WasReady,Visible(em,e),UnityEngine.Time.realtimeSinceStartup-state.MissingSince);}
 internal static void Repair(EntityManager em,Entity e,ref int attempts,ref float next){
  Protect(em,e);if(!em.Exists(e)||em.HasComponent<Deleted>(e)||em.HasComponent<KoreanRetired>(e)||em.HasComponent<Temp>(e))return;
  if(!em.HasComponent<KoreanRenderState>(e))em.AddComponentData(e,new KoreanRenderState{MissingSince=-1});
  var state=em.GetComponentData<KoreanRenderState>(e);float now=UnityEngine.Time.realtimeSinceStartup;
  if(Allocated(em,e)){if(attempts>0)RuntimeDiagnostics.Event("Custom mesh recovered: clone="+e);attempts=0;next=0;if(!state.WasReady||state.MissingSince!=-1){state.WasReady=true;state.MissingSince=-1;em.SetComponentData(e,state);RuntimeDiagnostics.Count("render.stateWrite");}return;}
  if(!Visible(em,e)){if(state.MissingSince!=-1){state.MissingSince=-1;em.SetComponentData(e,state);RuntimeDiagnostics.Count("render.stateWrite");}return;}
  if(state.MissingSince<0){state.MissingSince=now;em.SetComponentData(e,state);RuntimeDiagnostics.Count("render.stateWrite");}
  if(now<next)return;next=now+RenderRecoveryRules.RetryDelay(attempts);RuntimeDiagnostics.Count("render.recovery");attempts=System.Math.Min(attempts+1,3);
  if(!em.HasComponent<BatchesUpdated>(e))em.AddComponent<BatchesUpdated>(e);
  if(!em.HasComponent<Updated>(e))em.AddComponent<Updated>(e);
  RuntimeDiagnostics.Event("Custom mesh recovery requested: clone="+e+" retryStage="+attempts);
 }
 }
}
