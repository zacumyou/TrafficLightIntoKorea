using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Game;using Game.Common;using Game.Objects;using Game.Prefabs;using Game.SceneFlow;using Game.Tools;
using Unity.Entities;using Unity.Collections;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
[Serializable] public sealed class SignalChoice { public string key;public string nearRoadName="",farRoadName="";public string nearAsset="",farAsset="";public int farMode=-1;public bool nearHidden;public float farOffset,farRotation,farLateral;public string[] nearSigns,farSigns;public string[] nearProps,farProps; }
[Serializable] public sealed class SignalSidecar { public int schema=1;public string saveName="";public string[] nodes=new string[0];public SignalChoice[] signals=new SignalChoice[0]; }
public partial class IndividualSignalData:GameSystemBase {
 private readonly Dictionary<string,SignalChoice> _choices=new Dictionary<string,SignalChoice>();
 private readonly List<Entity> _savedNodes=new List<Entity>();private string _loadId,_snapshot,_saveName;private bool _ready,_saving;
 private readonly HashSet<string> _unresolved=new HashSet<string>(StringComparer.Ordinal);
 private readonly HashSet<string> _ambiguous=new HashSet<string>();
 private Entity[] _restoreNodes;private int _restoreCursor,_restorePass,_restored;private float _nextRestore;
 internal bool HasForcedFar{get;private set;}
 internal static string SaveStatus="";internal static string SaveProblem="";
 private static string Hash(string text){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-","");}
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
 internal SignalChoice Get(Entity owner,Entity source){var key=Key(owner,source);if(key!=null&&_choices.TryGetValue(key,out var c))return c;var old=Key(owner,source,true);return old!=null&&_choices.TryGetValue(old,out c)?c:null;}
 internal bool Set(Entity owner,Entity source,string action,string asset,bool far){if(!_ready)return false;var key=Key(owner,source);if(key==null)return false;if(!_choices.TryGetValue(key,out var c)){var old=Get(owner,source);c=new SignalChoice{key=key,nearRoadName=old?.nearRoadName??"",farRoadName=old?.farRoadName??"",nearAsset=old?.nearAsset??"",farAsset=old?.farAsset??"",farMode=old?.farMode??-1,nearHidden=old?.nearHidden??false,nearSigns=old?.nearSigns,farSigns=old?.farSigns,nearProps=old?.nearProps,farOffset=old?.farOffset??0,farRotation=old?.farRotation??0,farLateral=old?.farLateral??0,farProps=old?.farProps};}
  if(action=="hideNear"&&!far){if(!(World.GetExistingSystemManaged<FarSignalSystem>()?.DisplayReady(source)??false))return false;c.nearHidden=true;c.farMode=1;}else if(action=="restoreNear"&&far)c.nearHidden=false;else
  if((action=="offset"||action=="rotation"||action=="lateral")&&far){if(!float.TryParse(asset,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out var offset)||float.IsNaN(offset)||float.IsInfinity(offset))return false;if(action=="rotation")c.farRotation=FarOffsetRules.Rotation(offset);else if(action=="lateral")c.farLateral=FarOffsetRules.Clamp(offset);else c.farOffset=FarOffsetRules.Clamp(offset);_choices[key]=c;World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshOwner(owner);return true;}
  else if(action=="asset"){if(!Allowed().Contains(asset)||!Catalog.Entries.TryGetValue(asset,out var candidate)||!EntityManager.HasComponent<TrafficLightData>(candidate))return false;if(EntityManager.GetComponentData<TrafficLight>(source).m_GroupMask0!=0&&((int)EntityManager.GetComponentData<TrafficLightData>(candidate).m_Type&3)==0)return false;if(far)c.farAsset=asset;else c.nearAsset=asset;}
  else if(action=="reset"){if(far)c.farAsset="";else c.nearAsset="";}
  else if(action=="add"){if(EntityManager.GetComponentData<TrafficLight>(source).m_GroupMask0==0)return false;c.farMode=1;}else if(action=="delete"&&far){c.farMode=0;c.nearHidden=false;}else if(action=="auto")c.farMode=-1;else return false;
  _choices[key]=c;HasForcedFar=_choices.Values.Any(x=>x.farMode==1);World.GetExistingSystemManaged<ApproachSignalSystem>()?.RefreshOwner(owner);World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshOwner(owner);return true;
 }
 internal string SignSelection(Entity owner,Entity source,bool far,int kind){var c=Get(owner,source);var list=far?c?.farSigns:c?.nearSigns;return list!=null&&kind>0&&kind<=list.Length?list[kind-1]??"auto":"auto";}
 internal bool SetSign(Entity owner,Entity source,bool far,int kind,string name){if(!_ready||kind<1||kind>3||name!="auto"&&name!="none"&&SignRules.Kind(name)!=kind)return false;var key=Key(owner,source);if(key==null)return false;var c=Get(owner,source);if(c==null)c=new SignalChoice{key=key};else if(c.key!=key)c=new SignalChoice{key=key,nearRoadName=c.nearRoadName,farRoadName=c.farRoadName,nearAsset=c.nearAsset,farAsset=c.farAsset,farMode=c.farMode,nearHidden=c.nearHidden,nearSigns=c.nearSigns,farSigns=c.farSigns,nearProps=c.nearProps,farOffset=c.farOffset,farRotation=c.farRotation,farLateral=c.farLateral,farProps=c.farProps};var old=far?c.farSigns:c.nearSigns;var list=new[]{"auto","auto","auto"};if(old!=null)Array.Copy(old,list,Math.Min(3,old.Length));list[kind-1]=name;if(far)c.farSigns=list;else c.nearSigns=list;_choices[key]=c;World.GetExistingSystemManaged<TrafficSignSystem>()?.RefreshOwner(owner);RuntimeDiagnostics.Event("Individual sign key="+key+" far="+far+" kind="+kind+" value="+name);return true;}
 internal string[] Allowed(){var set=new HashSet<string>(StringComparer.Ordinal);var settings=Mod.Settings;if(settings==null)return new string[0];foreach(var source in SourceNames.All)set.Add(settings.Selection(source));foreach(var extra in DirectionalTargetNames.Parse(settings.IndividualAssets))set.Add(extra);set.Add(settings.StraightLeftVehicleAsset);set.Add(settings.StraightLeftCrosswalkAsset);set.RemoveWhere(x=>string.IsNullOrEmpty(x)||!Catalog.Entries.ContainsKey(x)||x.StartsWith("NA_")||x.StartsWith("EU_"));var result=set.ToArray();Array.Sort(result,StringComparer.Ordinal);return result;}
 internal Entity Target(Entity owner,Entity source,Entity fallback,bool far){var choice=Get(owner,source);string name=far?choice?.farAsset:choice?.nearAsset;if(string.IsNullOrEmpty(name)||!Allowed().Contains(name)||!Catalog.Entries.TryGetValue(name,out var asset)||!EntityManager.HasComponent<TrafficLightData>(asset))return ApplyAccessories(owner,source,fallback,far);
  if((EntityManager.GetComponentData<TrafficLight>(source).m_GroupMask0!=0)&&((int)EntityManager.GetComponentData<TrafficLightData>(asset).m_Type&3)==0)return ApplyAccessories(owner,source,fallback,far);
  var proxy=World.GetExistingSystemManaged<OverrideSystem>().RegularProxy(asset);return ApplyAccessories(owner,source,proxy==Entity.Null?fallback:proxy,far);
 }
 protected override void OnCreate(){base.OnCreate();GameManager.instance.onGameSaveLoad+=Saved;}
 protected override void OnDestroy(){GameManager.instance.onGameSaveLoad-=Saved;base.OnDestroy();}
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose p,GameMode m){_ready=false;_saving=false;_unresolved.Clear();_restoreNodes=null;_restorePass=0;_restored=0;SaveStatus="";SaveProblem="";HasForcedFar=false;_choices.Clear();_snapshot=null;_savedNodes.Clear();_loadId=null;base.OnGamePreload(p,m);}
 protected override void OnGameLoaded(Colossal.Serialization.Entities.Context context){base.OnGameLoaded(context);_loadId=context.instigatorGuid.ToString();RuntimeDiagnostics.Event("Individual sidecar load ID="+_loadId);}
 private static string Folder=>Path.Combine(UnityEngine.Application.persistentDataPath,"ModsData","TrafficLightIntoKorea","IndividualSignals");
 private static string FileFor(string id){if(string.IsNullOrEmpty(id)||id.Any(c=>!Uri.IsHexDigit(c)&&c!='-'))throw new InvalidDataException("Invalid save identifier");return Path.Combine(Folder,id+".json");}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose p,GameMode m){base.OnGameLoadingComplete(p,m);_ready=m==GameMode.Game;if(!_ready||string.IsNullOrEmpty(_loadId))return;
  try{var file=FileFor(_loadId);if(!File.Exists(file)){RuntimeDiagnostics.Event("Individual sidecar missing: "+file);return;}
   var data=Newtonsoft.Json.JsonConvert.DeserializeObject<SignalSidecar>(File.ReadAllText(file));
   if(data==null||data.schema!=1||data.nodes==null)throw new InvalidDataException("Unsupported sidecar");
   // Older Unity JSON files omit empty arrays. An absent signals array is valid.
   foreach(var c in data.signals??new SignalChoice[0])if(c!=null&&!string.IsNullOrEmpty(c.key)&&c.farMode>=-1&&c.farMode<=1)_choices[c.key]=c;
   foreach(var key in data.nodes)if(!string.IsNullOrEmpty(key))_unresolved.Add(key);
   HasForcedFar=_choices.Values.Any(x=>x.farMode==1);_restorePass=0;_restored=0;_nextRestore=0;
   RuntimeDiagnostics.Event("Individual sidecar read nodes="+_unresolved.Count+" choices="+_choices.Count);
  }catch(Exception e){SaveStatus="개별 설정 파일을 불러오지 못했습니다: "+e.Message;SaveProblem=SaveStatus;RuntimeDiagnostics.Event(SaveStatus);}
 }
 internal void TraceNode(Entity node,string action){RuntimeDiagnostics.Event("Individual "+action+" node="+node+" key="+NodeKey(node)+" legacy="+LegacyNodeKey(node));}
 internal void ForgetAllNodes(){_unresolved.Clear();_restoreNodes=null;_restorePass=3;}
 internal void ForgetNode(Entity node){var key=NodeKey(node);if(key!=null)_unresolved.Remove(key);var legacy=LegacyNodeKey(node);if(legacy!=null)_unresolved.Remove(legacy);}
 private void RestoreTick(){if(!_ready||_saving||_unresolved.Count==0||_restorePass>=3)return;
  if(_restoreNodes==null){if(UnityEngine.Time.realtimeSinceStartup<_nextRestore)return;using(var q=EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Game.Net.Node>(),ComponentType.ReadOnly<Game.Net.ConnectedEdge>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>()))using(var nodes=q.ToEntityArray(Allocator.Temp))_restoreNodes=nodes.ToArray();_restoreCursor=0;_ambiguous.Clear();var seen=new HashSet<string>();foreach(var n in _restoreNodes){var k=NodeKey(n);if(k!=null&&!seen.Add(k))_ambiguous.Add(k);}}
  // A finite load-time pass, spread over frames; never scan the city every update indefinitely.
  int budget=128;while(budget-->0&&_restoreCursor<_restoreNodes.Length){var n=_restoreNodes[_restoreCursor++];var key=NodeKey(n);var legacy=LegacyNodeKey(n);if(key!=null&&!_ambiguous.Contains(key)&&(_unresolved.Contains(key)||legacy!=null&&_unresolved.Contains(legacy))){World.GetExistingSystemManaged<KoreanJunctionSystem>()?.Select(n,true);if(EntityManager.HasComponent<KoreanJunction>(n)){_unresolved.Remove(key);if(legacy!=null)_unresolved.Remove(legacy);_restored++;RuntimeDiagnostics.Event("Individual restored node="+n+" key="+key+" legacy="+legacy);}}}
  if(_unresolved.Count==0||_restoreCursor>=_restoreNodes.Length){_restoreNodes=null;_restorePass++;_nextRestore=UnityEngine.Time.realtimeSinceStartup+5;SaveStatus="교차로 복원 "+_restored+"개 · 미확인 "+_unresolved.Count+"개";RuntimeDiagnostics.Event("Individual restore pass="+_restorePass+" restored="+_restored+" unresolved="+_unresolved.Count);}
 }
 internal void BeforeSave(){if(!_ready||_saving)return;_saving=true;_savedNodes.Clear();var keys=new HashSet<string>(_unresolved,StringComparer.Ordinal);using(var q=EntityManager.CreateEntityQuery(ComponentType.ReadOnly<KoreanJunction>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>()))using(var nodes=q.ToEntityArray(Allocator.Temp))foreach(var n in nodes){_savedNodes.Add(n);var key=NodeKey(n);if(key!=null){keys.Add(key);RuntimeDiagnostics.Event("Individual save node="+n+" key="+key+" legacy="+LegacyNodeKey(n));}}
  _snapshot=Newtonsoft.Json.JsonConvert.SerializeObject(new SignalSidecar{saveName=_saveName,nodes=keys.ToArray(),signals=_choices.Values.ToArray()},Newtonsoft.Json.Formatting.Indented);
  // Strip even the legacy serializable junction tag. Native save contains no dependency on this tag.
  foreach(var n in _savedNodes)EntityManager.RemoveComponent<KoreanJunction>(n);
 }
 internal void AfterSave(){_saving=false;foreach(var n in _savedNodes)if(EntityManager.Exists(n)&&!EntityManager.HasComponent<Deleted>(n)&&!EntityManager.HasComponent<KoreanJunction>(n))EntityManager.AddComponent<KoreanJunction>(n);_savedNodes.Clear();}
 private void Saved(string name,string preview,bool start,bool success){if(start){_saveName=name;return;}AfterSave();if(!success||_snapshot==null)return;try{var metadata=GameManager.instance.settings.userState.lastSaveGameMetadata;if(metadata==null)throw new InvalidOperationException("Save metadata missing");var file=FileFor(metadata.id.guid.ToString());SidecarStorage.Write(file,_snapshot);_snapshot=null;SaveProblem="";SaveStatus="개별 설정 저장 완료";RuntimeDiagnostics.Event("Individual signal sidecar saved "+file);}catch(Exception e){SaveStatus="개별 설정 저장 실패: "+e.Message;SaveProblem=SaveStatus;RuntimeDiagnostics.Event(SaveStatus);}}
 protected override void OnUpdate(){RestoreTick();}
}
}
