using System.Collections.Generic;
using Game;using Game.Common;using Game.Objects;using Game.Prefabs;using Game.Tools;
using Unity.Entities;using Unity.Collections;
namespace TrafficLightIntoKorea {
// Only this opt-in tag is saved; visual clones remain transient.
public struct KoreanJunction:IComponentData,Colossal.Serialization.Entities.IEmptySerializable {}
public partial class KoreanJunctionSystem:GameSystemBase {
 internal static int SelectedCount;
 internal static KoreanJunctionSystem Active;
 internal static string RestoreStatus="";
 private readonly SignQueue<KeyValuePair<Entity,bool>> _commands=new SignQueue<KeyValuePair<Entity,bool>>();
 private int _restoreTotal,_restoreDone;
 internal bool IsRestoring=>_restoreRequested||_restore.Count>0;
 internal void RequestSelect(Entity node,bool on){if(!_ready||IsRestoring)return;World.GetExistingSystemManaged<IndividualSignalData>()?.TraceNode(node,on?"toggle-on":"toggle-off");_commands.Enqueue(new KeyValuePair<Entity,bool>(node,on));RuntimeDiagnostics.Event("Junction command "+node+" korean="+on);}
 internal static void RestoreAllFromSettings(){if(Active==null||!Active._ready){RestoreStatus="도시를 불러온 후 사용하세요 / Load a city first";return;}Active.RequestRestoreAll();}

 private bool _restoreRequested;private readonly SignQueue<Entity> _restore=new SignQueue<Entity>();
 internal void RequestRestoreAll(){if(!_ready)return;World.GetExistingSystemManaged<IndividualSignalData>()?.ForgetAllNodes();_commands.Clear();_restoreRequested=true;RestoreStatus="복원 요청 접수 / Restore queued";RuntimeDiagnostics.Event("Restore all requested");}
 private bool RestoreBatch(){if(!IsRestoring)return false;if(_restoreRequested){_restoreRequested=false;_restore.Clear();var owners=new HashSet<Entity>(_nodes);using(var a=_selected.ToEntityArray(Allocator.Temp))foreach(var e in a)owners.Add(e);
 World.GetExistingSystemManaged<ApproachSignalSystem>()?.CollectOwners(owners);World.GetExistingSystemManaged<FarSignalSystem>()?.CollectOwners(owners);World.GetExistingSystemManaged<LeftSignalSystem>()?.CollectOwners(owners);
 if(BetterBulldozerRestore.Available)using(var a=_nativeSignalNodes.ToEntityArray(Allocator.Temp))foreach(var e in a)if(BetterBulldozerRestore.CanRestore(EntityManager,e))owners.Add(e);
 foreach(var e in owners)_restore.Enqueue(e);_restoreTotal=_restore.Count;_restoreDone=0;RuntimeDiagnostics.Event("Restore all targets="+_restoreTotal);}
 int budget=BetterBulldozerRestore.Available?1:16;while(budget-->0&&_restore.Count>0){Select(_restore.Dequeue(),false);_restoreDone++;}
 RestoreStatus="기본 복원 / Restore "+_restoreDone+" / "+_restoreTotal;
 if(_restore.Count==0){_nodes.Clear();_known.Clear();_watch.Clear();_cursor=0;SelectedCount=0;RestoreStatus="기본 복원 완료 / Restore complete: "+_restoreDone;RuntimeDiagnostics.Event("Restore all completed="+_restoreDone);}
 return true;}

