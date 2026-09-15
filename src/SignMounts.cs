using System.Collections.Generic;
using Game.Prefabs;
using Unity.Entities;
using Unity.Mathematics;
namespace TrafficLightIntoKorea {
internal struct SignMount {internal int Kind;internal float3 Position;internal quaternion Rotation;}
// Layouts belong to signal PREFABS, never to individual junctions.
internal sealed class SignMounts {
 private readonly Dictionary<Entity,SignMount[]> _layouts=new Dictionary<Entity,SignMount[]>();
 private readonly Dictionary<Entity,Entity> _aliases=new Dictionary<Entity,Entity>();
 internal void Alias(Entity visual,Entity source){if(visual!=source&&(!_aliases.TryGetValue(visual,out var old)||old!=source)){_aliases[visual]=source;_layouts.Remove(visual);}}
 internal void Unalias(Entity visual){_aliases.Remove(visual);_layouts.Remove(visual);}
 internal void Invalidate(){_layouts.Clear();}
 internal static int Kind(PrefabSystem ps,Entity prefab)=>ps!=null&&ps.TryGetPrefab<StaticObjectPrefab>(prefab,out var p)&&p!=null?SignRules.Kind(p.name):0;
 internal SignMount[] Get(Entity prefab,PrefabSystem ps,EntityManager em){for(int i=0;i<8&&_aliases.TryGetValue(prefab,out var source);i++)prefab=source;if(_layouts.TryGetValue(prefab,out var mounts))return mounts;
  var list=new List<SignMount>();if(!Collect(prefab,float3.zero,quaternion.identity,ps,em,list,new HashSet<Entity>(),0)||list.Count>16)list.Clear();mounts=list.ToArray();_layouts[prefab]=mounts;return mounts;
 }
 private bool Collect(Entity e,float3 position,quaternion rotation,PrefabSystem ps,EntityManager em,List<SignMount> result,HashSet<Entity> path,int depth){
  if(depth>8||result.Count>16||!path.Add(e)||!em.Exists(e))return false;
  int kind=Kind(ps,e);if(depth>0&&kind!=0){result.Add(new SignMount{Kind=kind,Position=position,Rotation=rotation});path.Remove(e);return true;}
  if(em.HasBuffer<Game.Prefabs.SubObject>(e))foreach(var child in em.GetBuffer<Game.Prefabs.SubObject>(e,true)){
   var p=position;var r=rotation;if(child.m_ParentIndex>=0){if(!em.HasBuffer<SubMesh>(e))return false;var parts=em.GetBuffer<SubMesh>(e,true);if(child.m_ParentIndex>=parts.Length)return false;var part=parts[child.m_ParentIndex];p+=math.rotate(r,part.m_Position);r=math.mul(r,part.m_Rotation);}
   if(!Collect(child.m_Prefab,p+math.rotate(r,child.m_Position),math.mul(r,child.m_Rotation),ps,em,result,path,depth+1))return false;
  }
  path.Remove(e);return true;
 }
}
}


