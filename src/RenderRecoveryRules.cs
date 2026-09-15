namespace TrafficLightIntoKorea {
internal static class RenderRecoveryRules {
 internal static bool ProtectFar(bool automatic,int? mode)=>mode!=0&&(automatic||mode==1);
 // A previously rendered object may briefly lose its batch during highlight/LOD changes.
 internal static bool KeepCustom(bool wasReady,bool visible,float missingSeconds)=>wasReady&&(!visible||missingSeconds<1f);
 // Never permanently abandon recovery after a tool-triggered batch rebuild.
 internal static float RetryDelay(int attempts)=>attempts<3?2f:10f;
}
}
