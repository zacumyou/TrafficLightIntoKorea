using System;using Game.Common;using Game.Prefabs;using Game.Tools;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
internal static class FarRoadRules {
 internal const float NarrowWidth=12f;
 internal static bool Narrow(float width)=>!float.IsNaN(width)&&!float.IsInfinity(width)&&width>0&&width<=NarrowWidth;
 internal static bool Keep(float elapsed,bool sourceLive,bool excluded)=>sourceLive&&!excluded&&elapsed<3f;
}
public partial class FarSignalSystem {
 // Resolve the road the signal's vehicles arrive FROM, not the perpendicular road.
 private bool TryApproachWidth(Entity owner,float3 origin,float2 direction,int mask,out float width){
  width=0;if(!EntityManager.HasBuffer<Game.Net.ConnectedEdge>(owner))return false;
  float best=float.MaxValue;int roadIndex=-1;
  foreach(var item in EntityManager.GetBuffer<Game.Net.SubLane>(owner,true)){
   var lane=item.m_SubLane;if(!Live(lane)||EntityManager.HasComponent<Temp>(lane)||!EntityManager.HasComponent<Game.Net.CarLane>(lane)||!EntityManager.HasComponent<Game.Net.Lane>(lane)||!EntityManager.HasComponent<Game.Net.Curve>(lane)||!EntityManager.HasComponent<Game.Net.LaneSignal>(lane))continue;
   if((EntityManager.GetComponentData<Game.Net.CarLane>(lane).m_Flags&Game.Net.CarLaneFlags.Forbidden)!=0)continue;
   var curve=EntityManager.GetComponentData<Game.Net.Curve>(lane).m_Bezier;
   if(math.dot(math.normalizesafe((curve.b-curve.a).xz),direction)<.9f)continue;
   int index=EntityManager.GetComponentData<Game.Net.Lane>(lane).m_StartNode.GetOwnerIndex();
   bool connected=false;foreach(var edge in EntityManager.GetBuffer<Game.Net.ConnectedEdge>(owner,true))if(edge.m_Edge.Index==index&&Live(edge.m_Edge)&&!EntityManager.HasComponent<Temp>(edge.m_Edge)){connected=true;break;}
   float score=math.distancesq(curve.a.xz,origin.xz);if(connected&&score<best){best=score;roadIndex=index;}
  }
  if(roadIndex<0)return false;
  foreach(var item in EntityManager.GetBuffer<Game.Net.ConnectedEdge>(owner,true)){
   var edge=item.m_Edge;if(edge.Index!=roadIndex)continue;
   if(EntityManager.HasComponent<Game.Net.Composition>(edge)){var comp=EntityManager.GetComponentData<Game.Net.Composition>(edge).m_Edge;if(EntityManager.HasComponent<NetCompositionData>(comp))width=EntityManager.GetComponentData<NetCompositionData>(comp).m_Width;}
   if(!(width>0)&&EntityManager.HasComponent<PrefabRef>(edge)){var prefab=EntityManager.GetComponentData<PrefabRef>(edge).m_Prefab;if(EntityManager.HasComponent<NetGeometryData>(prefab))width=EntityManager.GetComponentData<NetGeometryData>(prefab).m_DefaultWidth;}
   return width>0&&!float.IsNaN(width)&&!float.IsInfinity(width);
  }return false;
 }
}
}

