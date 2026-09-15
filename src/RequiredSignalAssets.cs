using System;using System.Collections.Generic;
namespace TrafficLightIntoKorea {
internal static class RequiredSignalAssets {
 internal static string[] Names(){var names=new HashSet<string>(StringComparer.Ordinal){"CSKR3w1lSTrafficLightCar01","CSKR4w2lTrafficLightCarLeft01","CSKR4w2lTrafficLightCarRight01","CSKR3w1lSTrafficLightCarCrosswalk01","CSKR3w1lSTrafficLightCarLeft01","CSKR4w2lTrafficLightCarCrosswalk01","CSKRTLOnlyPed",FixedSignalData.StraightLeftVehicleAsset,FixedSignalData.StraightLeftCrosswalkAsset,ThreeLampRules.DefaultVehicle,"CSKR3w2lTrafficLightCar01"};foreach(var name in DirectionalTargetNames.Parse(FixedSignalData.IndividualAssets))names.Add(name);var result=new string[names.Count];names.CopyTo(result);Array.Sort(result,StringComparer.Ordinal);return result;}
}
}
