using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
namespace TrafficLightIntoKorea.Diagnostics {
internal static class Probe {
 internal static readonly List<string> Coverage=new List<string>();
 internal static readonly string DirectoryPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+"Low","Colossal Order","Cities Skylines II","ModsData","TrafficLightIntoKorea","Diagnostics");
 internal static volatile string Session;
 internal static string Status=>(Session??"OFF")+(string.IsNullOrEmpty(lastError)?"":" | "+lastError)+" | dropped="+Interlocked.Read(ref dropped)+" errors="+Interlocked.Read(ref errors);
 private sealed class Row {internal string Session;internal object Payload;}
 private sealed class Sample {internal long Calls,Enabled;internal double Total,Peak;}
 private static readonly ConcurrentQueue<Row> Queue=new ConcurrentQueue<Row>();
 private static readonly Dictionary<string,Sample> Totals=new Dictionary<string,Sample>();
 private static readonly object Gate=new object(),SessionGate=new object();
 private static Thread worker;private static volatile bool quit;private static long dropped,errors,serial,frameStart,nextSummary;private static int queued;
 private static int frameSamples;private static double frameTotal,framePeak;
 private static volatile string lastError="";private static string totalsSession;
 internal static long Now=>Stopwatch.GetTimestamp();internal static double Ms(long ticks)=>ticks*1000.0/Stopwatch.Frequency;
 internal static void RecordFailure()=>Interlocked.Increment(ref errors);
 internal static void Start(){if(worker!=null)return;quit=false;worker=new Thread(Loop){IsBackground=true,Name="TLIK detailed diagnostics writer"};worker.Start();}
 internal static void Enable(bool value){lock(SessionGate){if(value==(Session!=null))return;if(value){Session="detail-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N").Substring(0,6);frameStart=0;nextSummary=0;frameSamples=0;frameTotal=framePeak=0;Emit("SESSION_START",new{version=typeof(Mod).Assembly.GetName().Version.ToString(),coverage=Coverage.ToArray(),scope="Post-load gameplay only: TLIK internal spans, direct panel commands, signal creation/recovery events and frame context. No global UI, DOM, or ECS hooks. Native/Burst/GPU work is not individually timed.",limits="20,000 queued rows, 512 MiB per session (64 MiB parts), frame summaries once per second, individual frames at 100 ms or more. Health/status reports omissions. Instrumentation has overhead."});}else{Emit("SESSION_STOP",new{dropped,errors});Session=null;}}}
 internal static void Stop(){Enable(false);quit=true;if(worker==null||worker.Join(3000))worker=null;}
 // Callers pass immutable snapshots (strings/scalars/anonymous records), never live Unity objects.
 internal static void Emit(string kind,object data){if(Session==null||(!Controller.InGame&&kind!="SESSION_STOP"))return;try{lock(SessionGate){if(Session==null)return;if(Interlocked.CompareExchange(ref queued,0,0)>=20000){Interlocked.Increment(ref dropped);return;}var row=new Row{Session=Session,Payload=new{utc=DateTime.UtcNow,session=Session,seq=Interlocked.Increment(ref serial),thread=Thread.CurrentThread.ManagedThreadId,kind,data}};Interlocked.Increment(ref queued);Queue.Enqueue(row);}}catch{RecordFailure();}}
 internal static void Aggregate(string name,double ms,bool enabled){if(Session==null)return;lock(Gate){if(totalsSession!=Session){Totals.Clear();totalsSession=Session;}if(!Totals.TryGetValue(name,out var s))Totals[name]=s=new Sample();s.Calls++;s.Total+=ms;s.Peak=Math.Max(s.Peak,ms);if(enabled)s.Enabled++;}}
 internal static void Frame(){if(Session==null){frameStart=0;return;}long now=Now;double interval=frameStart==0?0:Ms(now-frameStart);if(frameStart!=0){frameSamples++;frameTotal+=interval;framePeak=Math.Max(framePeak,interval);}if(interval>=100)Emit("FRAME_SLOW",new{frame=Time.frameCount,intervalMs=interval});frameStart=now;if(now<nextSummary)return;nextSummary=now+Stopwatch.Frequency;Emit("FRAME_SUMMARY",new{frame=Time.frameCount,samples=frameSamples,avgMs=frameSamples==0?0:frameTotal/frameSamples,maxMs=framePeak,managedBytes=GC.GetTotalMemory(false),gc0=GC.CollectionCount(0),gc1=GC.CollectionCount(1),gc2=GC.CollectionCount(2)});frameSamples=0;frameTotal=framePeak=0;lock(Gate){if(totalsSession!=Session){Totals.Clear();totalsSession=Session;}foreach(var p in Totals)Emit("SYSTEM_SUMMARY",new{system=p.Key,calls=p.Value.Calls,enabledCalls=p.Value.Enabled,inclusiveMs=p.Value.Total,maxMs=p.Value.Peak});Totals.Clear();}Emit("HEALTH",new{dropped,errors,queued,lastError});}
 internal static object Scalar(object value){if(value==null)return null;if(value is string t)return t.Length<=512?t:t.Substring(0,512)+" [TRUNCATED]";return value.GetType().IsPrimitive||value.GetType().IsEnum?value:(object)value.GetType().FullName;}
 private static void Loop(){StreamWriter writer=null;string current=null;long bytes=0,total=0;int part=0;long statusAt=0;try{while(!quit||!Queue.IsEmpty){bool wrote=false;int budget=2000;while(budget-->0&&Queue.TryDequeue(out var row)){Interlocked.Decrement(ref queued);try{if(row.Session!=current){writer?.Dispose();writer=null;current=row.Session;part=0;bytes=total=0;}string text=JsonConvert.SerializeObject(row.Payload);long size=System.Text.Encoding.UTF8.GetByteCount(text)+2;if(total+size>512L*1024*1024){Interlocked.Increment(ref dropped);lastError="Session size limit reached; toggle detailed logging off/on to start a new session.";continue;}if(writer==null||bytes+size>64L*1024*1024){writer?.Dispose();Directory.CreateDirectory(DirectoryPath);writer=new StreamWriter(new FileStream(Path.Combine(DirectoryPath,current+"-"+(part++).ToString("D2")+".jsonl"),FileMode.CreateNew,FileAccess.Write,FileShare.ReadWrite));bytes=0;}writer.WriteLine(text);wrote=true;bytes+=size;total+=size;}catch(Exception e){lastError=e.Message;RecordFailure();try{writer?.Dispose();}catch{}writer=null;}}try{if(wrote)writer?.Flush();if(current!=null&&(Session!=null||wrote)&&(Now>=statusAt||Session==null)){statusAt=Now+Stopwatch.Frequency;File.WriteAllText(Path.Combine(DirectoryPath,"detail-status.json"),JsonConvert.SerializeObject(new{utc=DateTime.UtcNow.ToString("O"),session=Session,lastSession=current,bytes=total,dropped,errors,queued,lastError,coverage=Coverage.ToArray()}));}}catch(Exception e){lastError=e.Message;RecordFailure();}if(Session==null&&Queue.IsEmpty){writer?.Dispose();writer=null;}if(!quit)Thread.Sleep(100);}}finally{try{writer?.Dispose();}catch{}}}
}
}