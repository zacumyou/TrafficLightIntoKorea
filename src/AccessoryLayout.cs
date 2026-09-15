using System;using System.Collections.Generic;using System.Security.Cryptography;using System.Text;
using Game.Prefabs;using Unity.Entities;
namespace TrafficLightIntoKorea {
internal sealed class AccessorySlot {internal string Key,Name;internal Entity Prefab;}
internal static class AccessoryKeys {
 internal static string Child(string route,int index,string name)=>route+"/"+index+":"+name;
 internal static string Key(string route){using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(route))).Replace("-","");}
 internal static string Signature(IEnumerable<string> disabled){var keys=new List<string>(disabled);keys.Sort(StringComparer.Ordinal);return Key(string.Join("|",keys));}
}
internal sealed class AccessoryLayout {
 private readonly Dictionary<Entity,AccessorySlot[]> _cache=new Dictionary<Entity,AccessorySlot[]>();
 internal void Clear()=>_cache.Clear();
 internal AccessorySlot[] Get(Entity source,PrefabSystem ps,EntityManager em){if(_cache.TryGetValue(source,out var result))return result;var list=new List<AccessorySlot>();if(!Walk(source,Root(source,ps),ps,em,list,new HashSet<Entity>(),0))list.Clear();return _cache[source]=list.ToArray();}
 internal static string Root(Entity source,PrefabSystem ps)=>ps.TryGetPrefab<StaticObjectPrefab>(source,out var p)?p.name:"";
 private bool Walk(Entity e,string route,PrefabSystem ps,EntityManager em,List<AccessorySlot> slots,HashSet<Entity> path,int depth){
  if(depth>8||slots.Count>64||!em.Exists(e)||!path.Add(e))return false;
  if(em.HasBuffer<Game.Prefabs.SubObject>(e)){var children=em.GetBuffer<Game.Prefabs.SubObject>(e,true);for(int i=0;i<children.Length;i++){
   var child=children[i].m_Prefab;if(SignMounts.Kind(ps,child)!=0)continue;
   if(!ps.TryGetPrefab<StaticObjectPrefab>(child,out var prefab))return false;
   var next=AccessoryKeys.Child(route,i,prefab.name);
   if(em.HasBuffer<SubMesh>(child)&&em.GetBuffer<SubMesh>(child,true).Length>0)slots.Add(new AccessorySlot{Key=AccessoryKeys.Key(next),Name=prefab.name,Prefab=child});
   if(!Walk(child,next,ps,em,slots,path,depth+1))return false;
  }}path.Remove(e);return slots.Count<=64;
 }
}
}
