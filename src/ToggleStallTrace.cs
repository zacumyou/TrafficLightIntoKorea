using System;using System.Diagnostics;
namespace TrafficLightIntoKorea {
// Enabled briefly by explicit toggle/bulk commands. No stack walking, forced GC,
// job completion or disk output for ordinary frames.
internal static class ToggleStallTrace {
 private static long _until,_last;private static string _phase;private static bool _exit;private static int _frame,_gc,_remaining;

 internal static void Arm(){_until=Stopwatch.GetTimestamp()+Stopwatch.Frequency*10;_last=0;_remaining=12;}
 internal static void Checkpoint(string phase,bool exit){
  bool detailed=Diagnostics.Probe.Session!=null;if(!detailed&&_remaining==0)return;long now=Stopwatch.GetTimestamp();if(!detailed&&now>_until){_remaining=0;return;}
  int frame=UnityEngine.Time.frameCount,gc=GC.CollectionCount(0);
  double gap=_last==0?0:(now-_last)*1000.0/Stopwatch.Frequency;
  if(gap>=100){if(!detailed)_remaining--;RuntimeDiagnostics.Event("Toggle gap ms="+gap.ToString("F2")+" from="+_phase+(_exit?" exit":" enter")+" to="+phase+(exit?" exit":" enter")+" frames="+_frame+"->"+frame+" gc0Delta="+(gc-_gc));}
  _last=Stopwatch.GetTimestamp();_phase=phase;_exit=exit;_frame=frame;_gc=gc;
 }
}
}
