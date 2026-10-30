using System;using System.IO;using System.Linq;using System.Threading;using System.Reflection;using Newtonsoft.Json.Linq;using TrafficLightIntoKorea;using TrafficLightIntoKorea.Diagnostics;
namespace UnityEngine {public static class Time{public static int frameCount;}}
namespace TrafficLightIntoKorea {public class Mod{}}
namespace TrafficLightIntoKorea.Diagnostics {
 internal static class Controller {internal static bool InGame=true;}
 // Test-only path shim keeps fixture logs outside the game's real data folder.
 internal static class Environment {internal enum SpecialFolder{LocalApplicationData}internal static string GetFolderPath(SpecialFolder _)=>Path.Combine(Path.GetTempPath(),"tlik-perf-"+System.Environment.ProcessId);}
}
class Program {
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 sealed class Payload {internal static int ThreadId;public string Value{get{ThreadId=Thread.CurrentThread.ManagedThreadId;return "snapshot";}}}
 static void Main(){var q=new PanelCommandQueue();q.Enqueue("prop:a:0");q.Enqueue("prop:b:1");Check(q.Dequeue()=="prop:a:0"&&q.Dequeue()=="prop:b:1","rapid command order");q.Enqueue("old");q.Clear();Check(q.Count==0,"closed panel discards commands");
 Probe.Enable(true);var session=Probe.Session;Probe.Emit("TEST_SNAPSHOT",new Payload());Check(Payload.ThreadId==0,"serialization must not run in Emit");Probe.Frame();for(int i=0;i<200;i++){UnityEngine.Time.frameCount++;Probe.Frame();}typeof(Probe).GetField("frameStart",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,Probe.Now-System.Diagnostics.Stopwatch.Frequency/5);Probe.Frame();Probe.Aggregate("fixture",2.5,true);typeof(Probe).GetField("nextSummary",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,0L);Probe.Frame();Probe.Start();Probe.Stop();Check(Payload.ThreadId!=Thread.CurrentThread.ManagedThreadId,"worker serialization");
 var rows=Directory.GetFiles(Probe.DirectoryPath,session+"-*.jsonl").SelectMany(File.ReadAllLines).Select(JObject.Parse).ToArray();Check(rows.Count(r=>(string)r["kind"]=="FRAME_SUMMARY")==2,"bounded frame summaries");Check(rows.Count(r=>(string)r["kind"]=="FRAME_SLOW")==1,"slow frame retained");Check(rows.Any(r=>(string)r["kind"]=="SYSTEM_SUMMARY"&&(double)r["data"]["inclusiveMs"]==2.5),"span summary retained");Check((string)rows.Last()["kind"]=="SESSION_STOP","stop drains queue");var status=JObject.Parse(File.ReadAllText(Path.Combine(Probe.DirectoryPath,"detail-status.json")));Check(status["session"].Type==JTokenType.Null,"stopped status refreshed");var seq=rows.Select(r=>(long)r["seq"]).ToArray();Check(seq.SequenceEqual(seq.OrderBy(x=>x)),"event order");
 Controller.InGame=false;Probe.Enable(true);Probe.Emit("BLOCKED",new{});Probe.Start();Probe.Stop();Check(!Directory.GetFiles(Probe.DirectoryPath,"*.jsonl").SelectMany(File.ReadAllLines).Any(x=>x.Contains("BLOCKED")),"outside gameplay excluded");Console.WriteLine("PASS: worker serialization, frame aggregation/slow frames, queue drain/restart, inactive status, gameplay gate, ordered panel commands. Fixture only.");
 }
}
