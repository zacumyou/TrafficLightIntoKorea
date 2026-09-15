using System;using Game.Common;using Game.Prefabs;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
public partial class FarSignalSystem {
 // A terminating T approach has no Forward lane. Intersect its forward ray with
 // an actual pedestrian lane band on the perpendicular connected road instead.
 private bool TryTeeSidewalk(Entity owner,float3 origin,float2 direction,float inset,out float3 result){
 result=origin;if(!EntityManager.HasBuffer<Game.Net.ConnectedEdge>(owner))return false;
 var edges=EntityManager.GetBuffer<Game.Net.ConnectedEdge>(owner,true);if(edges.Length<3)return false;
 float best=0;float3 chosen=origin;
 foreach(var item in edges){var edge=item.m_Edge;if(!Live(edge)||!EntityManager.HasComponent<Game.Net.Edge>(edge)||!EntityManager.HasComponent<Game.Net.Curve>(edge)||!EntityManager.HasComponent<Game.Net.Composition>(edge))continue;
 var connection=EntityManager.GetComponentData<Game.Net.Edge>(edge);bool start=connection.m_Start==owner;if(!start&&connection.m_End!=owner)continue;
 var curve=EntityManager.GetComponentData<Game.Net.Curve>(edge);var b=curve.m_Bezier;float3 anchor=start?b.a:b.d;var tangent=math.normalizesafe((start?b.b-b.a:b.d-b.c).xz);if(math.lengthsq(tangent)<.9f||math.abs(math.dot(tangent,direction))>.5f)continue;
 var comp=EntityManager.GetComponentData<Game.Net.Composition>(edge).m_Edge;if(!EntityManager.HasBuffer<NetCompositionLane>(comp))continue;var normal=new float2(tangent.y,-tangent.x);
 foreach(var lane in EntityManager.GetBuffer<NetCompositionLane>(comp,true)){
 if((lane.m_Flags&LaneFlags.Pedestrian)==0||(lane.m_Flags&(LaneFlags.Master|LaneFlags.Underground|LaneFlags.Virtual|LaneFlags.CrossRoad))!=0)continue;
 float offset=lane.m_Position.x;if(math.abs(offset)<.5f||math.dot(normal*offset,direction)<=0)continue;
 float width=EntityManager.HasComponent<NetLaneData>(lane.m_Lane)?EntityManager.GetComponentData<NetLaneData>(lane.m_Lane).m_Width:0;
 if(width>.5f)offset=math.sign(offset)*(math.abs(offset)-width*.5f+math.clamp(inset,.25f,math.max(.25f,width-.25f)));
 var point=anchor.xz+normal*offset;
 if(!SidewalkPlacementRules.Intersect(origin.x,origin.z,direction.x,direction.y,point.x,point.y,tangent.x,tangent.y,out float distance,out float along))continue;
 float outward=start?along:-along;
 if(distance<5||distance>80||outward<-.5f||outward>math.min(30f,curve.m_Length)||math.abs(anchor.y+lane.m_Position.y-origin.y)>3f)continue;
 if(distance>best){best=distance;chosen=new float3(origin.x+direction.x*distance,anchor.y+lane.m_Position.y,origin.z+direction.y*distance);}
 }
 }
 if(best==0)return false;result=chosen;return true;
 }
}
internal static class SidewalkPlacementRules {
 internal static bool Intersect(float ox,float oz,float dx,float dz,float px,float pz,float tx,float tz,out float distance,out float along){
 distance=along=0;float denominator=dx*tz-dz*tx;if(Math.Abs(denominator)<.01f)return false;
 float qx=px-ox,qz=pz-oz;distance=(qx*tz-qz*tx)/denominator;along=(qx*dz-qz*dx)/denominator;return !float.IsNaN(distance)&&!float.IsInfinity(distance)&&!float.IsNaN(along)&&!float.IsInfinity(along);
 }
}
}
