namespace TrafficLightIntoKorea {
// Native mesh allocation happens after our systems return. Bound new signal
// instances, not only the CPU time spent planning their placements.
internal static class DisplayCreationBudget {
 private static int _frame=-1,_created;
 internal static bool Take(int frame){if(_frame!=frame){_frame=frame;_created=0;}if(_created>=4)return false;_created++;return true;}
}
}
