using System.Diagnostics;
using UnityEngine;
namespace TrafficLightIntoKorea {
// Shared across near and far rebuilding. A single junction cannot be interrupted.
internal static class SignalWorkBudget {
 private static int _frame=-1,_owners;private static long _start;
 internal static bool Available {get{if(_frame!=Time.frameCount){_frame=Time.frameCount;_owners=0;_start=Stopwatch.GetTimestamp();}return _owners<4&&(Stopwatch.GetTimestamp()-_start)*1000.0/Stopwatch.Frequency<2.0;}}
 internal static bool TakeOwner(){bool available=Available;if(!available&&_owners!=0)return false;_owners++;return true;}
}
}
