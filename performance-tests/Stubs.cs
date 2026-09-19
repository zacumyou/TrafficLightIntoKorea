using System;using System.Collections.Generic;
namespace Unity.Entities {
 public interface IComponentData {}
 public record struct Entity(int Id);
 public class EntityManager {
  int next;public readonly Dictionary<Entity,Dictionary<Type,object>> Data=new();public readonly Dictionary<Type,int> Writes=new();
  public Entity Create(){var e=new Entity(++next);Data[e]=new();return e;}
  public bool Exists(Entity e)=>Data.ContainsKey(e);
  public bool HasComponent<T>(Entity e)=>Exists(e)&&Data[e].ContainsKey(typeof(T));
  public bool HasBuffer<T>(Entity e)=>HasComponent<List<T>>(e);
  public List<T> GetBuffer<T>(Entity e,bool readOnly)=>(List<T>)Data[e][typeof(List<T>)];
  public T GetComponentData<T>(Entity e)=>(T)Data[e][typeof(T)];
  public void SetComponentData<T>(Entity e,T value){Writes.TryGetValue(typeof(T),out int old);Writes[typeof(T)]=old+1;Data[e][typeof(T)]=value;}
  public void AddComponentData<T>(Entity e,T value)=>Data[e][typeof(T)]=value;
  public void AddComponent<T>(Entity e)=>Data[e][typeof(T)]=default(T);
  public void RemoveComponent<T>(Entity e)=>Data[e].Remove(typeof(T));
 }
}
namespace Game.Common {public struct Deleted{} public struct Updated{} public struct BatchesUpdated{}}
namespace Game.Tools {public struct Temp{} public struct Hidden{} public struct Overridden{}}
namespace Game.Rendering {public struct CullingInfo{public int m_PassedCulling;}public struct MeshBatch{public int m_MeshGroup,m_MeshIndex,m_GroupIndex,m_InstanceIndex;}}
namespace UnityEngine {public static class Time {public static float realtimeSinceStartup;}}
namespace TrafficLightIntoKorea {public struct KoreanSignalClone{} public struct KoreanSignVisual{} public struct KoreanRetired{} public static class RuntimeDiagnostics{public static void Count(string name){}public static void Event(string text){}}}
