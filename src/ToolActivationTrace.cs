using System.Diagnostics;
namespace TrafficLightIntoKorea {
// One record per accepted menu selection; no tracing of rejected prefab probes.
internal static class ToolActivationTrace {
 private static string _tool;
 private static long _requested;
 internal static void Request(string tool){
  ToggleStallTrace.Arm();_tool=tool;_requested=Stopwatch.GetTimestamp();
  RuntimeDiagnostics.Event("Tool activation requested "+tool);
 }
 internal static void FirstUpdate(string tool){
  if(_tool!=tool)return;
  double ms=(Stopwatch.GetTimestamp()-_requested)*1000.0/Stopwatch.Frequency;
  _tool=null;
  RuntimeDiagnostics.Event("Tool activation first update "+tool+" elapsedMs="+ms.ToString("F2"));
 }
}
}
