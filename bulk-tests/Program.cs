using System;
using TrafficLightIntoKorea;
using Newtonsoft.Json;
class Program {
 static int checks;
 static void Check(bool value,string name){if(!value)throw new Exception(name);checks++;}
 static void Main(){
  foreach(bool global in new[]{false,true})foreach(bool bulk in new[]{false,true})foreach(int mode in new[]{-1,0,1})
   Check(BulkFarPolicy.Allows(global,bulk,mode)==(mode==1||(mode==-1&&global&&!bulk)),"far policy truth table");
  var policy=new BulkFarPolicy();policy.Set("N2:1",true);policy.Set("N2:2",true);
  var snapshot=new SignalSidecar{nodes=new[]{"N2:1"},nearOnlyNodes=policy.Save(new[]{"N2:1","N2:3"}),signals=new[]{new SignalChoice{farMode=1,manualPosition=new float[]{1,2,3},farRotation=90}}};
  var restored=JsonConvert.DeserializeObject<SignalSidecar>(JsonConvert.SerializeObject(snapshot));policy.Load(restored.nearOnlyNodes);
  Check(policy.NearOnly("N2:1")&&!policy.NearOnly("N2:2"),"save active policy and prune missing selection");
  Check(restored.signals[0].manualPosition[2]==3&&restored.signals[0].farMode==1&&restored.signals[0].farRotation==90,"manual placement retained");
  policy.Set("N2:1",false);Check(!policy.NearOnly("N2:1"),"individual opt-in resumes automatic far");
  policy.Load(new[]{"N2:1",null,"N2:1",""});Check(policy.Save(new[]{"N2:1"}).Length==1,"duplicates and null ignored");
  policy.Clear();Check(!policy.NearOnly("N2:1"),"restore all clears policy");
  policy.Load(JsonConvert.DeserializeObject<SignalSidecar>("{\"schema\":1,\"nodes\":[\"N2:1\"]}").nearOnlyNodes);
  Check(!policy.NearOnly("N2:1"),"old saves retain old behavior");
  Check(DisplayMaintenance.Due(0,10,false,false),"compatibility fallback checks each frame");
  Check(DisplayMaintenance.Due(0,10,true,true),"visibility change immediate");
  Check(DisplayMaintenance.Next(1,false,3)==1,"unready display checks each frame");
  Check(!DisplayMaintenance.Due(1,2,true,false)&&DisplayMaintenance.Due(2,2,true,false),"stable maintenance bounded");
  int work=0;var next=new float[424];
  for(int frame=0;frame<600;frame++)for(int i=0;i<next.Length;i++){float now=frame/60f;if(DisplayMaintenance.Due(now,next[i],true,false)){work++;next[i]=DisplayMaintenance.Next(now,true,i);}}
  Check(DisplayMaintenance.Next(0,true,0)==5&&DisplayMaintenance.Next(0,true,15)==6,"stable audit stagger 5 to 6 seconds");
  Check(work<=424*2,"stable maintenance at least tenfold fewer inspections");
  float remoteNext=0;int remoteWork=0;
  for(int frame=0;frame<600;frame++)if(RemoteUpdateRules.Due(false,frame/60f,ref remoteNext,5))remoteWork++;
  Check(remoteWork<=10,"remote work at most once a second");
  Check(RemoteUpdateRules.Due(true,9.99f,ref remoteNext,5)&&remoteNext==0,"camera arrival bypasses remote deadline");
  Check(RemoteUpdateRules.Near(false,float.PositiveInfinity)&&RemoteUpdateRules.Near(true,1000000)&&!RemoteUpdateRules.Near(true,1000001),"camera fallback and 1000m boundary");
  remoteNext=0;Check(RemoteUpdateRules.Due(false,0,ref remoteNext,5),"explicit refresh starts immediately");
  Console.WriteLine($"Remote fixture: {remoteWork} updates / 600 frames; camera arrival immediate.");
  Check(BulkStatusText.ApplyConfirmation(true)=="세이브 파일을 따로 저장하세요\n이 설정은 초당 프레임 드랍을 야기할 수 있으며, 그로 인해 게임 튕김 현상이 나타날 수 있습니다. 백업을 필히 권장합니다","exact Korean apply confirmation and newline");
  Check(BulkStatusText.RestoreConfirmation(true)=="모든 교차로 신호등을 기본으로 돌립니다\n이 설정으로 전부 교체된 후 저장을 하면 다시 세이브 파일을 불러올 때는 그 상태가 유지됩니다. 원치 않는 설정이 되지 않길 바랍니다","exact Korean restore confirmation and newline");
  foreach(BulkStatus status in Enum.GetValues(typeof(BulkStatus))){var korean=BulkStatusText.Format(status,true,3,151);var english=BulkStatusText.Format(status,false,3,151);Check(!System.Text.RegularExpressions.Regex.IsMatch(korean,"[A-Za-z]"),"Korean-only status "+status);Check(!System.Text.RegularExpressions.Regex.IsMatch(english,"[가-힣]"),"English-only status "+status);}
  Check(BulkStatusText.Format(BulkStatus.Applying,true,3,151).EndsWith("3 / 151")&&BulkStatusText.Format(BulkStatus.Restoring,false,3,151).EndsWith("3 / 151"),"progress counts survive locale switch");
  Check(SignalNodeRules.Selectable(2,true,false),"mid-road signal crossing can be targeted");
  Check(!SignalNodeRules.Selectable(2,false,false),"ordinary two-road seam excluded");
  Check(SignalNodeRules.Selectable(2,false,true),"selected crossing remains restorable after native signal removal");
  Check(SignalNodeRules.Selectable(3,false,false)&&SignalNodeRules.Selectable(4,true,false),"existing junction targeting preserved");
  Check(!SignalNodeRules.Selectable(1,true,true)&&!SignalNodeRules.Selectable(0,true,false),"invalid endpoints excluded");
  Check(BulkStatusText.Format(BulkStatus.Applied,true,151)=="151개 적용"&&BulkStatusText.Format(BulkStatus.Restored,true,151)=="151개 복원","completion contains only operation and count");
  Check(BulkStatusText.Format(BulkStatus.NoTargets,true)=="0개 적용","empty result reports zero");
  Check(!SignalSelectionRules.CanApply(true,true,false),"native signal removal prevents pending bulk/restore from reapplying");
  Check(SignalSelectionRules.CanApply(true,true,true)&&!SignalSelectionRules.CanApply(false,true,true),"live native signal required for apply");
  Check(SignalSelectionRules.NativeRemoval(true,true,false,false),"native removal clears selected style");
  Check(!SignalSelectionRules.NativeRemoval(true,true,false,true)&&!SignalSelectionRules.NativeRemoval(true,true,true,false),"preview and still-active nodes preserve style");
  Check(SignalSelectionRules.NativeSource(true,false,false,false)&&!SignalSelectionRules.NativeSource(true,false,true,false)&&!SignalSelectionRules.NativeSource(true,false,false,true),"custom or retired displays cannot seed replacements");
  var deferred=new DeferredVisualQueue<int>();var activated=new System.Collections.Generic.List<int>();
  deferred.Add(1);deferred.Add(1);deferred.Add(2);deferred.Add(3);
  Check(deferred.Count==3,"deferred selection deduplicated");
  int examined=0;deferred.Tick(n=>true,n=>{examined++;return n==3;},activated.Add,2);
  Check(examined==2&&activated.Count==0&&deferred.Count==3,"distance scan bounded, distant data retained");
  deferred.Tick(n=>true,n=>true,activated.Add,32);
  Check(activated.Count==1&&activated[0]==3&&deferred.Count==2,"one newly nearby junction activated per tick");
  deferred.Remove(1);deferred.Tick(n=>false,n=>true,activated.Add,32);
  Check(activated.Count==1&&deferred.Count==0,"restored/deleted selections never activate");
  deferred.Add(4);deferred.Clear();deferred.Tick(n=>true,n=>true,activated.Add);
  Check(activated.Count==1,"restore all clears display queue");
  Check(BulkVisualRules.Band(true,360000)==0&&BulkVisualRules.Band(true,360001)==1&&BulkVisualRules.Band(true,1440000)==1&&BulkVisualRules.Band(true,1440001)==2,"camera distance bands");
  Check(BulkVisualRules.Nearby(false,float.PositiveInfinity)&&BulkVisualRules.Nearby(true,float.NaN),"missing camera falls back safely");
  var saved=new SignalSidecar{nodes=new[]{"N2:far"},nearOnlyNodes=new[]{"N2:far"}};
  var loaded=JsonConvert.DeserializeObject<SignalSidecar>(JsonConvert.SerializeObject(saved));policy.Load(loaded.nearOnlyNodes);
  Check(loaded.nodes[0]=="N2:far"&&policy.NearOnly(loaded.nodes[0]),"deferred selection uses existing save fields");
  Console.WriteLine($"PASS {checks} checks; stable fixture maintenance {424*600} -> {work}. Not measured game FPS.");
 }
}
