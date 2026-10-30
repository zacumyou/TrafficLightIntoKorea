using System;
namespace TrafficLightIntoKorea {
internal static class IndividualAssetRules {
 internal static bool KoreanVehicle(string name)=>!string.IsNullOrEmpty(name)
  &&name.IndexOf("CSKR",StringComparison.OrdinalIgnoreCase)>=0
  &&name.IndexOf("TrafficLight",StringComparison.OrdinalIgnoreCase)>=0
  &&name.IndexOf("Car",StringComparison.OrdinalIgnoreCase)>=0;
}
}
