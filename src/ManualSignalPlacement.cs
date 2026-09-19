using Game.Common;using Game.Objects;using Game.Prefabs;using Game.Tools;using Unity.Entities;using Unity.Jobs;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
internal static class ManualSignalPlacement {
 internal const float Radius=60f;
 internal static bool Valid(float3 origin,float3 point)=>FarOffsetRules.CanPlace(point.x-origin.x,point.y-origin.y,point.z-origin.z);
 internal static float3 Bound(float3 origin,float3 point){var delta=point.xz-origin.xz;float distance=math.length(delta);if(distance>Radius)point.xz=origin.xz+delta*(Radius/distance);return point;}
}
public partial class IndividualSignalData {
 internal bool PlaceManual(Entity owner,Entity source,float3 position,quaternion rotation,string asset){
  if(!_ready||!KoreanJunctionSystem.Selected(EntityManager,owner)||!EntityManager.HasComponent<Transform>(source)||!ManualSignalPlacement.Valid(EntityManager.GetComponentData<Transform>(source).m_Position,position)||System.Array.IndexOf(Allowed(),asset)<0)return false;
  var key=Key(owner,source);if(key==null)return false;var c=Get(owner,source)??new SignalChoice();c.key=key;c.farMode=1;c.manualPosition=new[]{position.x,position.y,position.z};c.manualRotation=new[]{rotation.value.x,rotation.value.y,rotation.value.z,rotation.value.w};c.farAsset=asset;c.farOffset=c.farLateral=c.farRotation=0;
  _choices[key]=c;HasForcedFar=true;World.GetExistingSystemManaged<FarSignalSystem>().RefreshOwner(owner);return true;
 }
}
public partial class FarSignalSystem {
 private bool TryManualPlan(Entity owner,Entity source,SignalChoice choice,out Plan plan){
  plan=null;var p=choice.manualPosition;var q=choice.manualRotation;if(p==null||p.Length!=3||q==null||q.Length!=4)return false;
  var origin=EntityManager.GetComponentData<Transform>(source).m_Position;var position=new float3(p[0],p[1],p[2]);var rotation=new quaternion(q[0],q[1],q[2],q[3]);if(!ManualSignalPlacement.Valid(origin,position)||!math.all(math.isfinite(rotation.value))||math.lengthsq(rotation.value)<.5f)return false;
  rotation=math.normalize(rotation);var axis=-math.rotate(rotation,new float3(0,0,1));position+=axis*FarOffsetRules.Clamp(choice.farOffset)+new float3(axis.z,0,-axis.x)*FarOffsetRules.Clamp(choice.farLateral);position=ManualSignalPlacement.Bound(origin,position);
  var data=World.GetExistingSystemManaged<IndividualSignalData>();var fallback=World.GetExistingSystemManaged<ApproachSignalSystem>().VehicleTarget(source);var prefab=data.Target(owner,source,fallback,true);if(prefab==Entity.Null||!EntityManager.HasComponent<ObjectData>(prefab))return false;
  plan=new Plan{Manual=true,Source=source,Prefab=prefab,Transform=new Transform{m_Position=position,m_Rotation=math.mul(quaternion.RotateY(math.radians(FarOffsetRules.FarRotation(choice.farRotation))),rotation)}};return true;
 }
}
public partial class IndividualSignalTool {
 private bool _placing;private IndividualSignalLink _placementLink;private string _placementAsset;private quaternion _placementRotation;
 internal void BeginPlacement(IndividualSignalLink link,Entity prefab,string asset){
  CancelPlacement();if(!EntityManager.Exists(link.Source)||!EntityManager.HasComponent<ObjectData>(prefab))return;
  var display=World.GetExistingSystemManaged<ApproachSignalSystem>().DisplayFor(link.Source);if(!EntityManager.HasComponent<Transform>(display))return;
  Hover(Entity.Null);_placementLink=link;_placementAsset=asset;_placementRotation=EntityManager.GetComponentData<Transform>(display).m_Rotation;
  World.GetOrCreateSystemManaged<ManualSignalPreviewSystem>().Begin(link.Owner,link.Source,prefab,_placementRotation);_placing=true;InitializeRaycast();
 }
 internal void CancelPlacement(){World.GetExistingSystemManaged<ManualSignalPreviewSystem>()?.Cancel();_placing=false;InitializeRaycast();}
 private JobHandle PlacementTick(JobHandle deps){
  if(cancelAction.WasPressedThisFrame()||!EntityManager.Exists(_placementLink.Source)||EntityManager.HasComponent<Deleted>(_placementLink.Source)||!KoreanJunctionSystem.Selected(EntityManager,_placementLink.Owner)){CancelPlacement();return deps;}
  var origin=EntityManager.GetComponentData<Transform>(_placementLink.Source).m_Position;bool hitFound=GetRaycastResult(out Entity owner,out RaycastHit hit);var point=hit.m_HitPosition;bool valid=hitFound&&ManualSignalPlacement.Valid(origin,point);
  var overlay=World.GetOrCreateSystemManaged<Game.Rendering.OverlayRenderSystem>();var buffer=overlay.GetBuffer(out var dependency);dependency.Complete();var color=valid?new UnityEngine.Color(.2f,.75f,1f,.5f):new UnityEngine.Color(1f,.3f,.25f,.5f);
  for(int i=0;i<96;i++){float a=i*math.PI*2/96,b=(i+.55f)*math.PI*2/96;var p=origin+new float3(math.cos(a)*60,.15f,math.sin(a)*60);var q=origin+new float3(math.cos(b)*60,.15f,math.sin(b)*60);buffer.DrawLine(color,new Colossal.Mathematics.Line3.Segment(p,q),.15f);}overlay.AddBufferWriter(default);
  World.GetExistingSystemManaged<ManualSignalPreviewSystem>()?.Move(point,hitFound);
  if(valid&&applyAction.WasPressedThisFrame()&&World.GetExistingSystemManaged<IndividualSignalData>().PlaceManual(_placementLink.Owner,_placementLink.Source,point,_placementRotation,_placementAsset))CancelPlacement();return deps;
 }
}
}
