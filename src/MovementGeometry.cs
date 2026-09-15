using Game.Net;
namespace TrafficLightIntoKorea {
internal static class MovementGeometry {
 internal static int Classify(CarLaneFlags flags,float agreement,float left){
  if((flags&(CarLaneFlags.Forbidden|CarLaneFlags.UTurnLeft|CarLaneFlags.UTurnRight))!=0)return 0;
  if((flags&(CarLaneFlags.TurnLeft|CarLaneFlags.GentleTurnLeft))!=0)return 2;
  if((flags&(CarLaneFlags.TurnRight|CarLaneFlags.GentleTurnRight))!=0)return 0;
  if(agreement>.7f)return 1;
  return left>.35f&&agreement>-.85f?2:0;
 }
}
}
