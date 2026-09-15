using Game.Settings;
namespace TrafficLightIntoKorea {
// Approved distribution data. Legacy property writes are ignored deliberately.
internal static class FixedSignalData {
 internal const bool UseStraightLeftAssets=true;
 internal const string StraightLeftVehicleAsset="CSKR4w2lTrafficLightCarRight01";
 internal const string StraightLeftCrosswalkAsset="CSKR4w2lTrafficLightCarCrosswalk01";
 internal const string IndividualAssets="CSKR4w1lTrafficLightCarLeft01\nCSKR4w1lTrafficLightCarRight01\nCSKR4w1lTrafficLightCarCrosswalk01\nCSKR4w1lTrafficLightCarCrosswalkLeft01";
 internal const string DirectionalTargets="CSKRTL4w2l Mesh\nCSKRTL4w2lPed Mesh\nCSKRTL4w1l Mesh\nCSKRTL4w1lMirrored Mesh\nCSKRTL4w2lMirrored Mesh";
 internal const string DirectionalMesh="CSKRTL4w2lMirrored Mesh";
 internal const bool SecondHead=false;
 internal const int RedId=5;
 internal const int YellowId=10;
 internal const int StraightId=15;
 internal const int ArrowId=50;
 internal const int RedId2=20;
 internal const int YellowId2=25;
 internal const int StraightId2=30;
 internal const int ArrowId2=50;
}
public sealed partial class Settings {
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public bool UseStraightLeftAssets {get=>FixedSignalData.UseStraightLeftAssets;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public string StraightLeftVehicleAsset {get=>FixedSignalData.StraightLeftVehicleAsset;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public string StraightLeftCrosswalkAsset {get=>FixedSignalData.StraightLeftCrosswalkAsset;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public string DirectionalMesh {get=>FixedSignalData.DirectionalMesh;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public bool SecondHead {get=>FixedSignalData.SecondHead;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public int RedId {get=>FixedSignalData.RedId;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public int YellowId {get=>FixedSignalData.YellowId;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public int StraightId {get=>FixedSignalData.StraightId;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public int ArrowId {get=>FixedSignalData.ArrowId;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public int RedId2 {get=>FixedSignalData.RedId2;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public int YellowId2 {get=>FixedSignalData.YellowId2;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public int StraightId2 {get=>FixedSignalData.StraightId2;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public int ArrowId2 {get=>FixedSignalData.ArrowId2;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public string IndividualAssets {get=>FixedSignalData.IndividualAssets;set{}}
 [SettingsUIHidden, Newtonsoft.Json.JsonIgnore] public string DirectionalTargets {get=>FixedSignalData.DirectionalTargets;set{}}
}
}
