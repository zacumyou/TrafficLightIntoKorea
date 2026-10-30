using Game;using Game.Common;using Game.Tools;using Unity.Collections;using Unity.Entities;
namespace TrafficLightIntoKorea {
// Observe the final native modification result, not temporary upgrade previews.
public partial class NativeSignalRemovalSystem:GameSystemBase {
 private EntityQuery _removed;
 protected override void OnCreate(){base.OnCreate();_removed=GetEntityQuery(ComponentType.ReadOnly<KoreanJunction>(),ComponentType.ReadOnly<Game.Net.Node>(),ComponentType.Exclude<Game.Net.TrafficLights>(),ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>());}
 protected override void OnUpdate(){
  using(var nodes=_removed.ToEntityArray(Allocator.Temp))foreach(var node in nodes)
   World.GetExistingSystemManaged<KoreanJunctionSystem>()?.NativeSignalsRemoved(node);
 }
}
}
