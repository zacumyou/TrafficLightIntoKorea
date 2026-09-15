namespace TrafficLightIntoKorea {
internal static class SignalControllerTypes {
 internal static readonly string[] Names={"TrafficLightManager.Code.Components.CustomTrafficLights","C2VM.TrafficToolEssentials.Components.CustomTrafficLights"};
 internal static string Label(string fullName){if(fullName==Names[0])return "TLM";if(fullName==Names[1])return "TTE";return "";}
}
}
