using System;using System.Collections.Generic;using Game.Common;using Game.Net;using Unity.Entities;
namespace TrafficLightIntoKorea {
// One known junction per update; catches lane rebuilds without owner Updated.
internal sealed class JunctionWatch {
 private readonly List<Entity> _owners=new List<Entity>();private readonly Dictionary<Entity,int> _hashes=new Dictionary<Entity,int>();private int _cursor;
 internal void Track(Entity owner){if(owner!=Entity.Null&&!_hashes.ContainsKey(owner)){_owners.Add(owner);_hashes.Add(owner,int.MinValue);}}
 internal void Untrack(Entity owner){if(!_hashes.Remove(owner))return;int i=_owners.IndexOf(owner);if(i>=0){_owners[i]=_owners[_owners.Count-1];_owners.RemoveAt(_owners.Count-1);}if(_cursor>=_owners.Count)_cursor=0;}
 internal void Clear(){_owners.Clear();_hashes.Clear();_cursor=0;}
 internal void Tick(EntityManager em,Action<Entity> enqueue){if(_owners.Count==0||!SignalWorkBudget.Available)return;if(_cursor>=_owners.Count)_cursor=0;var owner=_owners[_cursor];
  if(!em.Exists(owner)||em.HasComponent<Deleted>(owner)){enqueue(owner);_hashes.Remove(owner);_owners[_cursor]=_owners[_owners.Count-1];_owners.RemoveAt(_owners.Count-1);return;}_cursor++;
  int hash=17;unchecked {
   if(em.HasBuffer<ConnectedEdge>(owner))foreach(var item in em.GetBuffer<ConnectedEdge>(owner,true)){var edge=item.m_Edge;hash=hash*31+edge.GetHashCode();if(!em.Exists(edge)||em.HasComponent<Deleted>(edge)||em.HasComponent<Game.Tools.Temp>(edge)){hash=hash*31-1;continue;}if(em.HasComponent<Edge>(edge))hash=hash*31+em.GetComponentData<Edge>(edge).GetHashCode();if(em.HasComponent<Curve>(edge))hash=hash*31+em.GetComponentData<Curve>(edge).m_Bezier.GetHashCode();if(em.HasComponent<Composition>(edge)){var comp=em.GetComponentData<Composition>(edge).m_Edge;hash=hash*31+comp.GetHashCode();if(em.HasComponent<Game.Prefabs.NetCompositionData>(comp))hash=hash*31+em.GetComponentData<Game.Prefabs.NetCompositionData>(comp).m_Width.GetHashCode();}}

   if(em.HasBuffer<SubLane>(owner))foreach(var s in em.GetBuffer<SubLane>(owner,true)){var e=s.m_SubLane;hash=hash*31+e.GetHashCode();if(em.HasComponent<Deleted>(e)||em.HasComponent<Game.Tools.Temp>(e)){hash=hash*31-1;continue;}if(em.HasComponent<CarLane>(e))hash=hash*31+(int)em.GetComponentData<CarLane>(e).m_Flags;if(em.HasComponent<Lane>(e))hash=hash*31+em.GetComponentData<Lane>(e).GetHashCode();if(em.HasComponent<Curve>(e))hash=hash*31+em.GetComponentData<Curve>(e).m_Bezier.GetHashCode();if(em.HasComponent<LaneSignal>(e))hash=hash*31+em.GetComponentData<LaneSignal>(e).m_GroupMask;}
   if(em.HasBuffer<Game.Objects.SubObject>(owner))foreach(var s in em.GetBuffer<Game.Objects.SubObject>(owner,true)){var e=s.m_SubObject;hash=hash*31+e.GetHashCode();if(!em.Exists(e)||em.HasComponent<Deleted>(e)||em.HasComponent<Game.Tools.Temp>(e)){hash=hash*31-1;continue;}if(em.HasComponent<Game.Objects.Transform>(e))hash=hash*31+em.GetComponentData<Game.Objects.Transform>(e).GetHashCode();if(em.HasComponent<Game.Prefabs.PrefabRef>(e))hash=hash*31+em.GetComponentData<Game.Prefabs.PrefabRef>(e).m_Prefab.GetHashCode();if(em.HasComponent<Game.Objects.TrafficLight>(e)){var state=em.GetComponentData<Game.Objects.TrafficLight>(e);hash=hash*31+state.m_GroupMask0;hash=hash*31+state.m_GroupMask1;}}
  }
  if(_hashes[owner]!=hash){RuntimeDiagnostics.Count("junction.fingerprintChanged");_hashes[owner]=hash;enqueue(owner);}
 }
}
}
