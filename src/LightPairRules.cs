using System;
namespace TrafficLightIntoKorea {
internal static class LightPairRules {
 internal static bool RightPartner(float lateral,float longitudinal,float vertical,float directionAgreement,int ownMask,int otherMask) =>
  (ownMask & otherMask)!=0 && lateral>1f && lateral<40f && Math.Abs(longitudinal)<3f && Math.Abs(vertical)<2f && directionAgreement>0.98f;
}
}
