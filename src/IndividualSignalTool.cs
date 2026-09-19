using System;using Game;using Game.Common;using Game.Objects;using Game.Prefabs;using Game.Tools;using Unity.Entities;using Unity.Jobs;
namespace TrafficLightIntoKorea {
public struct IndividualSignalLink:IComponentData {public Entity Source,Owner;public bool Far;}
public class IndividualSignalToolPrefab:PrefabBase {}
public partial class IndividualSignalTool:ToolBaseSystem {
 private PrefabBase _prefab;private Entity _hover;private bool _highlight;
 public override string toolID=>"TrafficLightIntoKorea.Individual";
 public override PrefabBase GetPrefab()=>_prefab;
 public override bool TrySetPrefab(PrefabBase p){if(!(p is IndividualSignalToolPrefab))return false;_prefab=p;return true;}
 public override void InitializeRaycast(){base.InitializeRaycast();m_ToolRaycastSystem.typeMask=_placing?TypeMask.Terrain|TypeMask.Net:TypeMask.StaticObjects;m_ToolRaycastSystem.collisionMask=CollisionMask.OnGround|CollisionMask.Overground;m_ToolRaycastSystem.raycastFlags|=RaycastFlags.SubElements;m_ToolRaycastSystem.raycastFlags&=~RaycastFlags.IgnoreSecondary;}
 protected override void OnStartRunning(){base.OnStartRunning();applyAction.shouldBeEnabled=true;cancelAction.shouldBeEnabled=true;World.GetExistingSystemManaged<IndividualSignalUI>()?.Install();}
 protected override void OnStopRunning(){CancelPlacement();Hover(Entity.Null);World.GetExistingSystemManaged<IndividualSignalUI>()?.Close();base.OnStopRunning();}
 private void Hover(Entity e){if(e==_hover)return;if(_highlight&&EntityManager.Exists(_hover)&&EntityManager.HasComponent<Highlighted>(_hover)){EntityManager.RemoveComponent<Highlighted>(_hover);if(!EntityManager.HasComponent<BatchesUpdated>(_hover))EntityManager.AddComponent<BatchesUpdated>(_hover);}_hover=e;_highlight=false;if(e!=Entity.Null&&!EntityManager.HasComponent<Highlighted>(e)){EntityManager.AddComponent<Highlighted>(e);if(!EntityManager.HasComponent<BatchesUpdated>(e))EntityManager.AddComponent<BatchesUpdated>(e);_highlight=true;}}
 private Entity Find(Entity e){for(int i=0;i<6&&EntityManager.Exists(e)&&!EntityManager.HasComponent<Deleted>(e)&&!EntityManager.HasComponent<KoreanRetired>(e);i++){if(EntityManager.HasComponent<IndividualSignalLink>(e))return e;var display=World.GetExistingSystemManaged<ApproachSignalSystem>()?.DisplayFor(e)??Entity.Null;if(display!=Entity.Null)return display;if(!EntityManager.HasComponent<Owner>(e))break;e=EntityManager.GetComponentData<Owner>(e).m_Owner;}return Entity.Null;}
 protected override JobHandle OnUpdate(JobHandle deps){deps.Complete();if(_placing)return PlacementTick(deps);var ui=World.GetExistingSystemManaged<IndividualSignalUI>();if(cancelAction.WasPressedThisFrame()){if(ui.Opened)ui.Close();else m_ToolSystem.activeTool=World.GetOrCreateSystemManaged<DefaultToolSystem>();return deps;}if(ui.Opened)return deps;Entity e=Entity.Null;if(GetRaycastResult(out Entity owner,out RaycastHit hit)){e=Find(hit.m_HitEntity);if(e==Entity.Null)e=Find(owner);}Hover(e);if(e!=Entity.Null&&applyAction.WasPressedThisFrame())ui.Open(e,EntityManager.GetComponentData<IndividualSignalLink>(e));return deps;}
}
}
