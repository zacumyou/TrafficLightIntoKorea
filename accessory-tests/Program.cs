using System;using System.Collections.Generic;using System.Linq;
using Unity.Entities;using Game.Prefabs;using TrafficLightIntoKorea;
namespace Unity.Entities {
 public struct Entity {public int Id;public static implicit operator Entity(int id)=>new Entity{Id=id};}
 public class EntityManager {
  private readonly Dictionary<(int,Type),object> buffers=new();public readonly HashSet<int> Live=new();
  public bool Exists(Entity e)=>Live.Contains(e.Id);
  public bool HasBuffer<T>(Entity e)=>buffers.ContainsKey((e.Id,typeof(T)));
  public T[] GetBuffer<T>(Entity e,bool readOnly)=>(T[])buffers[(e.Id,typeof(T))];
  public void Buffer<T>(Entity e,params T[] values){Live.Add(e.Id);buffers[(e.Id,typeof(T))]=values;}
 }
}
namespace Game.Prefabs {
 public class StaticObjectPrefab {public string name;}
 public struct SubObject {public Entity m_Prefab;}
 public struct SubMesh {}
 public class PrefabSystem {
  public readonly Dictionary<int,StaticObjectPrefab> Items=new();
  public bool TryGetPrefab<T>(Entity e,out T p) where T:class {p=Items.TryGetValue(e.Id,out var found)?found as T:null;return p!=null;}
 }
}
namespace TrafficLightIntoKorea {internal static class SignMounts {internal static int Kind(PrefabSystem ps,Entity e)=>ps.TryGetPrefab<StaticObjectPrefab>(e,out var p)&&p.name=="CSKRTrafficLightSignSpeed50"?1:0;}}
static class Program {
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Main(){
  var em=new EntityManager();var ps=new PrefabSystem();
  void Add(int id,string name){ps.Items[id]=new StaticObjectPrefab{name=name};em.Buffer<SubMesh>(id,new SubMesh());}
  Add(1,"SignalA");Add(2,"Camera");Add(3,"Box");Add(4,"CSKRTrafficLightSignSpeed50");Add(5,"SignBracket");Add(6,"SignalB");
  em.Buffer<SubObject>(1,new(){m_Prefab=2},new(){m_Prefab=2},new(){m_Prefab=4});
  em.Buffer<SubObject>(2,new SubObject{m_Prefab=3});em.Buffer<SubObject>(4,new SubObject{m_Prefab=5});
  var layout=new AccessoryLayout();var a=layout.Get(1,ps,em);
  Check(a.Length==4,"Both camera instances and nested boxes are declared");
  Check(a.Select(x=>x.Key).Distinct().Count()==4,"Repeated prefab instances need independent switches");
  Check(!a.Any(x=>x.Name.Contains("Sign")),"Signs and their children must stay in the sign tab");
  Check(ReferenceEquals(a,layout.Get(1,ps,em)),"Cached layout must avoid repeated traversal");
  em.Buffer<SubObject>(6,new SubObject{m_Prefab=2});var b=layout.Get(6,ps,em);
  Check(!a.Select(x=>x.Key).Intersect(b.Select(x=>x.Key)).Any(),"An asset switch cannot reuse unrelated prop settings");
  Check(AccessoryKeys.Signature(a.Select(x=>x.Key))==AccessoryKeys.Signature(a.Reverse().Select(x=>x.Key)),"Toggle order must reuse the same variant");
  em.Buffer<SubObject>(3,new SubObject{m_Prefab=1});layout.Clear();Check(layout.Get(1,ps,em).Length==0,"Cyclic prefab hierarchy must fail closed");
  em.Buffer<SubObject>(3);layout.Clear();Check(layout.Get(1,ps,em).Length==4,"Layout reload must recover after a prefab edit");
  Console.WriteLine("PASS: 8 accessory hierarchy, repeated instance, sign exclusion, cache, asset isolation and cycle checks.");
 }
}
