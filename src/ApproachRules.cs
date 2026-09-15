using Game.Net;
namespace TrafficLightIntoKorea {
internal static class ApproachRules {
 internal static int Movement(CarLaneFlags flags){if((flags&(CarLaneFlags.Forbidden|CarLaneFlags.UTurnLeft|CarLaneFlags.UTurnRight))!=0)return 0;int result=0;if((flags&CarLaneFlags.Forward)!=0)result|=1;if((flags&(CarLaneFlags.TurnLeft|CarLaneFlags.GentleTurnLeft))!=0)result|=2;return result;}
 internal static bool SameApproach(float agreement,float distanceSquared)=>agreement>.98f&&distanceSquared<=144f;
 internal static bool StraightLeft(int movements)=>(movements&3)==3;
}
}
