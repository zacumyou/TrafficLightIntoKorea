using System;using System.Collections.Generic;
namespace Unity.Entities {
 public interface IComponentData{}
 public record struct Entity(int Id){public static Entity Null=>default;}
 public class EntityManager {
  private readonly Dictionary<Entity,Dictionary<Type,object>> _data=new();private int _id;
  public int DeletedWrites;
  public Entity Create(){var e=new Entity(++_id);_data[e]=new();return e;}
  public bool Exists(Entity e)=>_data.ContainsKey(e);
  public bool HasComponent<T>(Entity e)=>Exists(e)&&_data[e].ContainsKey(typeof(T));
  public T GetComponentData<T>(Entity e)=>(T)_data[e][typeof(T)];
  public void AddComponent<T>(Entity e){if(!HasComponent<T>(e)){_data[e][typeof(T)]=default(T);if(typeof(T)==typeof(Game.Common.Deleted))DeletedWrites++;}}
  public void AddComponentData<T>(Entity e,T data)=>_data[e][typeof(T)]=data;
  public void RemoveComponent<T>(Entity e)=>_data[e].Remove(typeof(T));
  public void Destroy(Entity e)=>_data.Remove(e);
 }
}
namespace Game {
 public enum GameMode {Game}
 public class GameSystemBase {
  public Unity.Entities.EntityManager EntityManager=new();
  protected virtual void OnUpdate(){}
  protected virtual void OnGamePreload(Colossal.Serialization.Entities.Purpose p,GameMode m){}
  protected virtual void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose p,GameMode m){}
  public void Tick()=>OnUpdate();public void Preload()=>OnGamePreload(default,GameMode.Game);
 }
}
namespace Colossal.Serialization.Entities {public enum Purpose {SaveGame}}
namespace Game.Common {public struct Deleted{}public struct BatchesUpdated{}public struct Owner {public Unity.Entities.Entity m_Owner;}}
namespace Game.Tools {public struct Hidden{}public struct Temp{}}
namespace Game.Net {public struct Node{}public struct TrafficLights{}}
namespace TrafficLightIntoKorea {internal static class RuntimeDiagnostics {internal static void Count(string name){}}}
