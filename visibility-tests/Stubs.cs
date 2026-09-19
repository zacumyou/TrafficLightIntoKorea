using System;using System.Collections;using System.Collections.Generic;using System.Linq;
namespace Unity.Collections {public enum Allocator{Temp} public class Snapshot<T>:IEnumerable<T>,IDisposable {T[] a;public Snapshot(T[] a){this.a=a;} public IEnumerator<T> GetEnumerator()=>((IEnumerable<T>)a).GetEnumerator();IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();public void Dispose(){}}}
namespace Unity.Entities {
 public interface IComponentData{} public record struct Entity(int Id);
 public record struct ComponentType(Type Type){public static ComponentType ReadOnly<T>()=>new(typeof(T));}
 public class EntityQueryDesc {public ComponentType[] All=Array.Empty<ComponentType>(),Any=Array.Empty<ComponentType>(),None=Array.Empty<ComponentType>();}
 public class QueryData {public EntityManager Manager;public EntityQueryDesc[] Descs;}
 public struct EntityQuery {
  public QueryData Data;
  public EntityQueryDesc[] GetEntityQueryDescs()=>Data.Descs.Select(d=>new EntityQueryDesc{All=d.All.ToArray(),Any=d.Any.ToArray(),None=d.None.ToArray()}).ToArray();
  public Unity.Collections.Snapshot<Entity> ToEntityArray(Unity.Collections.Allocator allocator){var data=Data;return new(data.Manager.Entities.Where(e=>data.Descs.Any(d=>d.All.All(t=>data.Manager.Has(e,t.Type))&&(d.Any.Length==0||d.Any.Any(t=>data.Manager.Has(e,t.Type)))&&d.None.All(t=>!data.Manager.Has(e,t.Type)))).ToArray());}
 }
 public class EntityManager {
  Dictionary<Entity,Dictionary<Type,object>> data=new();int next;public int Writes;
  public IEnumerable<Entity> Entities=>data.Keys;
  public Entity Create(){var e=new Entity(++next);data[e]=new();return e;}
  public bool Exists(Entity e)=>data.ContainsKey(e);public bool Has(Entity e,Type t)=>Exists(e)&&data[e].ContainsKey(t);public bool HasComponent<T>(Entity e)=>Has(e,typeof(T));
  public void AddComponent<T>(Entity e){if(!HasComponent<T>(e)){Writes++;data[e][typeof(T)]=default(T);}}
  public void AddComponentData<T>(Entity e,T value){AddComponent<T>(e);data[e][typeof(T)]=value;}
  public T GetComponentData<T>(Entity e)=>(T)data[e][typeof(T)];
  public void RemoveComponent<T>(Entity e){if(data[e].Remove(typeof(T)))Writes++;}
  public EntityQuery Query(params EntityQueryDesc[] descriptions)=>new(){Data=new(){Manager=this,Descs=descriptions}};
 }
 public class World {
  public EntityManager Manager=new();Dictionary<Type,Game.GameSystemBase> systems=new();
  public T GetOrCreateSystemManaged<T>() where T:Game.GameSystemBase,new(){if(!systems.TryGetValue(typeof(T),out var s)){s=new T();systems[typeof(T)]=s;s.Initialize(this);}return (T)s;}
 }
}
namespace Game {
 public class GameSystemBase {public Unity.Entities.World World;public Unity.Entities.EntityManager EntityManager=>World.Manager;public void Initialize(Unity.Entities.World w){World=w;OnCreate();}protected virtual void OnCreate(){}protected virtual void OnDestroy(){}protected virtual void OnUpdate(){}protected Unity.Entities.EntityQuery GetEntityQuery(params Unity.Entities.EntityQueryDesc[] d)=>EntityManager.Query(d);protected Unity.Entities.EntityQuery GetEntityQuery(params Unity.Entities.ComponentType[] c)=>EntityManager.Query(new Unity.Entities.EntityQueryDesc{All=c});}
}
namespace Game.Common {public struct Deleted{}public struct BatchesUpdated{}public struct Owner{public Unity.Entities.Entity m_Owner;}}
namespace Game.Tools {public struct Hidden{}}
namespace Game.Objects {
 public struct Object{}
 public class SubObjectHiddenSystem:Game.GameSystemBase {
  private Unity.Entities.EntityQuery m_HiddenQuery;public Unity.Entities.EntityQuery Query=>m_HiddenQuery;
  protected override void OnCreate(){m_HiddenQuery=GetEntityQuery(Unity.Entities.ComponentType.ReadOnly<Game.Tools.Hidden>(),Unity.Entities.ComponentType.ReadOnly<Object>(),Unity.Entities.ComponentType.ReadOnly<Game.Common.Owner>());}
  // Relevant native 1.6.2f1 contract: ordinary hidden subobject + visible owner
  // => remove Hidden and mark its batches. Not a game renderer simulation.
  public void Tick(){using(var entities=m_HiddenQuery.ToEntityArray(Unity.Collections.Allocator.Temp))foreach(var e in entities)if(!EntityManager.HasComponent<Game.Tools.Hidden>(EntityManager.GetComponentData<Game.Common.Owner>(e).m_Owner)){EntityManager.RemoveComponent<Game.Tools.Hidden>(e);EntityManager.AddComponent<Game.Common.BatchesUpdated>(e);}}
 }
}
namespace TrafficLightIntoKorea {internal static class RuntimeDiagnostics {public static void Count(string text){}public static void Event(string text){}}}
