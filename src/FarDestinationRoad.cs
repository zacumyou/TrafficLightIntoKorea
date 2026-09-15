using Game.Prefabs;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
public partial class FarSignalSystem {
 private Entity DestinationRoad(Entity owner,int index){if(EntityManager.HasBuffer<Game.Net.ConnectedEdge>(owner))foreach(var edge in EntityManager.GetBuffer<Game.Net.ConnectedEdge>(owner,true))if(edge.m_Edge.Index==index&&Live(edge.m_Edge))return edge.m_Edge;return Entity.Null;}
 private float RoadWidth(Entity edge){if(EntityManager.HasComponent<Game.Net.Composition>(edge)){var c=EntityManager.GetComponentData<Game.Net.Composition>(edge).m_Edge;if(EntityManager.HasComponent<NetCompositionData>(c))return EntityManager.GetComponentData<NetCompositionData>(c).m_Width;}return 0;}
 // A wider search is allowed only on the outgoing road reached by a real straight lane.
 private bool OnDestinationSide(Entity owner,Entity edge,float2 incoming,float3 point){
  if(edge==Entity.Null)return false;if(!EntityManager.HasComponent<Game.Net.Edge>(edge)||!EntityManager.HasComponent<Game.Net.Curve>(edge))return false;
  var connection=EntityManager.GetComponentData<Game.Net.Edge>(edge);var b=EntityManager.GetComponentData<Game.Net.Curve>(edge).m_Bezier;bool start=connection.m_Start==owner;if(!start&&connection.m_End!=owner)return false;
  var center=start?b.a:b.d;var direction=math.normalizesafe((start?b.b-b.a:b.c-b.d).xz);if(math.dot(direction,incoming)<.7f)return false;
  var delta=point.xz-center.xz;float along=math.dot(delta,direction);float side=delta.x*direction.y-delta.y*direction.x;float width=RoadWidth(edge);bool lht=World.GetOrCreateSystemManaged<Game.City.CityConfigurationSystem>().leftHandTraffic;
  return along>=-8f&&along<=20f&&width>0&&math.abs(side)<=width*.5f+3f&&DrivingSideRules.Accept(delta.x,delta.y,direction.x,direction.y,lht);
 }
}
}
