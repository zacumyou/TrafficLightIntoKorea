using System;
using System.Collections.Generic;
using System.Linq;
namespace Unity.Entities {
 public record struct Entity(int Id) {public static Entity Null=>default;}
 public struct EntityArchetype {public bool Valid;}
 public class EntityManager {
  int next;public readonly Dictionary<Entity,Dictionary<Type,object>> Data=new();
  public Entity CreateEntity(EntityArchetype a){var e=new Entity(++next);Data[e]=new();return e;}
  public bool Exists(Entity e)=>Data.ContainsKey(e);
  public bool HasComponent<T>(Entity e)=>Exists(e)&&Data[e].ContainsKey(typeof(T));
  public T GetComponentData<T>(Entity e)=>(T)Data[e][typeof(T)];
  public void SetComponentData<T>(Entity e,T v)=>Data[e][typeof(T)]=v;
  public void AddComponent<T>(Entity e)=>Data[e][typeof(T)]=default(T);
  public void AddComponentData<T>(Entity e,T v)=>SetComponentData(e,v);
  public void RemoveComponent<T>(Entity e)=>Data[e].Remove(typeof(T));
 }
 public class World {public TrafficLightIntoKorea.KoreanVisualLifecycleSystem Lifecycle;public T GetOrCreateSystemManaged<T>()=>(T)(object)Lifecycle;}
}
namespace Unity.Mathematics {
 public record struct float3(float x,float y,float z);
 public record struct quaternion(float Value);
 public static class math {public static bool all(bool value)=>value;public static bool isfinite(float3 p)=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z);}
}
namespace Colossal.Serialization.Entities {public enum Purpose {Load}}
namespace Game {
 public enum GameMode {Game}
 public abstract class GameSystemBase {
  public Unity.Entities.EntityManager EntityManager=new();public Unity.Entities.World World=new();
  protected abstract void OnUpdate();public void Tick()=>OnUpdate();
  protected virtual void OnGamePreload(Colossal.Serialization.Entities.Purpose p,GameMode m){}
  public void Preload()=>OnGamePreload(default,default);
 }
}
namespace Game.Common {public struct Deleted {} public struct Created {} public struct Updated {} public struct BatchesUpdated {} public struct Owner {public Unity.Entities.Entity m_Owner;}}
namespace Game.Tools {public struct Hidden {} public struct Highlighted {}}
namespace Game.Objects {public struct Transform {public Unity.Mathematics.float3 m_Position;public Unity.Mathematics.quaternion m_Rotation;}public struct TrafficLight {public int State;}}
namespace Game.Prefabs {public struct ObjectData {public Unity.Entities.EntityArchetype m_Archetype;}public struct PrefabRef {public Unity.Entities.Entity m_Prefab;}}
namespace TrafficLightIntoKorea {
 public struct KoreanRetired {} public struct KoreanSignalClone {} public struct FarSignal {}
 public static class RuntimeDiagnostics {public static void Event(string message){}}
 public static class SignalRenderReadiness {public static void Protect(Unity.Entities.EntityManager em,Unity.Entities.Entity e){}}
 public static class KoreanJunctionSystem {public static bool Selected(Unity.Entities.EntityManager em,Unity.Entities.Entity e)=>em.Exists(e);}
 public class KoreanVisualLifecycleSystem {
  public Unity.Entities.EntityManager EM;public readonly List<Unity.Entities.Entity> Pending=new();
  public void Retire(Unity.Entities.Entity e){EM.AddComponent<KoreanRetired>(e);EM.AddComponent<Game.Tools.Hidden>(e);Pending.Add(e);}
  public void Tick(){foreach(var e in Pending)EM.AddComponent<Game.Common.Deleted>(e);Pending.Clear();}
 }
}
