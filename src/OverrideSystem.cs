using System;using System.Collections.Generic;using Colossal.Mathematics;using Colossal.Serialization.Entities;
using Game;using Game.Prefabs;using Game.Rendering;using Game.Serialization;using Unity.Collections;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
public partial class OverrideSystem:GameSystemBase,IPreDeserialize {
 private bool _reloadRequested;
 internal readonly SignMounts Mounts=new SignMounts();
 internal SignMount[] SignLayout(Entity prefab)=>Mounts.Get(prefab,_prefabs,EntityManager);
 internal static int ReloadCount,ReloadMissing;
 internal void RequestReload(){_reloadRequested=true;_next=0;}
 private readonly ApproachProxies _approachProxies=new ApproachProxies();
 internal void PrepareQueuedProxies(){_approachProxies.Process(this,_prefabs,EntityManager);PrepareAccessoryVariants();}
 internal string ApproachProxyStatus(Entity source)=>_approachProxies.Status(source,_prefabs,EntityManager);
 internal Entity ApproachProxy(Entity source){var e=_approachProxies.Resolve(source,_prefabs,EntityManager);if(e!=Entity.Null)Mounts.Alias(e,source);return e;}
 private EntityQuery _query;private PrefabSystem _prefabs;private bool _locked,_stopped,_cityRequested;private bool _allow=true;private int _next;
 protected override void OnCreate(){base.OnCreate();var mode=Game.SceneFlow.GameManager.instance.gameMode;_locked=mode==GameMode.Game;_cityRequested=_locked;_allow=mode!=GameMode.Editor;Catalog.Locked=_locked;_prefabs=World.GetOrCreateSystemManaged<PrefabSystem>();_query=GetEntityQuery(ComponentType.ReadOnly<PrefabData>(),ComponentType.ReadOnly<TrafficLightData>(),ComponentType.ReadOnly<SubMesh>(),ComponentType.ReadOnly<ObjectGeometryData>(),ComponentType.Exclude<Game.Common.Deleted>());}
 protected override void OnGamePreload(Purpose purpose,GameMode mode){base.OnGamePreload(purpose,mode);_cityRequested=mode==GameMode.Game;_allow=mode==GameMode.Game||mode==GameMode.MainMenu;_locked=false;Catalog.Locked=false;_next=0;Mounts.Invalidate();InvalidateAccessories();Restore();}
 protected override void OnGameLoaded(Context context){base.OnGameLoaded(context);_locked=_cityRequested;Catalog.Locked=_locked;}
 public void PreDeserialize(Context context){if(_stopped)return;Apply();_locked=_cityRequested;Catalog.Locked=_locked;}
 protected override void OnUpdate(){if(_stopped||Mod.Settings==null)return;int now=Environment.TickCount;if(_next!=0&&unchecked(now-_next)<0)return;_next=unchecked(now+1000);if(_reloadRequested){_reloadRequested=false;Mounts.Invalidate();InvalidateAccessories();World.GetExistingSystemManaged<TrafficSignSystem>()?.Reload();_approachProxies.Invalidate();_regularReady.Clear();Apply(_locked);Catalog.Version++;ReloadCount++;ReloadMissing=0;foreach(var name in SourceNames.All){var choice=Mod.Settings.Selection(name);if(!string.IsNullOrEmpty(choice)&&!Catalog.Entries.ContainsKey(choice))ReloadMissing++;}foreach(var choice in new[]{Mod.Settings.StraightLeftVehicleAsset,Mod.Settings.StraightLeftCrosswalkAsset})if(!string.IsNullOrEmpty(choice)&&!Catalog.Entries.ContainsKey(choice))ReloadMissing++;World.GetExistingSystemManaged<ApproachSignalSystem>()?.RefreshAll();World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshAll();return;}if(_locked){PrepareApproachSelections();return;}Apply();}
 private readonly HashSet<Entity> _regularRequests=new HashSet<Entity>();
 private readonly HashSet<Entity> _regularReady=new HashSet<Entity>();
 internal Entity RegularProxy(Entity source){_regularRequests.Add(source);return ApproachProxy(source);}
 private void PrepareApproachSelections(){int prepared=0;foreach(var source in _regularRequests){if(ApproachProxy(source)!=Entity.Null){if(_regularReady.Add(source)){Catalog.Version++;World.GetExistingSystemManaged<ApproachSignalSystem>()?.RefreshAll();}}else if(prepared++<2)_approachProxies.Prepare(source,this,_prefabs,EntityManager);}foreach(var choice in new[]{Mod.Settings.StraightLeftVehicleAsset,Mod.Settings.StraightLeftCrosswalkAsset})if(!string.IsNullOrEmpty(choice)&&Catalog.Entries.TryGetValue(choice,out var target))_approachProxies.Prepare(target,this,_prefabs,EntityManager);}
 internal Entity VisualSource(Entity proxy){var source=AccessorySource(proxy);return source!=Entity.Null?source:_approachProxies.SourceOf(proxy,_prefabs);}
 internal bool IsOverridden(Entity source)=>false;
 internal void Stop(){_stopped=true;if(!_locked)Restore();}
 // Compatibility entry points intentionally do not mutate native prefab data.
 internal void BeginPlacement(){} internal void EndPlacement(){}
 internal void BeginSave(){} internal void EndSave(){}
 private SubMesh[] Meshes(Entity e){using(var a=EntityManager.GetBuffer<SubMesh>(e,true).ToNativeArray(Allocator.Temp))return a.ToArray();}
 private void Restore(){Catalog.Applied=0;}
 private void Apply(bool catalogOnly=false){
  if(Mod.Settings==null||!_allow)return;Catalog.Unavailable=0;
  EntityManager.CompleteDependencyBeforeRW<SubMesh>();EntityManager.CompleteDependencyBeforeRW<ObjectGeometryData>();
  var all=new Dictionary<string,Entity>(StringComparer.Ordinal);var candidates=new Dictionary<string,Entity>(StringComparer.Ordinal);Catalog.Alias=null;var vehicles=new HashSet<string>();var pedestrians=new HashSet<string>();
  using(var entities=_query.ToEntityArray(Allocator.Temp))foreach(var e in entities){if(!_prefabs.TryGetPrefab<StaticObjectPrefab>(e,out var p)||p.name.StartsWith(ApproachProxies.Prefix,StringComparison.Ordinal))continue;all[p.name]=e;int type=(int)EntityManager.GetComponentData<TrafficLightData>(e).m_Type;int mask=0;foreach(var mesh in Meshes(e))if(EntityManager.HasBuffer<ProceduralLight>(mesh.m_SubMesh))foreach(var light in EntityManager.GetBuffer<ProceduralLight>(mesh.m_SubMesh,true)){if(light.m_Purpose==EmissiveProperties.Purpose.TrafficLight_Red)mask|=1;if(light.m_Purpose==EmissiveProperties.Purpose.TrafficLight_Yellow)mask|=2;if(light.m_Purpose==EmissiveProperties.Purpose.TrafficLight_Green)mask|=4;if(light.m_Purpose==EmissiveProperties.Purpose.PedestrianLight_Stop)mask|=8;if(light.m_Purpose==EmissiveProperties.Purpose.PedestrianLight_Walk)mask|=16;}bool vehicle=(type&3)!=0&&(mask&7)==7;bool pedestrian=(type&12)!=0&&(mask&24)==24;if(vehicle)vehicles.Add(p.name);if(pedestrian)pedestrians.Add(p.name);if(vehicle||pedestrian){candidates[p.name]=e;if(p.asset?.id.guid.ToString()=="d08cd6e8269e4260ae481ab7db50f85d")Catalog.Alias=p.name;}}
  bool changed=candidates.Count!=Catalog.Entries.Count||!vehicles.SetEquals(Catalog.Vehicles)||!pedestrians.SetEquals(Catalog.Pedestrians);foreach(var p in candidates)if(!Catalog.Entries.TryGetValue(p.Key,out var old)||old!=p.Value)changed=true;if(changed){Catalog.Vehicles.Clear();Catalog.Vehicles.UnionWith(vehicles);Catalog.Pedestrians.Clear();Catalog.Pedestrians.UnionWith(pedestrians);Catalog.Entries.Clear();foreach(var p in candidates)Catalog.Entries.Add(p.Key,p.Value);Catalog.Version++;}
  foreach(var choice in new[]{Mod.Settings.StraightLeftVehicleAsset,Mod.Settings.StraightLeftCrosswalkAsset})if(!string.IsNullOrEmpty(choice)&&candidates.TryGetValue(choice,out var candidate))_approachProxies.Prepare(candidate,this,_prefabs,EntityManager);
 }

