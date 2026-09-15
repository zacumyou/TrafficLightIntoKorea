using System;using System.IO;using System.Text;using Game.Common;using Game.Prefabs;using Game.Objects;using Game.Tools;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
public partial class LeftSignalSystem {
 internal string WriteDebugReport(Entity selected,RaycastHit hit){
 var s=new StringBuilder();s.AppendLine("TRAFFIC LIGHT INTO KOREA 2.3.28 / click snapshot");s.AppendLine("UTC="+DateTime.UtcNow.ToString("O"));
 s.AppendLine("RightHandTraffic="+!World.GetOrCreateSystemManaged<Game.City.CityConfigurationSystem>().leftHandTraffic);
 s.AppendLine("HideOption="+Mod.Settings.HideLeftReplacedLights+" Ready="+_ready+" Active="+_active+" Queued="+(_queue.Count-_head));
 s.AppendLine("RayHit="+hit.m_HitEntity+" HitPosition="+hit.m_HitPosition);Describe(s,selected,"SELECTED");
 var owner=LightOwner(selected);s.AppendLine("ResolvedOwner="+owner);World.GetExistingSystemManaged<TrafficSignSystem>()?.Describe(owner,s);
 var chain=selected;for(int i=0;i<8&&EntityManager.HasComponent<Owner>(chain);i++){chain=EntityManager.GetComponentData<Owner>(chain).m_Owner;s.AppendLine("OwnerChain="+chain+" Sublanes="+EntityManager.HasBuffer<Game.Net.SubLane>(chain)+" Subobjects="+EntityManager.HasBuffer<Game.Objects.SubObject>(chain));}
 if(owner!=Entity.Null){
 var t=EntityManager.GetComponentData<Transform>(selected);var mask=EntityManager.GetComponentData<Game.Objects.TrafficLight>(selected).m_GroupMask0;
 bool found=TryApproach(owner,t.m_Position,mask,out var direction);s.AppendLine("SelectedApproach="+found+" Direction="+direction+" HasRightReplacement="+HasRightReplacement(selected));
 var right=new float2(direction.y,-direction.x);
 foreach(var child in EntityManager.GetBuffer<Game.Objects.SubObject>(owner,true)){
 var e=child.m_SubObject;if(!EntityManager.HasComponent<Game.Objects.TrafficLight>(e)||!EntityManager.HasComponent<Transform>(e))continue;
 Describe(s,e,"CANDIDATE");var other=EntityManager.GetComponentData<Transform>(e);int om=EntityManager.GetComponentData<Game.Objects.TrafficLight>(e).m_GroupMask0;var delta=other.m_Position-t.m_Position;bool of=TryApproach(owner,other.m_Position,om,out var od);
 float lateral=math.dot(delta.xz,right),along=math.dot(delta.xz,direction),agreement=math.dot(direction,od);
 s.AppendLine("Approach="+of+" Direction="+od+" Lateral="+lateral+" Along="+along+" Height="+delta.y+" Agreement="+agreement+" CommonMask="+(mask&om)+" GeometryAndGroupPass="+(found&&of&&LightPairRules.RightPartner(lateral,along,delta.y,agreement,mask,om)));
 }
 foreach(var item in EntityManager.GetBuffer<Game.Net.SubLane>(owner,true)){
 var e=item.m_SubLane;if(!EntityManager.HasComponent<Game.Net.CarLane>(e))continue;s.Append("LANE "+e);
 if(EntityManager.HasComponent<Game.Net.LaneSignal>(e))s.Append(" GroupMask="+EntityManager.GetComponentData<Game.Net.LaneSignal>(e).m_GroupMask+" LiveSignal="+EntityManager.GetComponentData<Game.Net.LaneSignal>(e).m_Signal);
 if(EntityManager.HasComponent<Game.Net.Curve>(e)){var b=EntityManager.GetComponentData<Game.Net.Curve>(e).m_Bezier;s.Append(" A="+b.a+" B="+b.b+" C="+b.c+" D="+b.d);}s.AppendLine();
 }
 }
 // A click-only nearby scan also exposes candidates belonging to another owner.
 var selectedPosition=EntityManager.GetComponentData<Transform>(selected).m_Position;int nearby=0;
 using(var all=_signals.ToEntityArray(Unity.Collections.Allocator.Temp))foreach(var e in all){if(e==selected||math.distancesq(EntityManager.GetComponentData<Transform>(e).m_Position,selectedPosition)>3600f)continue;if(nearby++>=128){s.AppendLine("NEARBY truncated at 128");break;}Describe(s,e,"NEARBY");s.AppendLine("ResolvedOwner="+LightOwner(e));}
 var folder=Path.Combine(UnityEngine.Application.persistentDataPath,"ModsData","TrafficLightIntoKorea","Debug");Directory.CreateDirectory(folder);
 var path=Path.Combine(folder,"signal-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N")+".txt");File.WriteAllText(path,s.ToString(),Encoding.UTF8);return path;
 }
 private void Describe(StringBuilder s,Entity e,string label){World.GetExistingSystemManaged<DirectionalLightSystem>()?.Describe(e,s);s.AppendLine(label+" "+e+" FarSignal="+EntityManager.HasComponent<FarSignal>(e)+" Hidden="+EntityManager.HasComponent<Hidden>(e)+" OverlapOverridden="+EntityManager.HasComponent<Overridden>(e)+" ModOwnedHidden="+_owned.Contains(e)+" Deleted="+EntityManager.HasComponent<Deleted>(e)+" Temp="+EntityManager.HasComponent<Temp>(e));
 if(EntityManager.HasComponent<Game.Rendering.CullingInfo>(e)){var c=EntityManager.GetComponentData<Game.Rendering.CullingInfo>(e);s.AppendLine("CullingIndex="+c.m_CullingIndex+" PassedCulling="+c.m_PassedCulling+" Bounds="+c.m_Bounds);}
 if(EntityManager.HasBuffer<Game.Rendering.MeshBatch>(e)){s.AppendLine("MainBatchReady="+SignalRenderReadiness.MainReady(EntityManager,e));foreach(var b in EntityManager.GetBuffer<Game.Rendering.MeshBatch>(e,true))s.AppendLine("MeshBatch group="+b.m_MeshGroup+" mesh="+b.m_MeshIndex+" allocation="+b.m_GroupIndex+":"+b.m_InstanceIndex);}
 if(EntityManager.HasComponent<Transform>(e)){var t=EntityManager.GetComponentData<Transform>(e);s.AppendLine("Position="+t.m_Position+" Rotation="+t.m_Rotation.value);}
 if(EntityManager.HasComponent<Game.Objects.TrafficLight>(e)){var l=EntityManager.GetComponentData<Game.Objects.TrafficLight>(e);s.AppendLine("VehicleMask="+l.m_GroupMask0+" PedestrianMask="+l.m_GroupMask1+" State="+l.m_State);}
 if(EntityManager.HasComponent<PrefabRef>(e)){var p=EntityManager.GetComponentData<PrefabRef>(e).m_Prefab;var ps=World.GetOrCreateSystemManaged<PrefabSystem>();string name=ps.TryGetPrefab<PrefabBase>(p,out var prefab)?prefab.name:"?";s.AppendLine("Prefab="+p+" Name="+name+" Overridden="+World.GetExistingSystemManaged<OverrideSystem>().IsOverridden(p));if(EntityManager.HasComponent<TrafficLightData>(p))s.AppendLine("Type="+EntityManager.GetComponentData<TrafficLightData>(p).m_Type);}
 }
}
}



