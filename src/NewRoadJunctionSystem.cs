using Game;
using Game.Common;
using Game.Net;
using Game.Tools;
using Game.Prefabs;
using Unity.Collections;
using Unity.Entities;

namespace TrafficLightIntoKorea {
 // Inspect only the tool transaction before native ApplyNet rewrites its originals.
 public partial class NewRoadJunctionSystem : GameSystemBase {
  EntityQuery _nodes;
  protected override void OnCreate(){
   base.OnCreate();
   _nodes=GetEntityQuery(ComponentType.ReadOnly<Temp>(),ComponentType.ReadOnly<Node>(),
    ComponentType.ReadOnly<ConnectedEdge>(),ComponentType.Exclude<Deleted>());
   RequireForUpdate(_nodes);
  }
  internal static bool HasRoadJunction(EntityManager em,Entity node){
   if(!em.Exists(node)||!em.HasBuffer<ConnectedEdge>(node))return false;
   var arms=em.GetBuffer<ConnectedEdge>(node,true);if(arms.Length<3)return false;
   int roads=0;
   foreach(var arm in arms){
    var edge=arm.m_Edge;
    if(!em.HasComponent<Road>(edge)||em.HasComponent<Deleted>(edge))continue;
    if(em.HasComponent<Temp>(edge)&&(em.GetComponentData<Temp>(edge).m_Flags&(TempFlags.Delete|TempFlags.Cancel))!=0)continue;
    if(++roads==3)return true;
   }
   return false;
  }
  // Check every connected road, including arms after the third one.
  // A ramp joining a city road must not acquire automatic signals either.
  internal static bool AllowsAutomaticSignals(EntityManager em, Entity node) {
   if (!HasRoadJunction(em, node)) return false;
   var arms = em.GetBuffer<ConnectedEdge>(node, true);
   foreach (var arm in arms) {
    var edge = arm.m_Edge;
    if (!em.HasComponent<Road>(edge) || em.HasComponent<Deleted>(edge)) continue;
    if (em.HasComponent<Temp>(edge) &&
        (em.GetComponentData<Temp>(edge).m_Flags & (TempFlags.Delete | TempFlags.Cancel)) != 0) continue;
    if (!em.HasComponent<PrefabRef>(edge)) return false;
    var prefab = em.GetComponentData<PrefabRef>(edge).m_Prefab;
    if (!em.HasComponent<RoadData>(prefab)) return false;
    if ((em.GetComponentData<RoadData>(prefab).m_Flags & Game.Prefabs.RoadFlags.UseHighwayRules) != 0) return false;

   }
   return true;
  }
  protected override void OnUpdate(){
   var junctions=KoreanJunctionSystem.Active;
   if(junctions==null||!junctions.AcceptNewRoadJunctions)return;
   using(var nodes=_nodes.ToEntityArray(Allocator.Temp))foreach(var node in nodes){
    var temp=EntityManager.GetComponentData<Temp>(node);
    if((temp.m_Flags&(TempFlags.Delete|TempFlags.Cancel))!=0||!AllowsAutomaticSignals(EntityManager,node))continue;
    if(temp.m_Original!=Entity.Null&&HasRoadJunction(EntityManager,temp.m_Original))continue;
    junctions.QueueNewRoadJunction(temp.m_Original==Entity.Null||(temp.m_Flags&TempFlags.Replace)!=0?node:temp.m_Original);
   }
  }
 }
 public partial class KoreanJunctionSystem {
  readonly SignQueue<Entity> _newRoadJunctions=new SignQueue<Entity>();
  internal bool AcceptNewRoadJunctions=>_ready&&Mod.Settings?.Enabled==true&&Mod.Settings.AutoNewRoadJunctions&&!BulkUnavailable;
  internal void QueueNewRoadJunction(Entity node){_newRoadJunctions.Enqueue(node);}
  void TickNewRoadJunction(){
   if(_newRoadJunctions.Count==0)return;
   if(!AcceptNewRoadJunctions){_newRoadJunctions.Clear();return;}
   var node=_newRoadJunctions.Dequeue();
   if(!EntityManager.Exists(node)||EntityManager.HasComponent<Deleted>(node)||EntityManager.HasComponent<Temp>(node)
    ||Selected(EntityManager,node)||!NewRoadJunctionSystem.AllowsAutomaticSignals(EntityManager,node))return;
   if(EntityManager.HasComponent<TrafficLights>(node))Select(node,true);
   else BeginSignalCreation(node);
  }
 }
}

