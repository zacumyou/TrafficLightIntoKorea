using Game;
namespace TrafficLightIntoKorea {
// Read-only phase markers. Never complete native jobs or alter rendering state.
public partial class NativeCullingStartTrace:GameSystemBase {
 protected override void OnUpdate(){ToggleStallTrace.Checkpoint("Native.PreCulling",false);}
}
public partial class NativeCullingEndTrace:GameSystemBase {
 protected override void OnUpdate(){ToggleStallTrace.Checkpoint("Native.PreCulling",true);}
}
}
