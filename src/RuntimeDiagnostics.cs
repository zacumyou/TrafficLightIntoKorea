using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
namespace TrafficLightIntoKorea {
// Bounded, aggregated diagnostics; no per-entity logging and no Unity log callbacks.
internal static class RuntimeDiagnostics {
 private sealed class Counter {internal int Calls;internal double Total,Peak;}
 private static readonly Dictionary<string,Counter> Counters=new Dictionary<string,Counter>();
 private static readonly string DirectoryPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+"Low","Colossal Order","Cities Skylines II","ModsData","TrafficLightIntoKorea","Diagnostics");
 private static readonly string FilePath=Path.Combine(DirectoryPath,"runtime-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+".log");
 private static readonly Dictionary<string,long> Events=new Dictionary<string,long>();internal static void Count(string name){if(_failed)return;Events.TryGetValue(name,out var count);Events[name]=count+1;}private static int _lastFrame=-1,_frameSamples;private static double _frameMs,_framePeak;private static long _next;private static bool _failed;private static int _lines;
 internal struct Sample:IDisposable {private string _name;private long _start;internal Sample(string name){_name=name;_start=Stopwatch.GetTimestamp();}public void Dispose(){Record(_name,(Stopwatch.GetTimestamp()-_start)*1000.0/Stopwatch.Frequency);}}
 internal static Sample Measure(string name)=>new Sample(name);
 private static void Record(string name,double ms){if(_failed)return;int frame=UnityEngine.Time.frameCount;if(frame!=_lastFrame){_lastFrame=frame;double elapsed=UnityEngine.Time.unscaledDeltaTime*1000.0;if(elapsed>0){_frameSamples++;_frameMs+=elapsed;_framePeak=Math.Max(_framePeak,elapsed);}}if(!Counters.TryGetValue(name,out var c)){c=new Counter();Counters.Add(name,c);}c.Calls++;c.Total+=ms;c.Peak=Math.Max(c.Peak,ms);long now=Stopwatch.GetTimestamp();if(now<_next)return;_next=now+Stopwatch.Frequency*5;var text=new System.Text.StringBuilder();if(_frameSamples>0)text.Append("frames count=").Append(_frameSamples).Append(" avgMs=").Append((_frameMs/_frameSamples).ToString("F2")).Append(" peakMs=").Append(_framePeak.ToString("F2")).AppendLine();_frameSamples=0;_frameMs=0;_framePeak=0;foreach(var item in Counters){var v=item.Value;text.Append(item.Key).Append(" calls=").Append(v.Calls).Append(" totalMs=").Append(v.Total.ToString("F2")).Append(" peakMs=").Append(v.Peak.ToString("F2")).AppendLine();v.Calls=0;v.Total=v.Peak=0;}foreach(var item in Events)text.Append(item.Key).Append(" count=").Append(item.Value).AppendLine();Events.Clear();text.AppendLine(TrafficSignSystem.Status);Event(text.ToString());}
 internal static void Event(string text){if(_failed||_lines>=10000)return;try{System.IO.Directory.CreateDirectory(DirectoryPath);File.AppendAllText(FilePath,DateTime.UtcNow.ToString("O")+" 2.3.40 "+text+Environment.NewLine);_lines++;}catch{_failed=true;}}
}
}
