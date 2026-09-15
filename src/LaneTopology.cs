using System.Collections.Generic;
using Game.Common;using Game.Net;using Game.Tools;using Unity.Entities;
namespace TrafficLightIntoKorea {
internal static class LaneTopology {
 internal static HashSet<int> LiveRoads(EntityManager em,Entity owner){
  var roads=new HashSet<int>();if(!em.HasBuffer<ConnectedEdge>(owner))return roads;
  foreach(var item in em.GetBuffer<ConnectedEdge>(owner,true)){var e=item.m_Edge;if(!em.Exists(e)||em.HasComponent<Deleted>(e)||em.HasComponent<Temp>(e)||!em.HasComponent<Edge>(e))continue;var edge=em.GetComponentData<Edge>(e);if(edge.m_Start==owner||edge.m_End==owner)roads.Add(e.Index);}
  return roads;
 }
 internal static bool ConnectsLiveRoads(HashSet<int> roads,Lane lane)=>roads.Contains(lane.m_StartNode.GetOwnerIndex())&&roads.Contains(lane.m_EndNode.GetOwnerIndex());
}
}
