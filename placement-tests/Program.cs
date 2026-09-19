using System;
using System.IO;
using Newtonsoft.Json;
using TrafficLightIntoKorea;
static class Program {
 static void Check(bool v,string message){if(!v)throw new Exception(message);}
 static void Main(){DormantTests();
 var old=JsonConvert.DeserializeObject<SignalSidecar>("{\"schema\":1,\"signals\":[{\"key\":\"old\",\"farOffset\":2,\"farRotation\":30}]}");
 var o=old.signals[0];Check(o.nearOffset==0&&o.nearRotation==0&&o.nearLateral==0,"Old saves need zero near adjustments");Check(o.farOffset==2&&o.farRotation==30,"Preserve old far placement");
 o.nearOffset=-2;o.nearLateral=1.5f;o.nearRotation=-20;o.nearProps=new[]{"test"};
 var restored=JsonConvert.DeserializeObject<SignalSidecar>(JsonConvert.SerializeObject(old)).signals[0];
 Check(restored.nearOffset==-2&&restored.nearLateral==1.5f&&restored.nearRotation==-20&&restored.farOffset==2&&restored.farRotation==30&&restored.nearProps[0]=="test","Independent near/far roundtrip");
 Check(FarOffsetRules.Clamp(99)==3&&FarOffsetRules.Clamp(-99)==-3&&FarOffsetRules.Clamp(float.NaN)==0&&FarOffsetRules.Rotation(99)==45&&FarOffsetRules.Rotation(float.PositiveInfinity)==0,"Bounds and corrupt values");
 var suspended=JsonConvert.DeserializeObject<SignalSidecar>(JsonConvert.SerializeObject(new SignalSidecar{nodes=Array.Empty<string>(),retainedNodes=new[]{"N2:1,2,3"},signals=new[]{o}})); Check(suspended.nodes.Length==0&&suspended.retainedNodes.Length==1&&suspended.signals[0].nearRotation==-20&&suspended.signals[0].farOffset==2,"Default appearance preserves inactive junction history and edits"); StorageTests();Check(FarOffsetRules.FarRotation(120)==90&&FarOffsetRules.FarRotation(-120)==-90&&FarOffsetRules.Rotation(90)==45,"Independent rotation limits");Check(FarOffsetRules.CanPlace(60,0,0)&&!FarOffsetRules.CanPlace(60.01f,0,0)&&!FarOffsetRules.CanPlace(45,0,45)&&!FarOffsetRules.CanPlace(0,13,0)&&!FarOffsetRules.CanPlace(float.NaN,0,0),"Manual placement boundary");o.manualPosition=new[]{12f,3f,40f};o.manualRotation=new[]{0f,0f,0f,1f};o.farRotation=90;var manual=JsonConvert.DeserializeObject<SignalChoice>(JsonConvert.SerializeObject(o));Check(manual.manualPosition[2]==40&&manual.manualRotation[3]==1&&manual.farRotation==90,"Manual placement roundtrip");
 Console.WriteLine("PASS: legacy save defaults, independent near/far JSON roundtrip, bounds and non-finite values.");
 }
 static void DormantTests(){
  var state=new RestoreNodeState();state.Load(new[]{"N2:live","missing"},null);state.CompletePass(0);state.CompletePass(0);Check(state.Pending.Count==2,"Empty world is not evidence of missing junctions");
  state.Pending.Remove("N2:live");state.CompletePass(10);Check(state.Pending.Contains("missing")&&state.Dormant.Count==0,"First pass retains unresolved for late-load retry");
  var early=state.ActiveForSave(new[]{"N2:live"});Check(Array.IndexOf(early,"missing")>=0,"Save before completed retries preserves pending restore");
  state.CompletePass(10);Check(state.Pending.Count==0&&state.Dormant.Contains("missing"),"Second completed non-empty pass parks unresolved");
  var active=state.ActiveForSave(new[]{"N2:live"});var snapshot=new SignalSidecar{nodes=active,dormantNodes=state.DormantForSave(active),signals=new[]{new SignalChoice{key="old-settings",farRotation=90,manualPosition=new[]{1f,2f,3f}}}};
  var data=SidecarStorage.Parse(JsonConvert.SerializeObject(snapshot));var reloaded=new RestoreNodeState();reloaded.Load(data.nodes,data.dormantNodes);
  Check(reloaded.Pending.SetEquals(new[]{"N2:live"})&&reloaded.Dormant.SetEquals(new[]{"missing"}),"Reload never queues dormant identifiers");Check(data.signals[0].farRotation==90&&data.signals[0].manualPosition[2]==3,"Dormancy preserves individual choices");
  var reactivated=state.ActiveForSave(new[]{"missing"});Check(state.DormantForSave(reactivated).Length==0,"Explicit reapplication wins over dormancy");
  reloaded.Load(new[]{"new-city"},null);Check(reloaded.Dormant.Count==0&&!reloaded.Pending.Contains("N2:live"),"Different city resets archive state");
  Console.WriteLine("PASS: 8 dormant-node save/reload, early-save, empty-world, reapply and city-isolation checks.");
 }
 static void StorageTests(){
 var dir=Path.Combine(Path.GetTempPath(),"TLIK-tests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
 try{
 var file=Path.Combine(dir,"city-a.json");var b=Path.Combine(dir,"city-b.json");
 var first=JsonConvert.SerializeObject(new SignalSidecar{nodes=new[]{"N2:1,2,3"},signals=new[]{new SignalChoice{key="signal",nearAsset="near",farAsset="far",nearRotation=12,farRotation=-20}}});
 var second=JsonConvert.SerializeObject(new SignalSidecar{nodes=new[]{"N2:4,5,6"}});
 Check(SidecarStorage.Read(file,out var recovered)==null&&!recovered,"Missing sidecar");
 SidecarStorage.Write(file,first);SidecarStorage.Write(file,second);
 Check(SidecarStorage.Read(file,out recovered).nodes[0]=="N2:4,5,6"&&!recovered,"Primary takes precedence");
 Check(SidecarStorage.Parse(File.ReadAllText(file+".bak")).signals[0].nearRotation==12,"Previous snapshot backup");
 File.WriteAllText(file,"broken");var data=SidecarStorage.Read(file,out recovered);
 Check(recovered&&data.signals[0].nearAsset=="near"&&data.signals[0].farRotation==-20,"Corrupt primary backup recovery and placement preservation");
 SidecarStorage.Write(file,second);Check(File.ReadAllText(file+".bak")==first,"Recovery write preserves healthy backup");
 File.Delete(file);Check(SidecarStorage.Read(file,out recovered).nodes.Length==1&&recovered,"Missing primary backup recovery");
 File.WriteAllText(file,"{\"schema\":2,\"nodes\":[]}");bool failed=false;
 try{SidecarStorage.Read(file,out recovered);}catch(NotSupportedException){failed=true;}Check(failed,"Future schema must not fall back to stale backup");
 File.WriteAllText(file,"broken");File.WriteAllText(file+".bak","broken too");failed=false;
 try{SidecarStorage.Read(file,out recovered);}catch(InvalidDataException){failed=true;}Check(failed,"Both corrupt fail closed");
 SidecarStorage.Write(b,first);failed=false;try{SidecarStorage.Write(b,"null");}catch{failed=true;}
 Check(failed&&File.ReadAllText(b)==first&&!File.Exists(b+".tmp"),"Invalid snapshot must not touch existing data");
 Check(File.ReadAllText(file)=="broken","Different save identities isolated");
 Console.WriteLine("PASS: sidecar backup, recovery, future schema, corruption, write protection, city isolation.");
 }finally{Directory.Delete(dir,true);}
 }

}

