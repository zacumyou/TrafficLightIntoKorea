using System;
using System.Collections.Generic;
using System.Linq;
using TrafficLightIntoKorea;
class Program {
 static int checks;
 static void Check(bool ok,string name){checks++;if(!ok)throw new Exception(name);}
 static void Finish(IncrementalNodeRestore<int> scan){for(int i=0;!scan.Done&&i<100000;i++)scan.Step(32,1000);Check(scan.Done,"finite completion");}
 static void Main(){
  int keys=0,legacy=0,applied=0;var pending=new HashSet<string>{"N2:1","N2:2"};
  var scan=new IncrementalNodeRestore<int>(Enumerable.Range(0,100).ToArray(),pending,n=>{keys++;return "N2:"+n;},n=>{legacy++;return "old"+n;},(n,k,l)=>{applied++;return true;});
  scan.Step(3,1000);Check(keys==3&&applied==0,"index phase budget; no early selection");
  scan.Step(32,0);Check(keys==4,"elapsed budget yields between nodes");
  while(keys<100)scan.Step(1,1000);Check(applied==0,"all nodes checked for ambiguity first");
  scan.Step(32,1000);Check(applied==1&&pending.Count==1,"one application per step");Finish(scan);Check(applied==2&&legacy==0,"modern saves never invoke legacy SHA path");
  pending=new HashSet<string>{"N2:duplicate"};applied=0;
  scan=new IncrementalNodeRestore<int>(Enumerable.Range(0,100).ToArray(),pending,n=>n==0||n==99?"N2:duplicate":"N2:"+n,n=>"legacy",(n,k,l)=>{applied++;return true;});Finish(scan);Check(applied==0&&pending.Count==1,"late duplicate remains unresolved");
  pending=new HashSet<string>{"legacy-1","N2:2","missing-legacy"};applied=0;legacy=0;
  scan=new IncrementalNodeRestore<int>(new[]{1,2},pending,n=>"N2:"+n,n=>{legacy++;return "legacy-"+n;},(n,k,l)=>{applied++;return true;});Finish(scan);Check(applied==2&&legacy==2&&pending.SetEquals(new[]{"missing-legacy"}),"legacy and modern compatibility; unresolved retained");
  pending=new HashSet<string>{"N2:1"};applied=0;keys=0;
  scan=new IncrementalNodeRestore<int>(new[]{1},pending,n=>++keys==1?"N2:1":null,n=>"old",(n,k,l)=>{applied++;return true;});Finish(scan);Check(applied==0&&pending.Count==1,"deleted node excluded after index");
  pending=new HashSet<string>{"N2:1"};keys=0;
  scan=new IncrementalNodeRestore<int>(new[]{1},pending,n=>++keys==1?"N2:1":"N2:moved",n=>"old",(n,k,l)=>{applied++;return true;});Finish(scan);Check(applied==0&&pending.Count==1,"moved node not attached to stale key");
  pending=new HashSet<string>{"N2:1"};scan=new IncrementalNodeRestore<int>(new[]{1},pending,n=>"N2:1",n=>"old",(n,k,l)=>false);Finish(scan);Check(pending.Count==1,"failed application preserved for retry");
  scan=new IncrementalNodeRestore<int>(Array.Empty<int>(),pending,n=>"N2:1",n=>"old",(n,k,l)=>true);Finish(scan);Check(pending.Count==1,"empty world retained");
  pending=new HashSet<string>{"N2:1"};scan=new IncrementalNodeRestore<int>(new[]{1},pending,n=>"N2:1",n=>"old",(n,k,l)=>{applied++;return true;});scan.Step(1,1000);pending.Clear();Finish(scan);Check(applied==0,"user cancellation during index");
  Check(IncrementalNodeRestore<int>.RetryDelay(0)==5&&IncrementalNodeRestore<int>.RetryDelay(1)==5&&IncrementalNodeRestore<int>.RetryDelay(2)==10&&IncrementalNodeRestore<int>.RetryDelay(3)==20&&IncrementalNodeRestore<int>.RetryDelay(25)==30,"bounded retry backoff");
  pending=new HashSet<string>(Enumerable.Range(50000,17).Select(n=>"N2:"+n));keys=legacy=0;
  scan=new IncrementalNodeRestore<int>(Enumerable.Range(0,50000).ToArray(),pending,n=>{keys++;return "N2:"+n;},n=>{legacy++;return "old"+n;},(n,k,l)=>true);
  int steps=0,maxKeys=0;while(!scan.Done){int before=keys;scan.Step(32,1000);maxKeys=Math.Max(maxKeys,keys-before);steps++;}
  Check(keys==50000&&maxKeys<=32&&legacy==0&&pending.Count==17,"large city unmatched retry is bounded and hash-free");
  Console.WriteLine($"50,000-node modern city + 17 missing keys: {steps} steps, maximum {maxKeys} keys/step, {legacy} legacy hashes; old indexing = 50,000 keys in one frame plus 50,000 legacy hashes/pass.");
  pending=new HashSet<string>(Enumerable.Range(0,6).Select(n=>"N2:"+n).Concat(Enumerable.Range(0,17).Select(n=>"unmatched-legacy-"+n)));keys=legacy=applied=0;
  scan=new IncrementalNodeRestore<int>(Enumerable.Range(0,5000).ToArray(),pending,n=>{keys++;return "N2:"+n;},n=>{legacy++;return "legacy-"+n;},(n,k,l)=>{applied++;return true;});
  int maxLegacy=0,maxApplied=0;while(!scan.Done){int oldLegacy=legacy,oldApplied=applied;scan.Step(32,1000);maxLegacy=Math.Max(maxLegacy,legacy-oldLegacy);maxApplied=Math.Max(maxApplied,applied-oldApplied);}
  Check(applied==6&&pending.Count==17&&pending.All(k=>k.StartsWith("unmatched-legacy"))&&maxLegacy<=32&&maxApplied==1,"capture-shaped six modern plus seventeen missing legacy keys preserved and bounded");
  Console.WriteLine($"Capture-shaped legacy fixture: restored={applied}, unresolved={pending.Count}, maximum legacy calls/step={maxLegacy}, successful junctions/step={maxApplied}.");
  pending=new HashSet<string>{"N2:999","legacy-missing"};keys=legacy=applied=0;
  scan=new IncrementalNodeRestore<int>(Enumerable.Range(0,1000).ToArray(),pending,n=>{keys++;return "N2:"+n;},n=>{legacy++;return "legacy-"+n;},(n,k,l)=>{applied++;return true;});
  while(keys<1000)scan.Step(1,1000);scan.Step(32,1000);Check(applied==1&&legacy==1&&pending.Contains("legacy-missing"),"modern junction at end restored before city-wide legacy matches");
  Console.WriteLine($"PASS: {checks} restore checks using production scanner. Not an in-game frame benchmark.");
 }
}
