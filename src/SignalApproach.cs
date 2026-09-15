using System.Collections.Generic;
using Game.Common;using Game.Net;using Game.Objects;using Game.Tools;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
// Geometry identifies an approach; phase bit masks are mutable controller state.
internal static class SignalApproach {
 internal static bool Try(EntityManager em,Entity owner,Transform signal,out int road,out float2 direction){
  road=-1;direction=0;if(!em.HasBuffer<Game.Net.SubLane>(owner))return false;
  var live=LaneTopology.LiveRoads(em,owner);var toward=-math.normalizesafe(math.rotate(signal.m_Rotation,new float3(0,0,1)).xz);
  float best=float.MaxValue;bool ambiguous=false;
  foreach(var item in em.GetBuffer<Game.Net.SubLane>(owner,true)){
   var e=item.m_SubLane;if(!em.Exists(e)||em.HasComponent<Deleted>(e)||em.HasComponent<Temp>(e)||em.HasComponent<MasterLane>(e)||!em.HasComponent<CarLane>(e)||!em.HasComponent<Lane>(e)||!em.HasComponent<Curve>(e))continue;
   var lane=em.GetComponentData<Lane>(e);if(!LaneTopology.ConnectsLiveRoads(live,lane)||(em.GetComponentData<CarLane>(e).m_Flags&CarLaneFlags.Forbidden)!=0)continue;
   var curve=em.GetComponentData<Curve>(e).m_Bezier;var start=math.normalizesafe((curve.b-curve.a).xz);
   float distance=math.distancesq(curve.a.xz,signal.m_Position.xz);float agreement=math.dot(start,toward);
   if(!SignalApproachRules.Candidate(distance,agreement))continue;
   float score=distance;int candidate=lane.m_StartNode.GetOwnerIndex();
   if(score<best-.01f){road=candidate;direction=start;best=score;ambiguous=false;}
   else if(math.abs(score-best)<.01f&&candidate!=road)ambiguous=true;
  }
  return road>=0&&!ambiguous;
 }
}
internal static class SignalApproachRules {
 internal static bool Candidate(float distanceSquared,float facingAgreement)=>math.isfinite(distanceSquared)&&distanceSquared<=6400f&&facingAgreement>=.70710677f;
 internal static bool SameRoad(int a,int b)=>a>=0&&a==b;
}
}
