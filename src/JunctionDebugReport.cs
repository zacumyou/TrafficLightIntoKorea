using System;using System.IO;using System.Text;using Game.Common;using Game.Prefabs;using Game.Tools;using Unity.Entities;
namespace TrafficLightIntoKorea {
public partial class LeftSignalSystem {
 internal string WriteJunctionReport(Entity node,RaycastHit hit){
  var s=new StringBuilder();s.AppendLine("TRAFFIC LIGHT INTO KOREA 2.3.28 / junction snapshot");s.AppendLine("UTC="+DateTime.UtcNow.ToString("O"));s.AppendLine("Node="+node+" Hit="+hit.m_HitEntity+" Position="+hit.m_HitPosition);
  s.AppendLine("Enabled="+Mod.Settings.Enabled+" Far="+Mod.Settings.AddFarSignals+" StraightLeft="+Mod.Settings.UseStraightLeftAssets+" Inset="+Mod.Settings.FarSidewalkInset+" LeftHandTraffic="+World.GetOrCreateSystemManaged<Game.City.CityConfigurationSystem>().leftHandTraffic);
  s.AppendLine("VehicleChoice="+Mod.Settings.StraightLeftVehicleAsset+" CrossingChoice="+Mod.Settings.StraightLeftCrosswalkAsset);
  s.AppendLine(TlmCompatibilitySystem.Status);Describe(s,node,"JUNCTION");
  if(EntityManager.HasBuffer<Game.Net.ConnectedEdge>(node))foreach(var item in EntityManager.GetBuffer<Game.Net.ConnectedEdge>(node,true)){
   var e=item.m_Edge;Describe(s,e,"CONNECTED_EDGE");if(EntityManager.HasComponent<Game.Net.Edge>(e)){var edge=EntityManager.GetComponentData<Game.Net.Edge>(e);s.AppendLine("Start="+edge.m_Start+" End="+edge.m_End);}if(EntityManager.HasComponent<Game.Net.Curve>(e)){var c=EntityManager.GetComponentData<Game.Net.Curve>(e);s.AppendLine("Curve="+c.m_Bezier+" Length="+c.m_Length);}
   if(EntityManager.HasComponent<Game.Net.Composition>(e)){var comp=EntityManager.GetComponentData<Game.Net.Composition>(e).m_Edge;s.AppendLine("Composition="+comp);if(EntityManager.HasBuffer<NetCompositionLane>(comp))foreach(var lane in EntityManager.GetBuffer<NetCompositionLane>(comp,true))s.AppendLine("CompositionLane="+lane.m_Lane+" Flags="+lane.m_Flags+" Position="+lane.m_Position);}
  }
  if(EntityManager.HasBuffer<Game.Objects.SubObject>(node))foreach(var child in EntityManager.GetBuffer<Game.Objects.SubObject>(node,true)){var e=child.m_SubObject;Describe(s,e,"JUNCTION_OBJECT");if(EntityManager.HasComponent<Game.Objects.TrafficLight>(e)&&EntityManager.HasComponent<Game.Objects.Transform>(e)){bool found=SignalApproach.Try(EntityManager,node,EntityManager.GetComponentData<Game.Objects.Transform>(e),out int road,out var direction);s.AppendLine("GeometryApproach="+found+" Road="+road+" Direction="+direction);}}
  if(EntityManager.HasBuffer<Game.Net.SubLane>(node))foreach(var sub in EntityManager.GetBuffer<Game.Net.SubLane>(node,true)){
   var lane=sub.m_SubLane;s.AppendLine("LANE="+lane+" Deleted="+EntityManager.HasComponent<Deleted>(lane)+" Temp="+EntityManager.HasComponent<Temp>(lane)+" Master="+EntityManager.HasComponent<Game.Net.MasterLane>(lane));
   if(EntityManager.HasComponent<Game.Net.CarLane>(lane))s.AppendLine("CarFlags="+EntityManager.GetComponentData<Game.Net.CarLane>(lane).m_Flags);
   if(EntityManager.HasComponent<Game.Net.Lane>(lane)){var l=EntityManager.GetComponentData<Game.Net.Lane>(lane);s.AppendLine("StartRoad="+l.m_StartNode.GetOwnerIndex()+" EndRoad="+l.m_EndNode.GetOwnerIndex());}
   if(EntityManager.HasComponent<Game.Net.LaneSignal>(lane))s.AppendLine("Group="+EntityManager.GetComponentData<Game.Net.LaneSignal>(lane).m_GroupMask+" LiveSignal="+EntityManager.GetComponentData<Game.Net.LaneSignal>(lane).m_Signal);
   if(EntityManager.HasComponent<Game.Net.Curve>(lane)){var b=EntityManager.GetComponentData<Game.Net.Curve>(lane).m_Bezier;s.AppendLine("A="+b.a+" B="+b.b+" C="+b.c+" D="+b.d);}
  }
  World.GetExistingSystemManaged<ApproachSignalSystem>()?.DescribeApproaches(node,s);
  World.GetExistingSystemManaged<FarSignalSystem>()?.DescribePlacement(node,s);
  var folder=Path.Combine(UnityEngine.Application.persistentDataPath,"ModsData","TrafficLightIntoKorea","Debug");Directory.CreateDirectory(folder);var path=Path.Combine(folder,"junction-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N")+".txt");File.WriteAllText(path,s.ToString(),Encoding.UTF8);return path;
 }
}
}



