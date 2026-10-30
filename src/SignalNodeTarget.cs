using Game.Common;using Game.Tools;using Unity.Entities;
namespace TrafficLightIntoKorea {
internal static class SignalNodeTarget {
 internal static bool Selectable(EntityManager em,Entity node)=>em.Exists(node)&&!em.HasComponent<Deleted>(node)&&!em.HasComponent<Temp>(node)&&em.HasComponent<Game.Net.Node>(node)&&em.HasBuffer<Game.Net.ConnectedEdge>(node)&&SignalNodeRules.Selectable(em.GetBuffer<Game.Net.ConnectedEdge>(node,true).Length,em.HasComponent<Game.Net.TrafficLights>(node),em.HasComponent<KoreanJunction>(node));
}
}
