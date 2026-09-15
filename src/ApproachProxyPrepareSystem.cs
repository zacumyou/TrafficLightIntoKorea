using Game;
namespace TrafficLightIntoKorea {
// Registration must precede native initialization in the SAME PrefabUpdate pass.
// Created markers may be consumed at the end of that pass.
public partial class ApproachProxyPrepareSystem:GameSystemBase {
 protected override void OnUpdate(){World.GetOrCreateSystemManaged<TrafficSignSystem>().PrepareAssets();World.GetExistingSystemManaged<OverrideSystem>()?.PrepareQueuedProxies();}
}
}
