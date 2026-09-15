using Game.Settings;
namespace TrafficLightIntoKorea {
public sealed partial class Settings {
 [SettingsUISection("Advanced","EU")] public bool ExpandEU {get;set;}
 [SettingsUISection("Advanced","NA")] public bool ExpandNA {get;set;}
 public bool HideEUAssets()=>!ExpandEU;
 public bool HideNAAssets()=>!ExpandNA;
 private void ApplyDistributionDefaults(){
  DirectionalTargets="CSKRTL4w2l Mesh\nCSKRTL4w2lPed Mesh\nCSKRTL4w1l Mesh\nCSKRTL4w1lMirrored Mesh\nCSKRTL4w2lMirrored Mesh";
  DirectionalTargetsMigrated=true;
  IndividualAssets="CSKR4w1lTrafficLightCarLeft01\nCSKR4w1lTrafficLightCarRight01\nCSKR4w1lTrafficLightCarCrosswalk01\nCSKR4w1lTrafficLightCarCrosswalkLeft01";
  IndividualAssetPicker="";
  DirectionalMesh="CSKRTL4w2lMirrored Mesh";
  IncludeYieldLeft=true;
  ArrowId=50;
  ArrowId2=50;
  UseStraightLeftAssets=true;
  StraightLeftVehicleAsset="CSKR4w2lTrafficLightCarRight01";
  StraightLeftCrosswalkAsset="CSKR4w2lTrafficLightCarCrosswalk01";
  Schema=1;
  HideLeftReplacedLights=true;
  AddFarSignals=false;
  EUCar01="CSKR3w1lSTrafficLightCar01";
  EUCarLeft01="CSKR4w2lTrafficLightCarLeft01";
  EUCarRight01="CSKR4w2lTrafficLightCarRight01";
  EUCarCrosswalk01="CSKR3w1lSTrafficLightCarCrosswalk01";
  EUCarCrosswalkLeft01="CSKR3w1lSTrafficLightCarCrosswalk01";
  EUCarCrosswalkRight01="CSKR3w1lSTrafficLightCarCrosswalk01";
  EUCarCrosswalkLeft02="CSKR3w1lSTrafficLightCarCrosswalk01";
  EUCarCrosswalkRight02="CSKR3w1lSTrafficLightCarCrosswalk01";
  EUCarLeftCrosswalk01="CSKR3w1lSTrafficLightCarCrosswalk01";
  EUCarRightCrosswalk01="CSKR3w1lSTrafficLightCarCrosswalk01";
  NACar01="CSKR3w1lSTrafficLightCar01";
  NACarLeft01="CSKR3w1lSTrafficLightCarLeft01";
  NACarRight01="CSKR3w1lSTrafficLightCar01";
  NACarCrosswalk01="CSKR4w2lTrafficLightCarCrosswalk01";
  NACarCrosswalkLeft01="CSKR4w2lTrafficLightCarCrosswalk01";
  NACarCrosswalkRight01="CSKR3w1lSTrafficLightCarCrosswalk01";
  NACarCrosswalkLeft02="CSKR4w2lTrafficLightCarCrosswalk01";
  NACarCrosswalkRight02="CSKR4w2lTrafficLightCarCrosswalk01";
  NACarLeftCrosswalk01="CSKR4w2lTrafficLightCarCrosswalk01";
  NACarRightCrosswalk01="CSKR4w2lTrafficLightCarCrosswalk01";
  EUCrosswalk01="CSKRTLOnlyPed";
  NACrosswalk01="CSKRTLOnlyPed";
  Enabled=true;
  ExpandEU=false;ExpandNA=false;
 }
}
}