 private EntityQuery _sounds;private EntityQuery _nativeSignalNodes;private EntityQuery _selected;private readonly List<Entity> _nodes=new List<Entity>();private readonly HashSet<Entity> _known=new HashSet<Entity>();private int _cursor;private bool _ready,_loaded;private int _version=-1;private readonly JunctionWatch _watch=new JunctionWatch();private float _nextSettings;private string _settings="";
 internal static bool Selected(EntityManager em,Entity e)=>e!=Entity.Null&&em.Exists(e)&&!em.HasComponent<Deleted>(e)&&!em.HasComponent<Temp>(e)&&em.HasComponent<KoreanJunction>(e);
 protected override void OnCreate(){base.OnCreate();Active=this;_sounds=GetEntityQuery(ComponentType.ReadOnly<ToolUXSoundSettingsData>());_nativeSignalNodes=GetEntityQuery(ComponentType.ReadOnly<Game.Net.Node>(),ComponentType.ReadOnly<Game.Net.TrafficLights>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>());_selected=GetEntityQuery(ComponentType.ReadOnly<KoreanJunction>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>());}
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose p,GameMode m){_commands.Clear();RestoreStatus="";_restoreRequested=false;_restore.Clear();_ready=_loaded=false;_nodes.Clear();_known.Clear();_watch.Clear();_cursor=0;_version=-1;SelectedCount=0;base.OnGamePreload(p,m);}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose p,GameMode m){base.OnGameLoadingComplete(p,m);_ready=m==GameMode.Game;}
 internal void Select(Entity node,bool on){if(!_ready)return;if(on&&(!EntityManager.Exists(node)||EntityManager.HasComponent<Deleted>(node)||EntityManager.HasComponent<Temp>(node)))return;
  if(on){if(!EntityManager.HasComponent<Game.Net.Node>(node))return;if(_restoreRequested||_restore.Count>0)return;if(!EntityManager.HasComponent<KoreanJunction>(node))EntityManager.AddComponent<KoreanJunction>(node);Track(node);SelectedCount=_nodes.Count;Refresh(node);}
  else{World.GetExistingSystemManaged<IndividualSignalData>()?.ForgetNode(node);_watch.Untrack(node);World.GetExistingSystemManaged<DirectionalLightSystem>()?.RemoveOwner(node);if(EntityManager.Exists(node)&&EntityManager.HasComponent<KoreanJunction>(node))EntityManager.RemoveComponent<KoreanJunction>(node);World.GetExistingSystemManaged<LeftSignalSystem>()?.RemoveOwner(node);World.GetExistingSystemManaged<ApproachSignalSystem>()?.RemoveOwner(node);World.GetExistingSystemManaged<FarSignalSystem>()?.RemoveOwner(node);if(_known.Remove(node))_nodes.Remove(node);SelectedCount=_nodes.Count;BetterBulldozerRestore.Restore(EntityManager,node);}
 }
 private void Track(Entity e){if(_known.Add(e))_nodes.Add(e);_watch.Track(e);}
 private void Refresh(Entity e){if(!Selected(EntityManager,e))return;World.GetExistingSystemManaged<ApproachSignalSystem>()?.RefreshOwner(e);World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshOwner(e);World.GetExistingSystemManaged<LeftSignalSystem>()?.RefreshOwner(e);World.GetExistingSystemManaged<DirectionalLightSystem>()?.RefreshOwner(e);World.GetExistingSystemManaged<TrafficSignSystem>()?.RefreshOwner(e);}
 internal void PlayBuildSound(){if(_sounds.CalculateEntityCount()!=1)return;var sound=_sounds.GetSingleton<ToolUXSoundSettingsData>().m_NetBuildSound;if(sound!=Entity.Null)World.GetExistingSystemManaged<Game.Audio.AudioManager>()?.PlayUISound(sound);}
 protected override void OnDestroy(){if(Active==this)Active=null;base.OnDestroy();}
 protected override void OnUpdate(){if(!_ready)return;if(RestoreBatch())return;for(int i=0;i<1&&_commands.Count>0;i++){var c=_commands.Dequeue();if(EntityManager.Exists(c.Key)&&!EntityManager.HasComponent<Deleted>(c.Key)&&!EntityManager.HasComponent<Temp>(c.Key)){Select(c.Key,c.Value);PlayBuildSound();}}SelectedCount=_nodes.Count;if(!_loaded){using(var a=_selected.ToEntityArray(Allocator.Temp))foreach(var e in a){Track(e);Refresh(e);}_loaded=true;}
  if(_version!=Catalog.Version){_version=Catalog.Version;foreach(var e in _nodes)Refresh(e);}
  if(UnityEngine.Time.realtimeSinceStartup>=_nextSettings&&Mod.Settings!=null){_nextSettings=UnityEngine.Time.realtimeSinceStartup+1;var s=Mod.Settings;string key=s.Enabled+"/"+s.UseStraightLeftAssets+"/"+s.StraightLeftVehicleAsset+"/"+s.StraightLeftCrosswalkAsset+"/"+s.AddFarSignals+"/"+s.HideLeftReplacedLights+"/"+s.FarSidewalkInset;foreach(var name in SourceNames.All)key+="/"+s.Selection(name);if(key!=_settings){_settings=key;foreach(var e in _nodes)Refresh(e);}}
  _watch.Tick(EntityManager,Refresh);
  // Reconcile one selected junction per update, including native signal removal.
  if(_nodes.Count==0)return;if(_cursor>=_nodes.Count)_cursor=0;var node=_nodes[_cursor];if(!Selected(EntityManager,node)){World.GetExistingSystemManaged<ApproachSignalSystem>()?.RemoveOwner(node);World.GetExistingSystemManaged<FarSignalSystem>()?.RemoveOwner(node);World.GetExistingSystemManaged<LeftSignalSystem>()?.RemoveOwner(node);_watch.Untrack(node);World.GetExistingSystemManaged<DirectionalLightSystem>()?.RemoveOwner(node);_known.Remove(node);_nodes[_cursor]=_nodes[_nodes.Count-1];_nodes.RemoveAt(_nodes.Count-1);}else _cursor++;
 }
}
}
