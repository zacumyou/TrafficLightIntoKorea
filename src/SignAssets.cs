using System;
using System.Collections.Generic;
using Game.Prefabs;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace TrafficLightIntoKorea {
// At most ten render-only prefabs. No TrafficSignObject: sign metadata cannot
// alter simulation speed limits. Native render prefabs retain their shared LODs/materials.
internal sealed class SignAssets {
 internal const string Prefix="TLK_RuntimeSign_";
 private readonly Dictionary<string,StaticObjectPrefab> _proxies=new Dictionary<string,StaticObjectPrefab>(StringComparer.Ordinal);
 private readonly SignQueue<StaticObjectPrefab> _pending=new SignQueue<StaticObjectPrefab>();
 private int _attempt;private float _next;
 internal int Count=>_proxies.Count;
 internal void Retry(){_attempt=0;_next=0;}
 internal void Prepare(PrefabSystem ps,EntityManager em){
  if(ps==null)return;
  if(_proxies.Count<SignRules.Names.Length&&_pending.Count==0&&_attempt<6&&Time.realtimeSinceStartup>=_next){
   _attempt++;_next=Time.realtimeSinceStartup+10;
   var found=new Dictionary<string,StaticObjectPrefab>(StringComparer.Ordinal);var duplicate=new HashSet<string>();
   using(var query=em.CreateEntityQuery(ComponentType.ReadOnly<PrefabData>(),ComponentType.ReadOnly<ObjectData>()))
   using(var entities=query.ToEntityArray(Allocator.Temp))foreach(var e in entities)if(ps.TryGetPrefab<StaticObjectPrefab>(e,out var p)&&p!=null&&SignRules.Kind(p.name)!=0&&!_proxies.ContainsKey(p.name)){if(found.ContainsKey(p.name))duplicate.Add(p.name);else found[p.name]=p;}
   foreach(var pair in found)if(!duplicate.Contains(pair.Key))_pending.Enqueue(pair.Value);
  }
  for(int i=0;i<2&&_pending.Count>0;i++){
   var source=_pending.Dequeue();if(source==null||string.IsNullOrEmpty(source.name)||_proxies.ContainsKey(source.name)||source.m_Meshes==null||source.m_Meshes.Length==0)continue;
   var proxy=ScriptableObject.CreateInstance<StaticObjectPrefab>();proxy.name=Prefix+source.name;proxy.m_Meshes=source.m_Meshes;proxy.m_Circular=source.m_Circular;
   if(ps.AddPrefab(proxy))_proxies[source.name]=proxy;else UnityEngine.Object.Destroy(proxy);
  }
 }
 internal Entity Resolve(string name,PrefabSystem ps,EntityManager em){if(ps!=null&&name!=null&&_proxies.TryGetValue(name,out var p)&&p!=null&&ps.TryGetEntity(p,out var e)&&em.Exists(e)&&em.HasComponent<ObjectData>(e)&&em.GetComponentData<ObjectData>(e).m_Archetype.Valid)return e;return Entity.Null;}
}
}


