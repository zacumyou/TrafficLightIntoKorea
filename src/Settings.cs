using Game.Modding; using Game.Settings; using Game.UI.Widgets; using Colossal.IO.AssetDatabase;
namespace TrafficLightIntoKorea {
[FileLocation("TrafficLightIntoKorea")][SettingsUITabOrder("Lights","Advanced","Troubleshooting","Other")][SettingsUIGroupOrder("General","DirectionalControl","StraightLeft","EU","NA","Individual","Actions")]
public sealed partial class Settings:ModSetting {

 [SettingsUISection("Advanced","DirectionalControl")] public bool IndependentLights {get;set;}=true;
 
 public DropdownItem<string>[] DirectionalMeshes()=>DirectionalLightSystem.MeshItems(DirectionalMesh);
 [SettingsUISection("Advanced","DirectionalControl")] public bool IncludeYieldLeft {get;set;}
 
 
 
 
 
 
 
 
 
 [SettingsUIHidden] public string DirectionalStatus=>DirectionalLightSystem.Status;
 [SettingsUIHidden] public string SignStatus=>TrafficSignSystem.Status;
 public Settings(IMod mod):base(mod){SetDefaults();}
 
 
 public DropdownItem<string>[] ItemsStraightLeftVehicle()=>ApproachCatalog.Items(StraightLeftVehicleAsset,this,false);
 
