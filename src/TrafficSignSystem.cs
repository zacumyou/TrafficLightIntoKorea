using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game;
using Game.Common;
using Game.Objects;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
namespace TrafficLightIntoKorea {
public struct KoreanSignVisual:IComponentData {}
public struct KoreanSignalClone:IComponentData {}
// Independent sign instances keep native signal prefab identity, timing and lane data intact.
public partial class TrafficSignSystem:GameSystemBase {
 private sealed class Entry {internal Entity Parent,Prefab,Owner;internal SignMount[] Mounts;internal Entity[] Signs,Targets;internal SignChoice[] Choices;internal float MissingApproachSince=-1;internal readonly HashSet<Entity> HiddenChildren=new HashSet<Entity>();internal SignRoadState Road;internal float2 Incoming;internal int Index;internal float NextApproach;internal int SyncFrame=-1;internal float NextReconcile,NextSyncCheck;internal bool HasSync,LastHidden;internal Transform LastTransform;}
 private struct RoadKey:IEquatable<RoadKey>{internal Entity Owner,Edge;public bool Equals(RoadKey other)=>Owner==other.Owner&&Edge==other.Edge;public override bool Equals(object obj)=>obj is RoadKey other&&Equals(other);public override int GetHashCode()=>Owner.GetHashCode()*397^Edge.GetHashCode();}
 private readonly Dictionary<RoadKey,SignRoadState> _roadStates=new Dictionary<RoadKey,SignRoadState>();
 private readonly List<SignRoadState> _roadRound=new List<SignRoadState>();
 private readonly Dictionary<Entity,Entry> _entries=new Dictionary<Entity,Entry>();
 private readonly List<Entry> _round=new List<Entry>();
 private readonly SignQueue<Entity> _pending=new SignQueue<Entity>();private readonly HashSet<Entity> _queued=new HashSet<Entity>();
 private readonly Dictionary<Entity,HashSet<Entity>> _owners=new Dictionary<Entity,HashSet<Entity>>();
 private readonly SignalStartupScan _startup=new SignalStartupScan();
 private readonly SignAssets _assets=new SignAssets();private PrefabSystem _prefabs;private OverrideSystem _overrides;
 private EntityQuery _signals,_changed,_roads,_deleted;private bool _ready,_saving;private int _cursor,_visibilityCursor,_phaseCursor,_lastCatalog=-1,_lastAssets;
 private float _nextStatus;private int _processed,_created;private double _lastMs,_maxMs;internal static string Status="";
 internal bool[] MountKinds(Entity parent){var found=new bool[3];if(EntityManager.Exists(parent)&&EntityManager.HasComponent<PrefabRef>(parent))foreach(var mount in _overrides.SignLayout(EntityManager.GetComponentData<PrefabRef>(parent).m_Prefab))if(mount.Kind>=1&&mount.Kind<=3)found[mount.Kind-1]=true;return found;}
 internal string[] AvailableSigns(){var list=new List<string>();foreach(var name in SignRules.Names)if(_assets.Resolve(name,_prefabs,EntityManager)!=Entity.Null)list.Add(name);return list.ToArray();}
 internal void PrepareAssets(){_assets.Prepare(_prefabs,EntityManager);}
 internal void Reload(){_assets.Retry();_startup.Request();foreach(var e in _round){e.Prefab=Entity.Null;Queue(e.Parent);}}
 protected override void OnCreate(){base.OnCreate();_prefabs=World.GetOrCreateSystemManaged<PrefabSystem>();_overrides=World.GetOrCreateSystemManaged<OverrideSystem>();
  _signals=GetEntityQuery(ComponentType.ReadOnly<KoreanSignalClone>(),ComponentType.ReadOnly<TrafficLight>(),ComponentType.ReadOnly<Transform>(),ComponentType.ReadOnly<PrefabRef>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>());
  _changed=GetEntityQuery(new EntityQueryDesc{All=new[]{ComponentType.ReadOnly<KoreanSignalClone>(),ComponentType.ReadOnly<TrafficLight>(),ComponentType.ReadOnly<Transform>(),ComponentType.ReadOnly<PrefabRef>()},Any=new[]{ComponentType.ReadOnly<Created>(),ComponentType.ReadOnly<Updated>()},None=new[]{ComponentType.ReadOnly<Deleted>(),ComponentType.ReadOnly<Temp>()}});
  _roads=GetEntityQuery(ComponentType.ReadOnly<Game.Net.Edge>(),ComponentType.ReadOnly<Updated>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>());
  _deleted=GetEntityQuery(ComponentType.ReadOnly<KoreanSignalClone>(),ComponentType.ReadOnly<TrafficLight>(),ComponentType.ReadOnly<Deleted>());
 }
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose p,GameMode mode){Clear();_ready=false;_assets.Retry();base.OnGamePreload(p,mode);}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose p,GameMode mode){base.OnGameLoadingComplete(p,mode);_ready=mode==GameMode.Game;_startup.Reset();_assets.Retry();}
 protected override void OnDestroy(){_startup.Dispose();base.OnDestroy();}
 private bool Live(Entity e)=>e!=Entity.Null&&EntityManager.Exists(e)&&!EntityManager.HasComponent<Deleted>(e)&&!EntityManager.HasComponent<KoreanRetired>(e);
 internal void Queue(Entity parent){if(parent!=Entity.Null&&_queued.Add(parent))_pending.Enqueue(parent);}
 internal void Forget(Entity parent){if(_entries.TryGetValue(parent,out var entry))Remove(entry);}
 internal void RefreshOwner(Entity owner){if(_owners.TryGetValue(owner,out var parents))foreach(var parent in parents){if(_entries.TryGetValue(parent,out var e)&&e.Road!=null){e.Road.NextRefresh=0;e.NextApproach=0;}Queue(parent);}}
 private Entity Original(Entity e)=>World.GetExistingSystemManaged<DirectionalLightSystem>()?.OriginalSource(e)??e;
 private Entity OwnerOf(Entity e){e=Original(e);for(int i=0;i<8&&Live(e)&&EntityManager.HasComponent<Owner>(e);i++){e=EntityManager.GetComponentData<Owner>(e).m_Owner;if(EntityManager.HasBuffer<Game.Net.SubLane>(e)&&EntityManager.HasBuffer<Game.Net.ConnectedEdge>(e))return e;}return Entity.Null;}
 private void Mark(Entity e){if(Live(e)&&!EntityManager.HasComponent<BatchesUpdated>(e))EntityManager.AddComponent<BatchesUpdated>(e);}
 private void Delete(Entity e){if(Live(e))World.GetOrCreateSystemManaged<KoreanVisualLifecycleSystem>().Retire(e);}
 private void Hidden(Entity e,bool hide){if(!Live(e))return;bool old=EntityManager.HasComponent<Hidden>(e);if(old==hide)return;if(hide)PersistentSignalVisibility.Hide(EntityManager,e);else PersistentSignalVisibility.Show(EntityManager,e);Mark(e);}
 private void ReleaseRoad(Entry entry){var state=entry.Road;if(state==null)return;entry.Road=null;if(--state.Users>0)return;_roadStates.Remove(new RoadKey{Owner=state.Owner,Edge=state.Edge});int last=_roadRound.Count-1;_roadRound[state.Index]=_roadRound[last];_roadRound[state.Index].Index=state.Index;_roadRound.RemoveAt(last);}
 private void Release(Entry entry){ReleaseRoad(entry);foreach(var e in entry.Signs)Delete(e);foreach(var e in entry.HiddenChildren)Hidden(e,false);entry.HiddenChildren.Clear();if(_owners.TryGetValue(entry.Owner,out var set)){set.Remove(entry.Parent);if(set.Count==0)_owners.Remove(entry.Owner);}}
 private void Remove(Entry entry){Release(entry);_entries.Remove(entry.Parent);int last=_round.Count-1;_round[entry.Index]=_round[last];_round[entry.Index].Index=entry.Index;_round.RemoveAt(last);}
 internal void Clear(){_saving=false;foreach(var e in _round)Release(e);_entries.Clear();_round.Clear();_roadStates.Clear();_roadRound.Clear();_owners.Clear();_pending.Clear();_queued.Clear();_startup.Reset();_cursor=_visibilityCursor=_phaseCursor=0;}
 protected override void OnUpdate(){using(RuntimeDiagnostics.Measure("TrafficSignSystem.OnUpdate")){if(_saving)return;if(!_ready||Mod.Settings==null||!Mod.Settings.Enabled){if(_round.Count>0)Clear();return;}
  var timer=Stopwatch.StartNew();_created=0;_processed=0;
  if(_lastCatalog!=Catalog.Version||_lastAssets!=_assets.Count){_lastCatalog=Catalog.Version;_lastAssets=_assets.Count;_startup.Request();}
  _startup.Tick(_signals,e=>e,Queue,UnityEngine.Time.realtimeSinceStartup);
  using(var deleted=_deleted.ToEntityArray(Allocator.Temp))foreach(var e in deleted)Queue(e);
  using(var changed=_changed.ToEntityArray(Allocator.Temp))foreach(var e in changed)Queue(e);
  
  // Give both new work and existing objects a slice; neither can starve the other.
  for(int i=0;i<4&&_pending.Count>0&&timer.Elapsed.TotalMilliseconds<1.5;i++){var e=_pending.Dequeue();_queued.Remove(e);Reconcile(e);_processed++;}
  for(int i=0,limit=Math.Min(4,_round.Count);i<limit&&timer.Elapsed.TotalMilliseconds<2;i++){if(_cursor>=_round.Count)_cursor=0;var e=_round[_cursor++];if(!SignalMaintenanceRules.Due(UnityEngine.Time.realtimeSinceStartup,e.NextReconcile))continue;Reconcile(e.Parent);_processed++;}
  // Reads only cached masks and controller state, no mesh rebuilding or lane scans.
  for(int i=0;i<128&&i<_roadRound.Count&&timer.Elapsed.TotalMilliseconds<2;i++){if(_phaseCursor>=_roadRound.Count)_phaseCursor=0;var state=_roadRound[_phaseCursor++];int old=state.Observed;state.Observe(EntityManager);if(old!=state.Observed&&_owners.TryGetValue(state.Owner,out var parents))foreach(var parent in parents)Queue(parent);}
  _lastMs=timer.Elapsed.TotalMilliseconds;_maxMs=Math.Max(_maxMs,_lastMs);if(UnityEngine.Time.realtimeSinceStartup<_nextStatus)return;_nextStatus=UnityEngine.Time.realtimeSinceStartup+.5f;Status="controllers="+SignPhaseController.Available+" assets="+_assets.Count+"/14 approaches="+_roadStates.Count+" parents="+_round.Count+" pending="+_pending.Count+" processed="+_processed+" created="+_created+" ms="+_lastMs.ToString("F2")+" peak="+_maxMs.ToString("F2");
 }}
 private void Reconcile(Entity parent){
  if(!Live(parent)||!EntityManager.HasComponent<PrefabRef>(parent)||!EntityManager.HasComponent<Transform>(parent)){if(_entries.TryGetValue(parent,out var dead))Remove(dead);return;}
  var prefab=EntityManager.GetComponentData<PrefabRef>(parent).m_Prefab;var owner=OwnerOf(parent);
  if(!Live(owner)){if(_entries.TryGetValue(parent,out var orphan))Remove(orphan);return;}
  _entries.TryGetValue(parent,out var entry);
  if(entry==null||entry.Prefab!=prefab||entry.Owner!=owner){if(entry!=null)Remove(entry);var mounts=_overrides.SignLayout(prefab);if(mounts.Length==0)return;
   entry=new Entry{Parent=parent,Prefab=prefab,Owner=owner,Mounts=mounts,Signs=new Entity[mounts.Length],Targets=new Entity[mounts.Length],Choices=new SignChoice[mounts.Length],Index=_round.Count};_round.Add(entry);_entries[parent]=entry;
   if(!_owners.TryGetValue(owner,out var set)){set=new HashSet<Entity>();_owners.Add(owner,set);}set.Add(parent);
  }
  float now=UnityEngine.Time.realtimeSinceStartup;entry.NextReconcile=now+SignalMaintenanceRules.SignInterval;RuntimeDiagnostics.Count("sign.reconcile");
  if(now>=entry.NextApproach){var original=Original(parent);Entity edge=Entity.Null;float2 direction=0;
   if(Live(original)&&EntityManager.HasComponent<Transform>(original)&&SignalApproach.Try(EntityManager,owner,EntityManager.GetComponentData<Transform>(original),out int index,out direction))foreach(var item in EntityManager.GetBuffer<Game.Net.ConnectedEdge>(owner,true))if(item.m_Edge.Index==index){edge=item.m_Edge;break;}
   entry.NextApproach=now+10;
   if(edge==Entity.Null){if(entry.MissingApproachSince<0)entry.MissingApproachSince=now;if(now-entry.MissingApproachSince>=10)ReleaseRoad(entry);entry.NextApproach=now+1;}else{entry.MissingApproachSince=-1;if(entry.Road==null||entry.Road.Edge!=edge){ReleaseRoad(entry);var key=new RoadKey{Owner=owner,Edge=edge};if(!_roadStates.TryGetValue(key,out var state)){state=new SignRoadState{Owner=owner,Edge=edge,Index=_roadRound.Count};_roadStates.Add(key,state);_roadRound.Add(state);}state.Users++;entry.Road=state;}entry.Incoming=direction;}
  }
  if(entry.Road!=null&&now>=entry.Road.NextRefresh){entry.Road.Read(EntityManager,entry.Incoming);entry.Road.NextRefresh=now+2;entry.Road.Observe(EntityManager);
  }
  HideMarkers(parent,entry,0);
  if(Suppressed(parent)){RetireSigns(entry);return;}
  SignalChoice selected=null;bool far=false;if(EntityManager.HasComponent<IndividualSignalLink>(parent)){var link=EntityManager.GetComponentData<IndividualSignalLink>(parent);far=link.Far;selected=World.GetExistingSystemManaged<IndividualSignalData>()?.Get(link.Owner,link.Source);}var selections=far?selected?.farSigns:selected?.nearSigns;for(int i=0;i<entry.Mounts.Length;i++){
   int kind=entry.Mounts[i].Kind;bool speedSlot=kind==1;
   string candidate=speedSlot?entry.Road?.Speed:kind==3?entry.Road?.Ban:entry.Road?.Mode;
   bool known=entry.Road!=null&&entry.Road.Valid&&(speedSlot?entry.Road.SpeedKnown:kind==3?entry.Road.BanKnown:entry.Road.ModeKnown);
   var pick=selections!=null&&kind>0&&kind<=selections.Length?selections[kind-1]??"auto":"auto";if(pick!="auto"){candidate=pick=="none"?null:pick;known=true;}
   if(entry.Choices[i]==null)entry.Choices[i]=new SignChoice();
   var resolved=_assets.Resolve(candidate,_prefabs,EntityManager);if(candidate!=null&&resolved==Entity.Null)known=false;
   string name=entry.Choices[i].Update(candidate,known,now);var target=_assets.Resolve(name,_prefabs,EntityManager);
   if(name!=null&&target==Entity.Null&&Live(entry.Signs[i]))continue;
   if(target!=entry.Targets[i]||!Live(entry.Signs[i])){Delete(entry.Signs[i]);entry.Signs[i]=Entity.Null;entry.Targets[i]=target;
    if(target!=Entity.Null){var e=EntityManager.CreateEntity(EntityManager.GetComponentData<ObjectData>(target).m_Archetype);EntityManager.AddComponent<KoreanSignVisual>(e);EntityManager.AddComponentData(e,new Owner{m_Owner=parent});EntityManager.SetComponentData(e,new PrefabRef{m_Prefab=target});entry.Signs[i]=e;_created++;}
   }
  }
  entry.HasSync=false;entry.SyncFrame=-1;Sync(entry);
 }
 private void HideMarkers(Entity parent,Entry entry,int depth){if(depth>=8||entry.HiddenChildren.Count>=64||!EntityManager.HasBuffer<Game.Objects.SubObject>(parent))return;
  // Snapshot: adding Hidden changes chunks and invalidates dynamic buffers.
  using(var children=EntityManager.GetBuffer<Game.Objects.SubObject>(parent,true).ToNativeArray(Allocator.Temp))foreach(var child in children){var e=child.m_SubObject;if(!Live(e)||!EntityManager.HasComponent<PrefabRef>(e))continue;int kind=SignMounts.Kind(_prefabs,EntityManager.GetComponentData<PrefabRef>(e).m_Prefab);if(kind!=0){if(!EntityManager.HasComponent<Hidden>(e)){Hidden(e,true);entry.HiddenChildren.Add(e);}}else HideMarkers(e,entry,depth+1);}
 }
 private bool Suppressed(Entity parent){if(!EntityManager.HasComponent<IndividualSignalLink>(parent))return false;var link=EntityManager.GetComponentData<IndividualSignalLink>(parent);return !link.Far&&(World.GetExistingSystemManaged<ApproachSignalSystem>()?.NearSuppressed(link.Source)??false);}
 private void RetireSigns(Entry entry){for(int i=0;i<entry.Signs.Length;i++){Delete(entry.Signs[i]);entry.Signs[i]=Entity.Null;}}
 private void Sync(Entry entry){if(Suppressed(entry.Parent)){RetireSigns(entry);return;}int frame=UnityEngine.Time.frameCount;entry.SyncFrame=frame;bool live=Live(entry.Parent)&&Live(entry.Owner);bool hidden=!live||EntityManager.HasComponent<Hidden>(entry.Parent)||(EntityManager.HasComponent<KoreanSignalClone>(entry.Parent)&&!SignalRenderReadiness.MainReady(EntityManager,entry.Parent));if(!live){foreach(var e in entry.Signs)Hidden(e,true);return;}
  var t=EntityManager.GetComponentData<Transform>(entry.Parent);float now=UnityEngine.Time.realtimeSinceStartup;if(entry.HasSync&&entry.LastHidden==hidden&&entry.LastTransform.Equals(t)&&!SignalMaintenanceRules.Due(now,entry.NextSyncCheck))return;entry.HasSync=true;entry.LastHidden=hidden;entry.LastTransform=t;entry.NextSyncCheck=now+SignalMaintenanceRules.SignInterval;RuntimeDiagnostics.Count("sign.sync");for(int i=0;i<entry.Signs.Length;i++){var e=entry.Signs[i];if(!Live(e))continue;if(!hidden)SignalRenderReadiness.Protect(EntityManager,e);var mount=entry.Mounts[i];var next=new Transform{m_Position=t.m_Position+math.rotate(t.m_Rotation,mount.Position),m_Rotation=math.mul(t.m_Rotation,mount.Rotation)};
   if(!next.Equals(EntityManager.GetComponentData<Transform>(e))){RuntimeDiagnostics.Count("sign.transformWrite");EntityManager.SetComponentData(e,next);Mark(e);}Hidden(e,hidden);}
 }
 internal void SyncParent(Entity parent){if(!_saving&&_entries.TryGetValue(parent,out var e)){Sync(e);}}
 internal void Describe(Entity owner,System.Text.StringBuilder report){report.AppendLine("DYNAMIC SIGNS "+Status);if(!_owners.TryGetValue(owner,out var parents))return;foreach(var parent in parents)if(_entries.TryGetValue(parent,out var e)){report.AppendLine("SIGN PARENT="+parent+" prefab="+e.Prefab+" approach="+e.Road?.Edge+" speed="+(e.Road?.Speed??"unresolved/unsupported")+" mode="+(e.Road?.Mode??"unconfirmed/unsupported")+" straightMask="+e.Road?.Straight+" leftMask="+e.Road?.Left+" controller="+(e.Road?.Controller??"native/other pattern")+" orderedProgram="+e.Road?.OrderedProgram+" speedKnown="+e.Road?.SpeedKnown+" modeKnown="+e.Road?.ModeKnown+" observed="+e.Road?.Observed);for(int i=0;i<e.Mounts.Length;i++)report.AppendLine("  mount="+i+" kind="+e.Mounts[i].Kind+" position="+e.Mounts[i].Position+" rotation="+e.Mounts[i].Rotation+" visual="+e.Signs[i]+" target="+e.Targets[i]);}}
 internal void SyncVisibility(){using(RuntimeDiagnostics.Measure("TrafficSignSystem.SyncVisibility")){if(_saving||!_ready)return;for(int i=0;i<128&&i<_round.Count;i++){if(_visibilityCursor>=_round.Count)_visibilityCursor=0;Sync(_round[_visibilityCursor++]);}}}
 internal void BeforeSave(){RuntimeDiagnostics.Event("before-save");_saving=true;foreach(var entry in _round){foreach(var e in entry.Signs)if(Live(e)&&!EntityManager.HasComponent<Temp>(e))EntityManager.AddComponent<Temp>(e);foreach(var child in entry.HiddenChildren)Hidden(child,false);}}
 internal void AfterSave(){foreach(var entry in _round){foreach(var e in entry.Signs)if(Live(e)&&EntityManager.HasComponent<Temp>(e)){EntityManager.RemoveComponent<Temp>(e);Mark(e);}foreach(var child in entry.HiddenChildren)Hidden(child,true);}_saving=false;RuntimeDiagnostics.Event("after-save");}
}
public partial class SignVisibilitySystem:GameSystemBase {protected override void OnUpdate(){World.GetExistingSystemManaged<TrafficSignSystem>()?.SyncVisibility();}}
}




