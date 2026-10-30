namespace TrafficLightIntoKorea {
internal static class DisplayMaintenance {
 // Only stable renderer/child visibility maintenance is delayed; signal state is not.
 internal static bool Due(float now,float next,bool protectedHides,bool visibilityChanged)=>!protectedHides||visibilityChanged||now>=next;
 internal static float Next(float now,bool ready,int sourceIndex)=>ready?now+5f+(sourceIndex&15)/15f:now;
}
}