 public DropdownItem<string>[] ItemsStraightLeftCrosswalk()=>ApproachCatalog.Items(StraightLeftCrosswalkAsset,this,true);
 [SettingsUIHidden] public int Schema {get;set;}
 [SettingsUISection("Lights","General")] public bool Enabled {get;set;}=true;
 [SettingsUISection("Lights","General")] public bool HideLeftReplacedLights {get;set;}
 [SettingsUISection("Lights","General")] public bool AddFarSignals {get;set;}
 [SettingsUISection("Advanced","General"),SettingsUISlider(min=0,max=3,step=.25f)] public float FarSidewalkInset {get;set;}=1f;
 [SettingsUIHidden] public string Status=>KoreanJunctionSystem.SelectedCount.ToString();
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCar01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCar01 {get;set;}="";
public DropdownItem<string>[] ItemsEUCar01()=>Catalog.Items(EUCar01,this);
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCarLeft01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCarLeft01 {get;set;}="";
public DropdownItem<string>[] ItemsEUCarLeft01()=>Catalog.Items(EUCarLeft01,this);
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCarRight01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCarRight01 {get;set;}="";
public DropdownItem<string>[] ItemsEUCarRight01()=>Catalog.Items(EUCarRight01,this);
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCarCrosswalk01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCarCrosswalk01 {get;set;}="";
public DropdownItem<string>[] ItemsEUCarCrosswalk01()=>Catalog.Items(EUCarCrosswalk01,this);
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCarCrosswalkLeft01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCarCrosswalkLeft01 {get;set;}="";
public DropdownItem<string>[] ItemsEUCarCrosswalkLeft01()=>Catalog.Items(EUCarCrosswalkLeft01,this);
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCarCrosswalkRight01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCarCrosswalkRight01 {get;set;}="";
public DropdownItem<string>[] ItemsEUCarCrosswalkRight01()=>Catalog.Items(EUCarCrosswalkRight01,this);
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCarCrosswalkLeft02)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCarCrosswalkLeft02 {get;set;}="";
public DropdownItem<string>[] ItemsEUCarCrosswalkLeft02()=>Catalog.Items(EUCarCrosswalkLeft02,this);
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCarCrosswalkRight02)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCarCrosswalkRight02 {get;set;}="";
public DropdownItem<string>[] ItemsEUCarCrosswalkRight02()=>Catalog.Items(EUCarCrosswalkRight02,this);
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCarLeftCrosswalk01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCarLeftCrosswalk01 {get;set;}="";
public DropdownItem<string>[] ItemsEUCarLeftCrosswalk01()=>Catalog.Items(EUCarLeftCrosswalk01,this);
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCarRightCrosswalk01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCarRightCrosswalk01 {get;set;}="";
public DropdownItem<string>[] ItemsEUCarRightCrosswalk01()=>Catalog.Items(EUCarRightCrosswalk01,this);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACar01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACar01 {get;set;}="";
public DropdownItem<string>[] ItemsNACar01()=>Catalog.Items(NACar01,this);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACarLeft01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACarLeft01 {get;set;}="";
public DropdownItem<string>[] ItemsNACarLeft01()=>Catalog.Items(NACarLeft01,this);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACarRight01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACarRight01 {get;set;}="";
public DropdownItem<string>[] ItemsNACarRight01()=>Catalog.Items(NACarRight01,this);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACarCrosswalk01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACarCrosswalk01 {get;set;}="";
public DropdownItem<string>[] ItemsNACarCrosswalk01()=>Catalog.Items(NACarCrosswalk01,this);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACarCrosswalkLeft01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACarCrosswalkLeft01 {get;set;}="";
public DropdownItem<string>[] ItemsNACarCrosswalkLeft01()=>Catalog.Items(NACarCrosswalkLeft01,this);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACarCrosswalkRight01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACarCrosswalkRight01 {get;set;}="";
public DropdownItem<string>[] ItemsNACarCrosswalkRight01()=>Catalog.Items(NACarCrosswalkRight01,this);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACarCrosswalkLeft02)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACarCrosswalkLeft02 {get;set;}="";
public DropdownItem<string>[] ItemsNACarCrosswalkLeft02()=>Catalog.Items(NACarCrosswalkLeft02,this);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACarCrosswalkRight02)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACarCrosswalkRight02 {get;set;}="";
public DropdownItem<string>[] ItemsNACarCrosswalkRight02()=>Catalog.Items(NACarCrosswalkRight02,this);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACarLeftCrosswalk01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACarLeftCrosswalk01 {get;set;}="";
public DropdownItem<string>[] ItemsNACarLeftCrosswalk01()=>Catalog.Items(NACarLeftCrosswalk01,this);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACarRightCrosswalk01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACarRightCrosswalk01 {get;set;}="";
public DropdownItem<string>[] ItemsNACarRightCrosswalk01()=>Catalog.Items(NACarRightCrosswalk01,this);
[SettingsUISection("Advanced","EU"),SettingsUIHideByCondition(typeof(Settings),nameof(HideEUAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsEUCrosswalk01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string EUCrosswalk01 {get;set;}="";
public DropdownItem<string>[] ItemsEUCrosswalk01()=>Catalog.Items(EUCrosswalk01,this,true);
[SettingsUISection("Advanced","NA"),SettingsUIHideByCondition(typeof(Settings),nameof(HideNAAssets)),SettingsUIDropdown(typeof(Settings),nameof(ItemsNACrosswalk01)),SettingsUIValueVersion(typeof(Settings),nameof(CatalogVersion))] public string NACrosswalk01 {get;set;}="";
public DropdownItem<string>[] ItemsNACrosswalk01()=>Catalog.Items(NACrosswalk01,this,true);
[SettingsUISection("Troubleshooting","Actions"),SettingsUIButton] public bool ReloadAssets {set{Unity.Entities.World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<OverrideSystem>()?.RequestReload();}}
[SettingsUIHidden] public string ReloadStatus=>Locale.ReloadStatus(this);
[SettingsUISection("Troubleshooting","Actions"),SettingsUIButton] public bool StartDebug {set{SignalDebugTool.Start();}}
[SettingsUISection("Troubleshooting","Actions"),SettingsUIButton] public bool StopDebug {set{SignalDebugTool.Stop();}}
[SettingsUISection("Troubleshooting","Actions")] public string DebugReport=>SignalDebugTool.LastReport;
[SettingsUIHidden] public int CatalogVersion=>Catalog.Version;
internal string Selection(string source){switch(source){case "EU_TrafficLightCar01":return EUCar01??"";case "EU_TrafficLightCarLeft01":return EUCarLeft01??"";case "EU_TrafficLightCarRight01":return EUCarRight01??"";case "EU_TrafficLightCarCrosswalk01":return EUCarCrosswalk01??"";case "EU_TrafficLightCarCrosswalkLeft01":return EUCarCrosswalkLeft01??"";case "EU_TrafficLightCarCrosswalkRight01":return EUCarCrosswalkRight01??"";case "EU_TrafficLightCarCrosswalkLeft02":return EUCarCrosswalkLeft02??"";case "EU_TrafficLightCarCrosswalkRight02":return EUCarCrosswalkRight02??"";case "EU_TrafficLightCarLeftCrosswalk01":return EUCarLeftCrosswalk01??"";case "EU_TrafficLightCarRightCrosswalk01":return EUCarRightCrosswalk01??"";case "NA_TrafficLightCar01":return NACar01??"";case "NA_TrafficLightCarLeft01":return NACarLeft01??"";case "NA_TrafficLightCarRight01":return NACarRight01??"";case "NA_TrafficLightCarCrosswalk01":return NACarCrosswalk01??"";case "NA_TrafficLightCarCrosswalkLeft01":return NACarCrosswalkLeft01??"";case "NA_TrafficLightCarCrosswalkRight01":return NACarCrosswalkRight01??"";case "NA_TrafficLightCarCrosswalkLeft02":return NACarCrosswalkLeft02??"";case "NA_TrafficLightCarCrosswalkRight02":return NACarCrosswalkRight02??"";case "NA_TrafficLightCarLeftCrosswalk01":return NACarLeftCrosswalk01??"";case "NA_TrafficLightCarRightCrosswalk01":return NACarRightCrosswalk01??"";case "EU_TrafficLightCrosswalk01":return EUCrosswalk01??"";case "NA_TrafficLightCrosswalk01":return NACrosswalk01??"";default:return "";}}
[SettingsUISection("Other","Actions"),SettingsUIButton] public bool EnableAll {set{IndependentLights=true;Enabled=true;HideLeftReplacedLights=true;AddFarSignals=true;UseStraightLeftAssets=true;ApplyAndSave();}}
[SettingsUISection("Other","Actions"),SettingsUIButton] public bool DisableAll {set{IndependentLights=false;Enabled=false;HideLeftReplacedLights=false;AddFarSignals=false;UseStraightLeftAssets=false;ApplyAndSave();}}
[SettingsUIHidden] public string RestoreAllStatus=>KoreanJunctionSystem.RestoreStatus;
[SettingsUISection("Other","Actions"),SettingsUIButton] public bool RestoreAllJunctions {set{KoreanJunctionSystem.RestoreAllFromSettings();}}
[SettingsUISection("Other","Actions"),SettingsUIButton] public bool Reset {set{SetDefaults();ApplyAndSave();}}
[SettingsUISection("Other","Actions")] public string Version=>"2.3.19";
public override void SetDefaults(){IndividualAssets="";IndividualAssetPicker="";KoreanTransitions=true;AmberSeconds=1f;IndependentLights=true;DirectionalMesh="CSKRTL4w2l Mesh";DirectionalTargets=DirectionalMesh;DirectionalTargetsMigrated=true;IncludeYieldLeft=SecondHead=false;RedId=5;YellowId=10;StraightId=15;ArrowId=35;RedId2=20;YellowId2=25;StraightId2=30;ArrowId2=40;Enabled=true;HideLeftReplacedLights=false;AddFarSignals=false;FarSidewalkInset=1f;UseStraightLeftAssets=false;StraightLeftVehicleAsset="";StraightLeftCrosswalkAsset="";Schema=1;NACrosswalk01="";EUCrosswalk01="";EUCar01="";EUCarLeft01="";EUCarRight01="";EUCarCrosswalk01="";EUCarCrosswalkLeft01="";EUCarCrosswalkRight01="";EUCarCrosswalkLeft02="";EUCarCrosswalkRight02="";EUCarLeftCrosswalk01="";EUCarRightCrosswalk01="";NACar01="";NACarLeft01="";NACarRight01="";NACarCrosswalk01="";NACarCrosswalkLeft01="";NACarCrosswalkRight01="";NACarCrosswalkLeft02="";NACarCrosswalkRight02="";NACarLeftCrosswalk01="";NACarRightCrosswalk01="";ApplyDistributionDefaults();}
internal void Migrate(IMod mod){MigrateDirectionalTargets();if(Schema!=0)return;var legacy=new LegacySettings(mod);AssetDatabase.global.LoadSettings("ReplaceRoadSigns",legacy,new LegacySettings(mod));EUCar01=legacy.EUCar01??"";EUCarLeft01=legacy.EUCarLeft01??"";EUCarRight01=legacy.EUCarRight01??"";EUCarCrosswalk01=legacy.EUCarCrosswalk01??"";EUCarCrosswalkLeft01=legacy.EUCarCrosswalkLeft01??"";EUCarCrosswalkRight01=legacy.EUCarCrosswalkRight01??"";EUCarCrosswalkLeft02=legacy.EUCarCrosswalkLeft02??"";EUCarCrosswalkRight02=legacy.EUCarCrosswalkRight02??"";EUCarLeftCrosswalk01=legacy.EUCarLeftCrosswalk01??"";EUCarRightCrosswalk01=legacy.EUCarRightCrosswalk01??"";NACar01=legacy.NACar01??"";NACarLeft01=legacy.NACarLeft01??"";NACarRight01=legacy.NACarRight01??"";NACarCrosswalk01=legacy.NACarCrosswalk01??"";NACarCrosswalkLeft01=legacy.NACarCrosswalkLeft01??"";NACarCrosswalkRight01=legacy.NACarCrosswalkRight01??"";NACarCrosswalkLeft02=legacy.NACarCrosswalkLeft02??"";NACarCrosswalkRight02=legacy.NACarCrosswalkRight02??"";NACarLeftCrosswalk01=legacy.NACarLeftCrosswalk01??"";NACarRightCrosswalk01=legacy.NACarRightCrosswalk01??"";Schema=1;}
}
[FileLocation("ReplaceRoadSigns")]public sealed class LegacySettings:ModSetting {public LegacySettings(IMod mod):base(mod){} public override void SetDefaults(){}
public string EUCar01 {get;set;}="";
public string EUCarLeft01 {get;set;}="";
public string EUCarRight01 {get;set;}="";
public string EUCarCrosswalk01 {get;set;}="";
public string EUCarCrosswalkLeft01 {get;set;}="";
public string EUCarCrosswalkRight01 {get;set;}="";
public string EUCarCrosswalkLeft02 {get;set;}="";
public string EUCarCrosswalkRight02 {get;set;}="";
public string EUCarLeftCrosswalk01 {get;set;}="";
public string EUCarRightCrosswalk01 {get;set;}="";
public string NACar01 {get;set;}="";
public string NACarLeft01 {get;set;}="";
public string NACarRight01 {get;set;}="";
public string NACarCrosswalk01 {get;set;}="";
public string NACarCrosswalkLeft01 {get;set;}="";
public string NACarCrosswalkRight01 {get;set;}="";
public string NACarCrosswalkLeft02 {get;set;}="";
public string NACarCrosswalkRight02 {get;set;}="";
public string NACarLeftCrosswalk01 {get;set;}="";
public string NACarRightCrosswalk01 {get;set;}="";
}}




