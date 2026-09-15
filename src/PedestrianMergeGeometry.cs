using Game.Common;using Game.Tools;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
internal static class PedestrianMergeGeometry {
 internal static bool SameCrosswalkEnd(EntityManager em,Entity owner,float3 vehicle,float3 pedestrian,ushort mask,float vehicleRadiusSquared=16f){
  if(!em.HasBuffer<Game.Net.SubLane>(owner))return false;
  foreach(var sub in em.GetBuffer<Game.Net.SubLane>(owner,true)){
   var lane=sub.m_SubLane;if(!em.Exists(lane)||em.HasComponent<Deleted>(lane)||em.HasComponent<Temp>(lane)||!em.HasComponent<Game.Net.PedestrianLane>(lane)||!em.HasComponent<Game.Net.LaneSignal>(lane)||!em.HasComponent<Game.Net.Curve>(lane))continue;
   if((em.GetComponentData<Game.Net.PedestrianLane>(lane).m_Flags&Game.Net.PedestrianLaneFlags.Crosswalk)==0||em.GetComponentData<Game.Net.LaneSignal>(lane).m_GroupMask!=mask)continue;
   var b=em.GetComponentData<Game.Net.Curve>(lane).m_Bezier;if(NearPedestrianMergeRules.SameEnd(math.distancesq(vehicle,b.a),math.distancesq(vehicle,b.d),math.distancesq(pedestrian,b.a),math.distancesq(pedestrian,b.d),vehicleRadiusSquared))return true;
  }return false;
 }
}
}
