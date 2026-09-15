namespace TrafficLightIntoKorea {
internal static class DrivingSideRules {
 // Signed lateral distance from the incoming road centerline, not from the existing pole.
 internal static bool Accept(float dx,float dz,float dirX,float dirZ,bool leftHandTraffic) {
  float side=(dx*dirZ-dz*dirX)*(leftHandTraffic?-1f:1f);
  return side>.35f;
 }
}
}