 internal bool Flatten(Entity prefab,float3 position,quaternion rotation,List<SubMesh> output,HashSet<Entity> path,int depth,HashSet<string> disabled=null,string route=null,Entity roadName=default){
  if(route==null)route=AccessoryLayout.Root(prefab,_prefabs);
  if(depth>0&&roadName!=Entity.Null&&RoadNameRules.Valid(AccessoryLayout.Root(prefab,_prefabs)))prefab=roadName;
  if(depth>0&&SignMounts.Kind(_prefabs,prefab)!=0)return true;
  if(depth>8||output.Count>64||!path.Add(prefab)||!EntityManager.HasBuffer<SubMesh>(prefab))return false;
  if(EntityManager.HasBuffer<SubMeshGroup>(prefab)&&EntityManager.GetBuffer<SubMeshGroup>(prefab,true).Length>0)return false;
  var parts=Meshes(prefab);if(parts.Length==0)return false;
  foreach(var original in parts){if(!EntityManager.HasComponent<MeshData>(original.m_SubMesh))return false;if(EntityManager.HasBuffer<ProceduralBone>(original.m_SubMesh)&&EntityManager.GetBuffer<ProceduralBone>(original.m_SubMesh,true).Length>0)return false;var part=original;part.m_Position=position+math.rotate(rotation,part.m_Position);part.m_Rotation=math.mul(rotation,part.m_Rotation);part.m_Flags=SubMeshFlags.HasTransform;if(depth==0||disabled==null||!disabled.Contains(AccessoryKeys.Key(route)))output.Add(part);if(output.Count>64)return false;}
  int childIndex=0;if(EntityManager.HasBuffer<Game.Prefabs.SubObject>(prefab))foreach(var child in EntityManager.GetBuffer<Game.Prefabs.SubObject>(prefab,true)){var childRoute=AccessoryKeys.Child(route,childIndex++,AccessoryLayout.Root(child.m_Prefab,_prefabs));
var p=position;var r=rotation;if(child.m_ParentIndex>=0){if(child.m_ParentIndex>=parts.Length)return false;var parent=parts[child.m_ParentIndex];p+=math.rotate(r,parent.m_Position);r=math.mul(r,parent.m_Rotation);}if(!Flatten(child.m_Prefab,p+math.rotate(r,child.m_Position),math.mul(r,child.m_Rotation),output,path,depth+1,disabled,childRoute,roadName))return false;}
  path.Remove(prefab);return true;
 }
}
}



