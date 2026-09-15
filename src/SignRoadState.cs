using System;
using Game.Common;
using Game.Net;
using Game.Objects;

using Unity.Entities;
using Unity.Mathematics;
namespace TrafficLightIntoKorea {
internal sealed class SignRoadState {
 internal string Ban;internal bool BanKnown;
 internal Entity Owner,Edge;internal int Straight,Left,Observed,Users,Index;internal string Speed;internal float NextRefresh;
 internal string Controller;
 internal bool Valid,SpeedKnown,SignalDataKnown,OrderedProgram;
 internal bool ModeKnown=>SignalDataKnown&&(OrderedProgram||Observed!=0||Straight==Left||Straight==0||Left==0||(Left&~Straight)!=0);
 internal void Read(EntityManager em,float2 incoming){
  Ban=BanRestrictions.Read(em,Owner,Edge,out BanKnown);
  string speed=null;int straight=0,left=0;bool haveS=false,haveL=false,consistent=true,haveSpeed=false;
  if(!em.Exists(Owner)||!em.Exists(Edge)||em.HasComponent<Deleted>(Owner)||em.HasComponent<Deleted>(Edge)){Valid=false;SpeedKnown=SignalDataKnown=false;Speed=null;Straight=Left=Observed=0;return;}
  if(em.HasBuffer<Game.Net.SubLane>(Edge))foreach(var item in em.GetBuffer<Game.Net.SubLane>(Edge,true)){
   var lane=item.m_SubLane;if(!Usable(em,lane)||!em.HasComponent<Curve>(lane))continue;
   var curve=em.GetComponentData<Curve>(lane).m_Bezier;var tangent=math.normalizesafe((curve.d-curve.a).xz);
   if(em.HasComponent<Node>(Owner)){var node=em.GetComponentData<Node>(Owner).m_Position;tangent=math.normalizesafe((math.distancesq(curve.a,node)<math.distancesq(curve.d,node)?curve.b-curve.a:curve.d-curve.c).xz);}
   if(math.dot(tangent,incoming)<.5f)continue;
   haveSpeed=true;var candidate=SignRules.Speed(em.GetComponentData<CarLane>(lane).m_SpeedLimit);
   if(candidate==null){consistent=false;break;}if(speed!=null&&speed!=candidate){consistent=false;break;}speed=candidate;
  }
  // No matching car lane: do not invent a speed from a junction's turn lane.
  if(!consistent)speed=null;SpeedKnown=haveSpeed;
  consistent=true;
  var roads=LaneTopology.LiveRoads(em,Owner);
  if(em.HasBuffer<Game.Net.SubLane>(Owner))foreach(var item in em.GetBuffer<Game.Net.SubLane>(Owner,true)){
   var lane=item.m_SubLane;if(!Usable(em,lane)||!em.HasComponent<LaneSignal>(lane)||!em.HasComponent<Lane>(lane)||!em.HasComponent<Curve>(lane))continue;
   var path=em.GetComponentData<Lane>(lane);if(path.m_StartNode.GetOwnerIndex()!=Edge.Index||!LaneTopology.ConnectsLiveRoads(roads,path))continue;
   var b=em.GetComponentData<Curve>(lane).m_Bezier;var a=math.normalizesafe((b.b-b.a).xz);var z=math.normalizesafe((b.d-b.c).xz);
   int movement=MovementGeometry.Classify(em.GetComponentData<CarLane>(lane).m_Flags,math.dot(a,z),a.x*z.y-a.y*z.x),mask=em.GetComponentData<LaneSignal>(lane).m_GroupMask;
   if(movement==1){if(haveS&&straight!=mask)consistent=false;straight=mask;haveS=true;}if(movement==2){if(haveL&&left!=mask)consistent=false;left=mask;haveL=true;}
  }
  SignalDataKnown=consistent&&(haveS||haveL)&&em.HasComponent<Game.Net.TrafficLights>(Owner);
  Controller=SignPhaseController.Provider(em,Owner);OrderedProgram=Controller!=null;
  if(!consistent||!em.HasComponent<Game.Net.TrafficLights>(Owner)){straight=left=0;}
  if(Straight!=straight||Left!=left){Straight=straight;Left=left;Observed=0;}
  Speed=speed;Valid=true;
 }
 private static bool Usable(EntityManager em,Entity e)=>em.Exists(e)&&!em.HasComponent<Deleted>(e)&&!em.HasComponent<Game.Tools.Temp>(e)&&!em.HasComponent<MasterLane>(e)&&em.HasComponent<CarLane>(e)&&(em.GetComponentData<CarLane>(e).m_Flags&(CarLaneFlags.Forbidden|CarLaneFlags.UTurnLeft|CarLaneFlags.UTurnRight))==0;
 internal void Observe(EntityManager em){if(!Valid||!em.Exists(Owner)||!em.HasComponent<Game.Net.TrafficLights>(Owner))return;var node=em.GetComponentData<Game.Net.TrafficLights>(Owner);
  if(OrderedProgram){Observed=SignRules.Ordered(Straight,Left,node.m_SignalGroupCount);return;}
  // Unordered native controllers still require observed transitions.
  if(node.m_State==Game.Net.TrafficLightState.Ending||node.m_State==Game.Net.TrafficLightState.Changing)Observed=SignRules.Observe(Observed,Straight,Left,node.m_CurrentSignalGroup,node.m_NextSignalGroup);
 }
 internal string Mode=>Valid?SignRules.Mode(Straight,Left,Observed):null;
}
}


