using System;using System.Collections.Generic;using Game.Prefabs;using Game.UI.Widgets;using Unity.Entities;
namespace TrafficLightIntoKorea {
internal static class ApproachCatalog {
 internal static bool Candidate(EntityManager em,string name,bool crosswalk,out Entity entity){
 entity=Entity.Null;if(string.IsNullOrEmpty(name)||!Catalog.Entries.TryGetValue(name,out var target)||!em.Exists(target)||!em.HasComponent<ObjectData>(target)||!em.HasComponent<TrafficLightData>(target)||!em.HasBuffer<SubMesh>(target))return false;
 int type=(int)em.GetComponentData<TrafficLightData>(target).m_Type;if((type&3)==0||((type&12)!=0)!=crosswalk)return false;
 var owner=World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<OverrideSystem>();var parts=new List<SubMesh>();if(owner==null||!owner.Flatten(target,Unity.Mathematics.float3.zero,Unity.Mathematics.quaternion.identity,parts,new HashSet<Entity>(),0))return false;foreach(var part in parts)if((part.m_Flags&~SubMeshFlags.HasTransform)!=0)return false;
 entity=target;return true;
 }
 internal static bool Resolve(EntityManager em,string name,bool crosswalk,out Entity entity){entity=Entity.Null;if(string.IsNullOrEmpty(name)||!Catalog.Entries.TryGetValue(name,out var target)||!em.Exists(target)||!em.HasComponent<TrafficLightData>(target))return false;int type=(int)em.GetComponentData<TrafficLightData>(target).m_Type;if((type&3)==0||((type&12)!=0)!=crosswalk)return false;entity=World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<OverrideSystem>().ApproachProxy(target);return entity!=Entity.Null;}
 internal static DropdownItem<string>[] Items(string selected,Settings s,bool crossing){var items=new List<DropdownItem<string>>{new DropdownItem<string>{value="",displayName=s.GetOptionLabelLocaleID("Original")}};var world=World.DefaultGameObjectInjectionWorld;if(world!=null&&world.IsCreated){var names=new List<string>(Catalog.Vehicles);names.Sort(StringComparer.Ordinal);foreach(var name in names)if(Candidate(world.EntityManager,name,crossing,out _))items.Add(new DropdownItem<string>{value=name,displayName=name});}if(!string.IsNullOrEmpty(selected)&&!items.Exists(x=>x.value==selected))items.Add(new DropdownItem<string>{value=selected,displayName=selected+" [—]"});return items.ToArray();}
}
}

