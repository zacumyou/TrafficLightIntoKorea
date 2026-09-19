namespace TrafficLightIntoKorea {
internal static class SignalMaintenanceRules {
 internal const float SignInterval=.25f;
 internal static bool Due(float now,float next)=>now>=next;
}
}
