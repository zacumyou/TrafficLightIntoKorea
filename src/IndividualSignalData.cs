using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Game;using Game.Common;using Game.Objects;using Game.Prefabs;using Game.SceneFlow;using Game.Tools;
using Unity.Entities;using Unity.Collections;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
public partial class IndividualSignalData:GameSystemBase {
 private readonly Dictionary<string,SignalChoice> _choices=new Dictionary<string,SignalChoice>();
 private readonly List<Entity> _savedNodes=new List<Entity>();private string _loadId,_snapshot,_saveName;private bool _ready,_saving,_readFailed;
 private readonly RestoreNodeState _restoreState=new RestoreNodeState();private HashSet<string> _unresolved=>_restoreState.Pending;
 
 private IncrementalNodeRestore<Entity> _restoreScan;private int _restorePass,_restored,_restoreStartCount,_restoreNodeCount,_stalledPasses;private float _nextRestore;
 private readonly HashSet<string> _retainedNodes=new HashSet<string>(StringComparer.Ordinal);
 internal bool HasRetainedSettings(Entity node){var key=NodeKey(node);return key!=null&&_retainedNodes.Contains(key);}
 internal bool HasForcedFar{get;private set;}
 internal static string SaveStatus="";internal static string SaveProblem="";
 private static string Hash(string text){RuntimeDiagnostics.Count("settings.hash");using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-","");}
 private static string Point(float3 p)=>string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0},{1},{2}",math.round(p.x*10),math.round(p.y*10),math.round(p.z*10));
 internal string NodeKey(Entity node){if(!EntityManager.Exists(node)||!EntityManager.HasComponent<Game.Net.Node>(node))return null;return "N2:"+Point(EntityManager.GetComponentData<Game.Net.Node>(node).m_Position);}
 internal string LegacyNodeKey(Entity node){
  if(!EntityManager.Exists(node)||!EntityManager.HasComponent<Game.Net.Node>(node)||!EntityManager.HasBuffer<Game.Net.ConnectedEdge>(node))return null;
  var links=new List<string>();foreach(var item in EntityManager.GetBuffer<Game.Net.ConnectedEdge>(node,true)){
   var e=item.m_Edge;if(!EntityManager.Exists(e)||EntityManager.HasComponent<Deleted>(e)||!EntityManager.HasComponent<Game.Net.Edge>(e))continue;
   var edge=EntityManager.GetComponentData<Game.Net.Edge>(e);var other=edge.m_Start==node?edge.m_End:edge.m_Start;
   if(!EntityManager.HasComponent<Game.Net.Node>(other))return null;string name="";
   if(EntityManager.HasComponent<PrefabRef>(e)&&World.GetExistingSystemManaged<PrefabSystem>().TryGetPrefab<PrefabBase>(EntityManager.GetComponentData<PrefabRef>(e).m_Prefab,out var p))name=p.name;
   links.Add(Point(EntityManager.GetComponentData<Game.Net.Node>(other).m_Position)+":"+name);
  }links.Sort(StringComparer.Ordinal);return Hash(Point(EntityManager.GetComponentData<Game.Net.Node>(node).m_Position)+"|"+string.Join("|",links));
 }
 internal string Key(Entity owner,Entity source,bool legacy=false){var node=legacy?LegacyNodeKey(owner):NodeKey(owner);if(node==null||!EntityManager.Exists(source)||EntityManager.HasComponent<Deleted>(source)||EntityManager.HasComponent<Temp>(source)||!EntityManager.HasComponent<Transform>(source)||!EntityManager.HasComponent<PrefabRef>(source))return null;
  var t=EntityManager.GetComponentData<Transform>(source);var p=EntityManager.GetComponentData<PrefabRef>(source).m_Prefab;if(!World.GetExistingSystemManaged<PrefabSystem>().TryGetPrefab<PrefabBase>(p,out var asset))return null;
  // Refuse indistinguishable overlapping originals rather than applying to both.
  if(EntityManager.HasBuffer<Game.Objects.SubObject>(owner))foreach(var item in EntityManager.GetBuffer<Game.Objects.SubObject>(owner,true)){var e=item.m_SubObject;if(e==source||!EntityManager.Exists(e)||EntityManager.HasComponent<KoreanSignalClone>(e)||!EntityManager.HasComponent<TrafficLight>(e)||!EntityManager.HasComponent<PrefabRef>(e)||!EntityManager.HasComponent<Transform>(e))continue;var o=EntityManager.GetComponentData<Transform>(e);if(EntityManager.GetComponentData<PrefabRef>(e).m_Prefab==p&&math.distancesq(o.m_Position,t.m_Position)<.0025f&&math.abs(math.dot(o.m_Rotation.value,t.m_Rotation.value))>.9999f)return null;}
  var forward=math.rotate(t.m_Rotation,new float3(0,0,1));return Hash(node+"|"+asset.name+"|"+Point(t.m_Position)+"|"+Point(forward));
 }
 internal SignalChoice Get(Entity owner,Entity source){if(_choices.Count==0)return null;RuntimeDiagnostics.Count("settings.lookup");var key=Key(owner,source);if(key!=null&&_choices.TryGetValue(key,out var c))return c;var old=Key(owner,source,true);return old!=null&&_choices.TryGetValue(old,out c)?c:null;}
 internal bool Set(Entity owner,Entity source,string action,string asset,bool far){if(!_ready)return false;var key=Key(owner,source);if(key==null)return false;if(!_choices.TryGetValue(key,out var c)){var old=Get(owner,source);c=new SignalChoice{key=key,nearRoadName=old?.nearRoadName??"",farRoadName=old?.farRoadName??"",nearAsset=old?.nearAsset??"",farAsset=old?.farAsset??"",farMode=old?.farMode??-1,nearHidden=old?.nearHidden??false,nearSigns=old?.nearSigns,farSigns=old?.farSigns,nearProps=old?.nearProps,nearOffset=old?.nearOffset??0,nearRotation=old?.nearRotation??0,nearLateral=old?.nearLateral??0,farOffset=old?.farOffset??0,farRotation=old?.farRotation??0,farLateral=old?.farLateral??0,manualPosition=old?.manualPosition,manualRotation=old?.manualRotation,farProps=old?.farProps};}
  if(action=="hideNear"&&!far){if(!(World.GetExistingSystemManaged<FarSignalSystem>()?.DisplayReady(source)??false))return false;c.nearHidden=true;c.farMode=1;}else if(action=="restoreNear"&&far)c.nearHidden=false;else
  if(action=="offset"||action=="rotation"||action=="lateral"){if(!float.TryParse(asset,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out var offset)||float.IsNaN(offset)||float.IsInfinity(offset))return false;if(far){if(action=="rotation")c.farRotation=FarOffsetRules.FarRotation(offset);else if(action=="lateral")c.farLateral=FarOffsetRules.Clamp(offset);else c.farOffset=FarOffsetRules.Clamp(offset);}else{if(action=="rotation")c.nearRotation=FarOffsetRules.Rotation(offset);else if(action=="lateral")c.nearLateral=FarOffsetRules.Clamp(offset);else c.nearOffset=FarOffsetRules.Clamp(offset);}_choices[key]=c;if(far)World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshOwner(owner);else{var approach=World.GetExistingSystemManaged<ApproachSignalSystem>();if(approach!=null&&!approach.RefreshPlacement(owner,source))approach.RefreshOwner(owner);}return true;}
  else if(action=="asset"){if(!Allowed().Contains(asset)||!Catalog.Entries.TryGetValue(asset,out var candidate)||!EntityManager.HasComponent<TrafficLightData>(candidate))return false;if(far)c.farAsset=asset;else c.nearAsset=asset;}
  else if(action=="reset"){if(far)c.farAsset="";else c.nearAsset="";}
  else if(action=="add"){if(EntityManager.GetComponentData<TrafficLight>(source).m_GroupMask0==0)return false;c.manualPosition=null;c.manualRotation=null;c.farMode=1;}else if(action=="delete"&&far){c.farMode=0;c.nearHidden=false;}else if(action=="auto"){c.farMode=-1;c.manualPosition=null;c.manualRotation=null;}else return false;
  _choices[key]=c;HasForcedFar=_choices.Values.Any(x=>x.farMode==1);World.GetExistingSystemManaged<ApproachSignalSystem>()?.RefreshOwner(owner);World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshOwner(owner);return true;
 }
 internal string SignSelection(Entity owner,Entity source,bool far,int kind){var c=Get(owner,source);var list=far?c?.farSigns:c?.nearSigns;return list!=null&&kind>0&&kind<=list.Length?list[kind-1]??"auto":"auto";}
 internal bool SetSign(Entity owner,Entity source,bool far,int kind,string name){if(!_ready||kind<1||kind>3||name!="auto"&&name!="none"&&SignRules.Kind(name)!=kind)return false;var key=Key(owner,source);if(key==null)return false;var c=Get(owner,source);if(c==null)c=new SignalChoice{key=key};else if(c.key!=key)c=new SignalChoice{key=key,nearRoadName=c.nearRoadName,farRoadName=c.farRoadName,nearAsset=c.nearAsset,farAsset=c.farAsset,farMode=c.farMode,nearHidden=c.nearHidden,nearSigns=c.nearSigns,farSigns=c.farSigns,nearProps=c.nearProps,nearOffset=c.nearOffset,nearRotation=c.nearRotation,nearLateral=c.nearLateral,farOffset=c.farOffset,farRotation=c.farRotation,farLateral=c.farLateral,manualPosition=c.manualPosition,manualRotation=c.manualRotation,farProps=c.farProps};var old=far?c.farSigns:c.nearSigns;var list=new[]{"auto","auto","auto"};if(old!=null)Array.Copy(old,list,Math.Min(3,old.Length));list[kind-1]=name;if(far)c.farSigns=list;else c.nearSigns=list;_choices[key]=c;World.GetExistingSystemManaged<TrafficSignSystem>()?.RefreshOwner(owner);RuntimeDiagnostics.Event("Individual sign key="+key+" far="+far+" kind="+kind+" value="+name);return true;}
 internal string[] Allowed(){var set=new HashSet<string>(StringComparer.Ordinal);var settings=Mod.Settings;if(settings==null)return new string[0];foreach(var source in SourceNames.All)set.Add(settings.Selection(source));foreach(var extra in DirectionalTargetNames.Parse(settings.IndividualAssets))set.Add(extra);set.Add(settings.StraightLeftVehicleAsset);set.Add(settings.StraightLeftCrosswalkAsset);set.RemoveWhere(x=>string.IsNullOrEmpty(x)||!Catalog.Entries.ContainsKey(x)||x.StartsWith("NA_")||x.StartsWith("EU_"));var result=set.ToArray();Array.Sort(result,StringComparer.Ordinal);return result;}
 internal Entity Target(Entity owner,Entity source,Entity fallback,bool far){var choice=Get(owner,source);string name=far?choice?.farAsset:choice?.nearAsset;if(string.IsNullOrEmpty(name)||!Allowed().Contains(name)||!Catalog.Entries.TryGetValue(name,out var asset)||!EntityManager.HasComponent<TrafficLightData>(asset))return ApplyAccessories(owner,source,fallback,far);
  // Visual selection does not change the original traffic simulation or its masks.
  var proxy=World.GetExistingSystemManaged<OverrideSystem>().RegularProxy(asset);return ApplyAccessories(owner,source,proxy==Entity.Null?fallback:proxy,far);
 }
 protected override void OnCreate(){base.OnCreate();GameManager.instance.onGameSaveLoad+=Saved;}
 protected override void OnDestroy(){GameManager.instance.onGameSaveLoad-=Saved;base.OnDestroy();}
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose p,GameMode m){_ready=false;_saving=false;_readFailed=false;_retainedNodes.Clear();_restoreState.Clear();_restoreScan=null;_restorePass=0;_restored=0;_stalledPasses=0;SaveStatus="";SaveProblem="";HasForcedFar=false;_choices.Clear();_snapshot=null;_savedNodes.Clear();_loadId=null;base.OnGamePreload(p,m);}
 protected override void OnGameLoaded(Colossal.Serialization.Entities.Context context){base.OnGameLoaded(context);_loadId=context.instigatorGuid.ToString();RuntimeDiagnostics.Event("Individual sidecar load ID="+_loadId);}
 private static string Folder=>Path.Combine(UnityEngine.Application.persistentDataPath,"ModsData","TrafficLightIntoKorea","IndividualSignals");
 private static string FileFor(string id){if(string.IsNullOrEmpty(id)||id.Any(c=>!Uri.IsHexDigit(c)&&c!='-'))throw new InvalidDataException("Invalid save identifier");return Path.Combine(Folder,id+".json");}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose p,GameMode m){base.OnGameLoadingComplete(p,m);_ready=m==GameMode.Game;if(!_ready||string.IsNullOrEmpty(_loadId))return;
  try{var file=FileFor(_loadId);var data=SidecarStorage.Read(file,out var recovered);if(data==null){SaveStatus="이 세이브의 개별 설정 파일이 없습니다. / No settings sidecar for this save.";RuntimeDiagnostics.Event("Individual sidecar missing: "+file+"; local/cloud transfer requires matching JSON");return;}if(recovered){SaveStatus="설정 백업에서 복원했습니다. / Recovered settings from backup.";RuntimeDiagnostics.Event("Individual sidecar recovered from backup: "+file);}
   // Older Unity JSON files omit empty arrays. An absent signals array is valid.
   foreach(var c in data.signals??new SignalChoice[0])if(c!=null&&!string.IsNullOrEmpty(c.key)&&c.farMode>=-1&&c.farMode<=1)_choices[c.key]=c;
   foreach(var key in data.retainedNodes??new string[0])if(!string.IsNullOrEmpty(key))_retainedNodes.Add(key);
   _restoreState.Load(data.nodes,data.dormantNodes);
   HasForcedFar=_choices.Values.Any(x=>x.farMode==1);_restorePass=0;_restored=0;_stalledPasses=0;_nextRestore=0;
   RuntimeDiagnostics.Event("Individual sidecar read nodes="+_unresolved.Count+" dormant="+_restoreState.Dormant.Count+" choices="+_choices.Count);
  }catch(Exception e){_readFailed=true;SaveStatus="개별 설정 파일을 불러오지 못했습니다: "+e.Message;SaveProblem=SaveStatus;RuntimeDiagnostics.Event(SaveStatus);}
 }
 internal void TraceNode(Entity node,string action){RuntimeDiagnostics.Event("Individual "+action+" node="+node+" key="+NodeKey(node));}
 internal void ForgetAllNodes(){_retainedNodes.UnionWith(_unresolved);_unresolved.Clear();_restoreScan=null;_restorePass=RestorePassLimit;}
 internal void ForgetNode(Entity node){var key=NodeKey(node);if(key!=null){if(EntityManager.HasComponent<KoreanJunction>(node)||_unresolved.Contains(key))_retainedNodes.Add(key);_unresolved.Remove(key);}var legacy=LegacyNodeKey(node);if(legacy!=null)_unresolved.Remove(legacy);}
 private const int RestorePassLimit=25;
 private string RestoreKey(Entity node){if(!EntityManager.Exists(node)||EntityManager.HasComponent<Deleted>(node)||EntityManager.HasComponent<Temp>(node))return null;RuntimeDiagnostics.Count("restore.nodeKey");return NodeKey(node);}
 private bool RestoreNode(Entity node,string key,string legacy){
  using(RuntimeDiagnostics.Measure("Individual.RestoreSelect"))World.GetExistingSystemManaged<KoreanJunctionSystem>()?.Select(node,true);
  if(!EntityManager.HasComponent<KoreanJunction>(node))return false;
  _restored++;RuntimeDiagnostics.Event("Individual restored node="+node+" key="+key+(legacy==null?"":" legacy="+legacy));return true;
 }
 private void RestoreTick(){if(!_ready||_saving||_unresolved.Count==0||_restorePass>=RestorePassLimit)return;
  if(_restoreScan==null){
   if(UnityEngine.Time.realtimeSinceStartup<_nextRestore)return;
   Entity[] snapshot;
   using(RuntimeDiagnostics.Measure("Individual.RestoreSnapshot"))
   using(var q=EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Game.Net.Node>(),ComponentType.ReadOnly<Game.Net.ConnectedEdge>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>()))
   using(var nodes=q.ToEntityArray(Allocator.Temp))snapshot=nodes.ToArray();
   _restoreStartCount=_unresolved.Count;_restoreNodeCount=snapshot.Length;
   _restoreScan=new IncrementalNodeRestore<Entity>(snapshot,_unresolved,RestoreKey,LegacyNodeKey,RestoreNode);
  }
  _restoreScan.Step(64,.5);
  if(_restoreScan.Done){_restoreState.CompletePass(_restoreNodeCount);_restoreScan=null;_restorePass++;_stalledPasses=_unresolved.Count<_restoreStartCount?0:_stalledPasses+1;_nextRestore=UnityEngine.Time.realtimeSinceStartup+IncrementalNodeRestore<Entity>.RetryDelay(_stalledPasses);SaveStatus="교차로 복원 "+_restored+"개 · 미확인 "+_unresolved.Count+"개 · 자동 검색 제외 "+_restoreState.Dormant.Count+"개";RuntimeDiagnostics.Event("Individual restore pass="+_restorePass+" restored="+_restored+" unresolved="+_unresolved.Count+" dormant="+_restoreState.Dormant.Count);}
 }
 internal void BeforeSave(){if(!_ready||_saving)return;_saving=true;_savedNodes.Clear();var keys=new HashSet<string>(_unresolved,StringComparer.Ordinal);using(var q=EntityManager.CreateEntityQuery(ComponentType.ReadOnly<KoreanJunction>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>()))using(var nodes=q.ToEntityArray(Allocator.Temp))foreach(var n in nodes){_savedNodes.Add(n);var key=NodeKey(n);if(key!=null){keys.Add(key);RuntimeDiagnostics.Event("Individual save node="+n+" key="+key);}}
  _snapshot=_readFailed?null:Newtonsoft.Json.JsonConvert.SerializeObject(new SignalSidecar{saveName=_saveName,retainedNodes=_retainedNodes.ToArray(),nodes=_restoreState.ActiveForSave(keys),dormantNodes=_restoreState.DormantForSave(keys),signals=_choices.Values.ToArray()},Newtonsoft.Json.Formatting.Indented);
  // Strip even the legacy serializable junction tag. Native save contains no dependency on this tag.
  foreach(var n in _savedNodes)EntityManager.RemoveComponent<KoreanJunction>(n);
 }
 internal void AfterSave(){World.GetExistingSystemManaged<PersistentSignalVisibilitySystem>()?.AfterSave();_saving=false;foreach(var n in _savedNodes)if(EntityManager.Exists(n)&&!EntityManager.HasComponent<Deleted>(n)&&!EntityManager.HasComponent<KoreanJunction>(n))EntityManager.AddComponent<KoreanJunction>(n);_savedNodes.Clear();}
 private void Saved(string name,string preview,bool start,bool success){
  if(start){_snapshot=null;_saveName=name;return;}AfterSave();
  if(!success){_snapshot=null;RuntimeDiagnostics.Event("Native save failed; sidecar not committed");return;}
  if(_readFailed){SaveProblem="읽지 못한 기존 설정을 보호하기 위해 개별 설정 저장을 중단했습니다. / Settings save blocked to protect unread data.";RuntimeDiagnostics.Event(SaveProblem);return;}
  if(_snapshot==null){SaveProblem="저장할 개별 설정 스냅샷이 없습니다. / Settings snapshot missing.";RuntimeDiagnostics.Event(SaveProblem);return;}
  try{var metadata=GameManager.instance.settings.userState.lastSaveGameMetadata;if(metadata==null)throw new InvalidOperationException("Save metadata missing");
   var file=FileFor(metadata.id.guid.ToString());SidecarStorage.Write(file,_snapshot);
   // Normal loading uses metadata ID; direct-data loading uses SaveGameData ID.
   var data=metadata.target?.saveGameData;if(data!=null){var dataFile=FileFor(data.id.guid.ToString());if(!string.Equals(file,dataFile,StringComparison.OrdinalIgnoreCase))SidecarStorage.Write(dataFile,_snapshot);}
   _snapshot=null;SaveProblem="";SaveStatus="개별 설정 저장 완료";RuntimeDiagnostics.Event("Individual signal sidecar saved "+file);
  }catch(Exception e){SaveStatus="개별 설정 저장 실패: "+e.Message;SaveProblem=SaveStatus;RuntimeDiagnostics.Event(SaveStatus);}
 }
 protected override void OnUpdate(){using(RuntimeDiagnostics.Measure("Individual.RestoreTick"))RestoreTick();}
}
}
