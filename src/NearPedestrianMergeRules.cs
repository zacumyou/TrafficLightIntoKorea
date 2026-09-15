using System;
namespace TrafficLightIntoKorea {
internal static class NearPedestrianMergeRules {
 internal static bool Compatible(int vehicleMask,int existingPedestrianMask,int pedestrianMask,float distanceSquared,float rise) =>
  vehicleMask!=0&&pedestrianMask!=0&&(existingPedestrianMask==0||existingPedestrianMask==pedestrianMask)&&distanceSquared<=16f&&Math.Abs(rise)<=.75f;
 internal static bool SameEnd(float va,float vb,float pa,float pb,float vehicleRadiusSquared=16f) =>
  Math.Min(va,vb)<=vehicleRadiusSquared&&Math.Min(pa,pb)<=16f&&Math.Abs(va-vb)>1f&&Math.Abs(pa-pb)>1f&&(va<vb)==(pa<pb);
}
}
