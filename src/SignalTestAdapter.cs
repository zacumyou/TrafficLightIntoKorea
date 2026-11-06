using System;
using System.Collections.Generic;
using Game.Common;
using Game.Objects;
using Game.Prefabs;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace TrafficLightIntoKorea {
// Optional development adapter. CityBridge gates calls to its owned, unsaved test city.
public partial class IndividualSignalUI {
 private static object TestPosition(Unity.Mathematics.float3 p)=>new{x=p.x,y=p.y,z=p.z};
 public string SignalTest(string json) {
  var a=JObject.Parse(json);var action=(string)a["action"]??"state";
  var em=EntityManager;var ps=World.GetExistingSystemManaged<PrefabSystem>();
  Func<Entity,string> name=e=>em.HasComponent<PrefabRef>(e)&&ps.TryGetPrefab<PrefabBase>(em.GetComponentData<PrefabRef>(e).m_Prefab,out var p)?p.name:"";
  var nodes=new List<object>();var displays=new List<object>();
  if(action=="junction"){
   var n=new Entity{Index=(int)a["index"],Version=(int)a["version"]};
   if(!em.Exists(n)||!SignalNodeTarget.Selectable(em,n))throw new ArgumentException("Invalid junction");
   World.GetExistingSystemManaged<KoreanJunctionSystem>().RequestSelect(n,(bool?)a["enabled"]??true);
  } else if(action=="bulk") {
   if((bool?)a["enabled"]??true)KoreanJunctionSystem.ApplyAllFromSettings();else KoreanJunctionSystem.RestoreAllFromSettings();
  } else if(action=="open") {
   var e=new Entity{Index=(int)a["index"],Version=(int)a["version"]};
   if(!Live(e)||!em.HasComponent<IndividualSignalLink>(e))throw new ArgumentException("Invalid display");
   World.GetExistingSystemManaged<Game.Tools.ToolSystem>().activeTool=World.GetExistingSystemManaged<IndividualSignalTool>();
   Open(e,em.GetComponentData<IndividualSignalLink>(e));
  } else if(action=="command") {
   var command=(string)a["value"]??"";
   if(command.Length>250||!System.Text.RegularExpressions.Regex.IsMatch(command,@"^(close|reset|add|delete|auto|hideNear|restoreNear|manual|asset:[A-Za-z0-9 _-]+|roadname:[A-Za-z0-9 _-]*|(?:offset|rotation|lateral):-?[0-9.]+|sign:[123]:[A-Za-z0-9 _-]+|prop:[A-Za-z0-9 _-]+:[01])$"))throw new ArgumentException("Unsupported panel command");
   if(!Opened)throw new InvalidOperationException("Panel closed");Choose(command);
  } else if(action!="state")throw new ArgumentException("Unknown signal test action");
  using(var q=em.CreateEntityQuery(ComponentType.ReadOnly<Game.Net.Node>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Game.Tools.Temp>()))using(var es=q.ToEntityArray(Allocator.Temp))foreach(var n in es){
   if(nodes.Count>=100)break;if(!SignalNodeTarget.Selectable(em,n))continue;
   nodes.Add(new{index=n.Index,version=n.Version,position=TestPosition(em.GetComponentData<Game.Net.Node>(n).m_Position),signals=em.HasComponent<Game.Net.TrafficLights>(n),selected=KoreanJunctionSystem.Selected(em,n)});
  }
  using(var q=em.CreateEntityQuery(ComponentType.ReadOnly<IndividualSignalLink>(),ComponentType.ReadOnly<Transform>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<KoreanRetired>()))using(var es=q.ToEntityArray(Allocator.Temp))foreach(var e in es){
   if(displays.Count>=200)break;var l=em.GetComponentData<IndividualSignalLink>(e);
   displays.Add(new{index=e.Index,version=e.Version,owner=l.Owner.Index,source=l.Source.Index,sourcePrefab=name(l.Source),sourcePosition=em.HasComponent<Transform>(l.Source)?TestPosition(em.GetComponentData<Transform>(l.Source).m_Position):null,sourceRotation=em.HasComponent<Transform>(l.Source)?TestPosition(Unity.Mathematics.math.rotate(em.GetComponentData<Transform>(l.Source).m_Rotation,new Unity.Mathematics.float3(0,0,1))):null,key=World.GetExistingSystemManaged<IndividualSignalData>().Key(l.Owner,l.Source),choice=World.GetExistingSystemManaged<IndividualSignalData>().Get(l.Owner,l.Source),far=l.Far,prefab=name(e),position=TestPosition(em.GetComponentData<Transform>(e).m_Position),mainReady=SignalRenderReadiness.MainReady(em,e),allocated=SignalRenderReadiness.Allocated(em,e),hidden=em.HasComponent<Hidden>(e),sourceHidden=em.HasComponent<Hidden>(l.Source)});
  }
  object panel=null;
  if(Opened&&Live(_entity))panel=new{entity=new{index=_entity.Index,version=_entity.Version},asset=CurrentAsset(),far=_link.Far,choice=World.GetExistingSystemManaged<IndividualSignalData>().Get(_link.Owner,_link.Source),props=PropItems(),roadNames=RoadNames(),mounts=World.GetExistingSystemManaged<TrafficSignSystem>().MountKinds(_entity),farReady=World.GetExistingSystemManaged<FarSignalSystem>().DisplayReady(_link.Source),pending=_commands.Count};
  return JsonConvert.SerializeObject(new{version=typeof(Mod).Assembly.GetName().Version.ToString(),enabled=Mod.Settings.Enabled,nodes,displays,assets=World.GetExistingSystemManaged<IndividualSignalData>().Allowed(),signs=World.GetExistingSystemManaged<TrafficSignSystem>().AvailableSigns(),opened=Opened,panel,bulk=KoreanJunctionSystem.RestoreStatus});
 }
}
}
