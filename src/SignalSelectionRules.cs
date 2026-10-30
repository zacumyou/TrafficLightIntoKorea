namespace TrafficLightIntoKorea {
internal static class SignalSelectionRules {
 internal static bool CanApply(bool live,bool node,bool nativeSignals)=>live&&node&&nativeSignals;
 internal static bool NativeRemoval(bool selected,bool node,bool nativeSignals,bool temporary)=>selected&&node&&!nativeSignals&&!temporary;
 internal static bool NativeSource(bool live,bool temporary,bool clone,bool retired)=>live&&!temporary&&!clone&&!retired;
}
}
