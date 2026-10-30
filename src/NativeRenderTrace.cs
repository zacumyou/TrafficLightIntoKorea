using Game;
using Game.Tools;
namespace TrafficLightIntoKorea {
// Read-only markers: timings include native dependency waits; no forced completion.
// Tool changes arm the same bounded window for native and third-party tools.
public partial class ToolChangeTrace:GameSystemBase {
 private ToolBaseSystem _lastTool;
 protected override void OnUpdate(){
  if(Game.SceneFlow.GameManager.instance.gameMode!=GameMode.Game)return;
  var tool=World.GetExistingSystemManaged<ToolSystem>()?.activeTool;
  if(object.ReferenceEquals(tool,_lastTool))return;
  bool related=tool is KoreanJunctionTool||tool is IndividualSignalTool||tool is SignalDebugTool||_lastTool is KoreanJunctionTool||_lastTool is IndividualSignalTool||_lastTool is SignalDebugTool;_lastTool=tool;if(!related)return;ToggleStallTrace.Arm();
  RuntimeDiagnostics.Event("Active tool changed "+(tool?.toolID??"none"));
 }
}
public partial class NativeBatchMeshStartTrace:GameSystemBase {protected override void OnUpdate(){ToggleStallTrace.Checkpoint("Native.BatchMesh",false);}}
public partial class NativeBatchMeshEndTrace:GameSystemBase {protected override void OnUpdate(){ToggleStallTrace.Checkpoint("Native.BatchMesh",true);}}
public partial class NativeBatchInstanceStartTrace:GameSystemBase {protected override void OnUpdate(){ToggleStallTrace.Checkpoint("Native.BatchInstance",false);}}
public partial class NativeBatchInstanceEndTrace:GameSystemBase {protected override void OnUpdate(){ToggleStallTrace.Checkpoint("Native.BatchInstance",true);}}
public partial class NativeManagedBatchStartTrace:GameSystemBase {protected override void OnUpdate(){ToggleStallTrace.Checkpoint("Native.ManagedBatch",false);}}
public partial class NativeManagedBatchEndTrace:GameSystemBase {protected override void OnUpdate(){ToggleStallTrace.Checkpoint("Native.ManagedBatch",true);}}
public partial class NativeBatchManagerStartTrace:GameSystemBase {protected override void OnUpdate(){ToggleStallTrace.Checkpoint("Native.BatchManager",false);}}
public partial class NativeBatchManagerEndTrace:GameSystemBase {protected override void OnUpdate(){ToggleStallTrace.Checkpoint("Native.BatchManager",true);}}
}
