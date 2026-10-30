namespace TrafficLightIntoKorea {
internal static class SignalNodeRules {
 // Ordinary road seams are not targets; an existing selection must remain restorable.
 internal static bool Selectable(int connections,bool hasSignals,bool selected)=>connections>=3||(connections==2&&(hasSignals||selected));
}
}
