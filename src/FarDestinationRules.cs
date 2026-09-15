using System;
namespace TrafficLightIntoKorea {
internal static class FarDestinationRules {
 // Search along the same forward corridor, not another arm of the junction.
 internal static float Corridor(float incomingWidth,float outgoingWidth)=>incomingWidth>0&&outgoingWidth>0?Math.Min(12f,3f+Math.Abs(outgoingWidth-incomingWidth)*.5f):3f;
 internal static bool Sidewalk(float forward,float lateralError,float longitudinalError,float rise,float travel,float corridor=3f) =>
  forward>=5f&&forward<=120f&&Math.Abs(lateralError)<=corridor&&Math.Abs(longitudinalError)<=Math.Max(5f,travel*.15f)&&Math.Abs(rise)<=3f;
 // Combining is a local fit after the vehicle destination has been resolved.
 internal static bool Pedestrian(float forward,float sideways,float rise,float distanceSquared) =>
  forward>=5f&&forward<=120f&&Math.Abs(sideways)<=16f&&Math.Abs(rise)<=1f&&distanceSquared<=16f;
}
}
