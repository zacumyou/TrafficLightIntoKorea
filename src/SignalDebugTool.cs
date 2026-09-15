using Game;using Game.Tools;using Game.Common;using Game.Prefabs;using Unity.Entities;using Unity.Jobs;
namespace TrafficLightIntoKorea {
public partial class SignalDebugTool:ToolBaseSystem {
 private Entity _hover;private bool _ownsHover;
 public override string toolID=>"TrafficLightIntoKorea.Debug";
 public override PrefabBase GetPrefab()=>null;
 public override bool TrySetPrefab(PrefabBase prefab)=>false;
 internal static string LastReport="—";
 internal static void Start(){var w=World.DefaultGameObjectInjectionWorld;if(w==null||!w.IsCreated||Game.SceneFlow.GameManager.instance.gameMode!=GameMode.Game)return;w.GetOrCreateSystemManaged<ToolSystem>().activeTool=w.GetOrCreateSystemManaged<SignalDebugTool>();}
 internal static void Stop(){var w=World.DefaultGameObjectInjectionWorld;if(w==null||!w.IsCreated)return;var t=w.GetOrCreateSystemManaged<ToolSystem>();if(t.activeTool is SignalDebugTool)t.activeTool=w.GetOrCreateSystemManaged<DefaultToolSystem>();}
 public override void InitializeRaycast(){base.InitializeRaycast();m_ToolRaycastSystem.typeMask=TypeMask.StaticObjects|TypeMask.Net;m_ToolRaycastSystem.netLayerMask=Game.Net.Layer.Road;m_ToolRaycastSystem.collisionMask=CollisionMask.OnGround|CollisionMask.Overground;m_ToolRaycastSystem.raycastFlags|=RaycastFlags.SubElements;m_ToolRaycastSystem.raycastFlags&=~RaycastFlags.IgnoreSecondary;}
 protected override void OnStartRunning(){base.OnStartRunning();applyAction.shouldBeEnabled=true;cancelAction.shouldBeEnabled=true;}
 protected override void OnStopRunning(){SetHover(Entity.Null);base.OnStopRunning();}
 private void Mark(Entity e){if(!EntityManager.HasComponent<Game.Common.BatchesUpdated>(e))EntityManager.AddComponent<Game.Common.BatchesUpdated>(e);}
 private void SetHover(Entity e){if(e!=_hover){if(_ownsHover&&EntityManager.Exists(_hover)&&EntityManager.HasComponent<Highlighted>(_hover)){EntityManager.RemoveComponent<Highlighted>(_hover);Mark(_hover);}_hover=e;_ownsHover=false;}if(e!=Entity.Null&&EntityManager.Exists(e)&&!EntityManager.HasComponent<Highlighted>(e)){EntityManager.AddComponent<Highlighted>(e);_ownsHover=true;Mark(e);}}
 protected override JobHandle OnUpdate(JobHandle inputDeps){

 if(cancelAction.WasPressedThisFrame()){Stop();return inputDeps;}
 inputDeps.Complete();
 if(GetRaycastResult(out Entity owner,out RaycastHit hit)){
 var e=FindSignal(hit.m_HitEntity);if(e==Entity.Null)e=FindSignal(owner);
 bool signal=e!=Entity.Null;if(!signal){e=FindJunction(hit.m_HitEntity,hit.m_HitPosition);if(e==Entity.Null)e=FindJunction(owner,hit.m_HitPosition);}SetHover(e);
 if(e!=Entity.Null&&applyAction.WasPressedThisFrame()){try{var reports=World.GetExistingSystemManaged<LeftSignalSystem>();LastReport=signal?reports.WriteDebugReport(e,hit):reports.WriteJunctionReport(e,hit);}catch(System.Exception ex){LastReport="ERROR: "+ex.Message;}}
 }else SetHover(Entity.Null);
 return inputDeps;
 }
 private Entity FindSignal(Entity e){for(int i=0;i<5&&EntityManager.Exists(e);i++){if(EntityManager.HasComponent<Game.Objects.TrafficLight>(e))return e;if(!EntityManager.HasComponent<Owner>(e))break;e=EntityManager.GetComponentData<Owner>(e).m_Owner;}return Entity.Null;}
 private bool IsJunction(Entity e)=>EntityManager.Exists(e)&&!EntityManager.HasComponent<Deleted>(e)&&!EntityManager.HasComponent<Temp>(e)&&EntityManager.HasComponent<Game.Net.Node>(e)&&EntityManager.HasBuffer<Game.Net.ConnectedEdge>(e)&&EntityManager.GetBuffer<Game.Net.ConnectedEdge>(e,true).Length>=3;
 private Entity FindJunction(Entity e,Unity.Mathematics.float3 position){for(int i=0;i<5&&EntityManager.Exists(e);i++){if(IsJunction(e))return e;if(EntityManager.HasComponent<Game.Net.Edge>(e)){var edge=EntityManager.GetComponentData<Game.Net.Edge>(e);Entity best=Entity.Null;float distance=3600f;foreach(var node in new[]{edge.m_Start,edge.m_End})if(IsJunction(node)){float d=Unity.Mathematics.math.distancesq(EntityManager.GetComponentData<Game.Net.Node>(node).m_Position,position);if(d<distance){distance=d;best=node;}}return best;}if(!EntityManager.HasComponent<Owner>(e))break;e=EntityManager.GetComponentData<Owner>(e).m_Owner;}return Entity.Null;}
}
}


