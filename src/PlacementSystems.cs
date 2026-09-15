using Game;
namespace TrafficLightIntoKorea {
public partial class OriginalPlacementSystem:GameSystemBase {
 protected override void OnUpdate(){World.GetExistingSystemManaged<OverrideSystem>()?.BeginPlacement();World.GetExistingSystemManaged<TlmCompatibilitySystem>()?.BeginPlacement();}
}
public partial class RestorePlacementVisualSystem:GameSystemBase {
 protected override void OnUpdate(){try{World.GetExistingSystemManaged<OverrideSystem>()?.EndPlacement();}finally{World.GetExistingSystemManaged<TlmCompatibilitySystem>()?.EndPlacement();}}
}
}
