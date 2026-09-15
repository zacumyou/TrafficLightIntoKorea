using System.Collections.Generic;
using Game.Prefabs;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace TrafficLightIntoKorea {
// Registered native prefabs, without spawning/placement metadata or child entities.
internal sealed class ApproachProxies {
 internal const string Prefix="TLK_RuntimeApproach_";
 private int _generation;private readonly string _suffix;private readonly HashSet<string> _disabled;private readonly Entity _roadName;internal ApproachProxies(string suffix="",HashSet<string> disabled=null,Entity roadName=default){_roadName=roadName;_suffix=suffix;_disabled=disabled;}
 internal void Invalidate(){_prefabs.Clear();_pending.Clear();_queued.Clear();_status.Clear();_generation++;}
 private readonly Dictionary<Entity,StaticObjectPrefab> _prefabs=new Dictionary<Entity,StaticObjectPrefab>();
 private readonly List<Entity> _pending=new List<Entity>();
 private readonly HashSet<Entity> _queued=new HashSet<Entity>();
 private readonly Dictionary<Entity,string> _status=new Dictionary<Entity,string>();
 internal string Status(Entity source,PrefabSystem system,EntityManager em){if(Resolve(source,system,em)!=Entity.Null)return "ready";return _status.TryGetValue(source,out var reason)?reason:"not requested";}
 internal void Prepare(Entity source,OverrideSystem owner,PrefabSystem system,EntityManager em){if(_prefabs.ContainsKey(source))return;if(_queued.Add(source)){_pending.Add(source);_status[source]="queued before PrefabInitializeSystem";}}
 internal void Process(OverrideSystem owner,PrefabSystem system,EntityManager em){for(int i=0;i<2&&_pending.Count>0;i++){var source=_pending[0];_pending.RemoveAt(0);_queued.Remove(source);if(em.Exists(source))Create(source,owner,system,em);}}
 private void Create(Entity source,OverrideSystem owner,PrefabSystem system,EntityManager em){
  if(_prefabs.ContainsKey(source))return;
  _status[source]="source prefab or TrafficLightObject unavailable";
  if(!system.TryGetPrefab<StaticObjectPrefab>(source,out var original)||!original.TryGet<TrafficLightObject>(out var traffic))return;
  _status[source]="fixed mesh hierarchy unavailable";
  var parts=new List<SubMesh>();if(!owner.Flatten(source,float3.zero,quaternion.identity,parts,new HashSet<Entity>(),0,_disabled,null,_roadName))return;
  _status[source]="render prefab unavailable";
  var meshes=new List<ObjectMeshInfo>();foreach(var part in parts){
   if((part.m_Flags&~SubMeshFlags.HasTransform)!=0||!system.TryGetPrefab<RenderPrefabBase>(part.m_SubMesh,out var mesh))return;
   meshes.Add(new ObjectMeshInfo{m_Mesh=mesh,m_Position=part.m_Position,m_Rotation=part.m_Rotation});
  }
  var proxy=ScriptableObject.CreateInstance<StaticObjectPrefab>();proxy.name=Prefix+source.Index+"_"+source.Version+"_"+_generation+"_"+_suffix;proxy.m_Meshes=meshes.ToArray();proxy.m_Circular=original.m_Circular;
  proxy.AddComponentFrom(traffic);
  if(system.AddPrefab(proxy)){_prefabs.Add(source,proxy);_status[source]="registered; awaiting native initialization";}else {_status[source]="native registration rejected";Object.Destroy(proxy);}
 }
 internal Entity SourceOf(Entity proxy,PrefabSystem system){foreach(var pair in _prefabs)if(system.TryGetEntity(pair.Value,out var e)&&e==proxy)return pair.Key;return Entity.Null;}
 internal Entity Resolve(Entity source,PrefabSystem system,EntityManager em){
  if(!_prefabs.TryGetValue(source,out var prefab)||!system.TryGetEntity(prefab,out var e)||!em.Exists(e)||em.HasComponent<Game.Common.Deleted>(e)||!em.HasComponent<ObjectData>(e)||!em.GetComponentData<ObjectData>(e).m_Archetype.Valid)return Entity.Null;
  return e;
 }
}
}

