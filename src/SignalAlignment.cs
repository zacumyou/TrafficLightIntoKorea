using Game.Common;using Game.Objects;using Game.Prefabs;using Game.Tools;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
internal static class SignalAlignment {
 internal static Transform Align(EntityManager em,Entity owner,Entity source,Transform display){
  if(!em.HasComponent<PrefabRef>(source)||!em.HasComponent<Transform>(source))return display;
  var prefab=em.GetComponentData<PrefabRef>(source).m_Prefab;
  if(!em.HasComponent<TrafficLightData>(prefab)||((int)em.GetComponentData<TrafficLightData>(prefab).m_Type&3)==0)return display;
  var original=em.GetComponentData<Transform>(source);
  if(!SignalApproach.Try(em,owner,original,out _,out var approach))return display;
  var facing=-approach;float best=144f;
  // Use the nearby crossing's width axis; preserve which side the signal faces.
  foreach(var sub in em.GetBuffer<Game.Net.SubLane>(owner,true)){
   var lane=sub.m_SubLane;if(!em.Exists(lane)||em.HasComponent<Deleted>(lane)||em.HasComponent<Temp>(lane)||!em.HasComponent<Game.Net.PedestrianLane>(lane)||!em.HasComponent<Game.Net.Curve>(lane))continue;
   if((em.GetComponentData<Game.Net.PedestrianLane>(lane).m_Flags&Game.Net.PedestrianLaneFlags.Crosswalk)==0)continue;
   var curve=em.GetComponentData<Game.Net.Curve>(lane).m_Bezier;if(math.min(math.abs(display.m_Position.y-curve.a.y),math.abs(display.m_Position.y-curve.d.y))>3f)continue;var span=curve.d.xz-curve.a.xz;if(math.lengthsq(span)<1)continue;
   var axis=math.normalize(span);var normal=new float2(-axis.y,axis.x);float agreement=math.dot(normal,-approach);if(math.abs(agreement)<.85f)continue;if(agreement<0)normal=-normal;
   float distance=math.min(math.distancesq(display.m_Position.xz,curve.a.xz),math.distancesq(display.m_Position.xz,curve.d.xz));if(distance>=best)continue;best=distance;facing=normal;
  }
  display.m_Rotation=quaternion.RotateY(math.atan2(facing.x,facing.y));return display;
 }
}
}
