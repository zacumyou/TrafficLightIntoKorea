using System;using Game.Prefabs;using Game.Objects;using Unity.Entities;
namespace TrafficLightIntoKorea {
internal static class FarVehicleRules {
 internal static string Counterpart(string source,int type){
 if(string.IsNullOrEmpty(source))return null;
 string theme=source.StartsWith("EU_TrafficLight",StringComparison.Ordinal)?"EU":source.StartsWith("NA_TrafficLight",StringComparison.Ordinal)?"NA":null;
 if(theme==null)return null;
 switch(type&3){case 1:return theme+"_TrafficLightCarLeft01";case 2:return theme+"_TrafficLightCarRight01";case 3:return theme+"_TrafficLightCar01";default:return null;}
 }
 internal static TrafficLight VehicleState(TrafficLight source){source.m_GroupMask1=0;source.m_State=(TrafficLightState)((int)source.m_State&15);return source;}
}
public partial class FarSignalSystem {
 private Entity VehiclePrefab(Entity source){
 var data=EntityManager.GetComponentData<TrafficLightData>(source);
 if(((int)data.m_Type&12)==0)return source;
 var system=World.GetOrCreateSystemManaged<PrefabSystem>();
 if(!system.TryGetPrefab<StaticObjectPrefab>(source,out var original))return Entity.Null;
 string name=FarVehicleRules.Counterpart(original.name,(int)data.m_Type);
 if(name==null||!system.TryGetPrefab(new PrefabID(nameof(StaticObjectPrefab),name),out var prefab)||!system.TryGetEntity(prefab,out var result))return Entity.Null;
 if(!EntityManager.HasComponent<ObjectData>(result)||!EntityManager.HasComponent<TrafficLightData>(result)||((int)EntityManager.GetComponentData<TrafficLightData>(result).m_Type&12)!=0)return Entity.Null;
 // This is the original vehicle template entity, whose visual was already
 // overridden before loading; use its independently initialized render data.
 return result;
 }
}
}
