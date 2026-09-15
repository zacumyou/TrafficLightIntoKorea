namespace TrafficLightIntoKorea {internal static class BranchRules {
 // Distinct exit roads at least 35 degrees apart; lane fans on one exit do not count.
 internal static bool Separate(float dot)=>dot<=0.819152f;
}}
