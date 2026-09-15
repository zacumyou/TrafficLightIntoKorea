using System;
using TrafficLightIntoKorea;
class Program {
 static int count;
 static void Check(bool value,string name){count++;if(!value)throw new Exception(name);}
 static int State(bool go=false,bool yield=false,bool stop=false,bool allow=false,bool beginning=false,int native=4)=>MovementSignalState.Resolve(go,yield,stop,allow,beginning,native);
 static void Main(){
  var t=new KoreanSignalTransition();
  Check(t.Update(3,0,60)==1,"Startup red+yellow must not become green");
  Check(State(yield:true,allow:true,beginning:true,native:3)==1,"TLM Beginning Yield is not permissive left");
  Check(State(native:4)==1,"Aggregate mask green cannot overrule stopped movement");
  Check(State(go:true,native:1)==4,"Live Go is authoritative even when native mask is stale");
  Check(State(stop:true)==2,"SafeStop is clearance amber");
  Check(State(yield:true)==1,"Yield option off");
  Check(State(yield:true,allow:true)==4,"Explicit yield option outside preparation");
  Check(State(native:0)==0,"Disabled signal remains unlit");
  Check(t.Update(4,10,60)==4,"Actual Go turns green");
  Check(t.Update(1,11,60)==2,"Go end starts amber");
  Check(t.Update(1,70,60)==2,"Amber follows simulation frames");
  Check(t.Update(1,71,60)==1,"Amber expires");
  Check(t.Update(0,72,60)==0,"Disabled clears transition");
  Check(!LeftThreeLampRules.Lamp(0,0)&&!LeftThreeLampRules.Lamp(2,0),"Disabled left3 unlit");
  Check(LeftThreeLampRules.Lamp(0,4)&&LeftThreeLampRules.Lamp(2,4),"Left3 retains red plus arrow");
  // Logged junction: straight/left masks 8/8, 16/16, 3/1, 6/4; right turns 31.
  // Exercise all 5 phases: a right-only Go must never light either head.
  foreach(var masks in new[]{(8,8),(16,16),(3,1),(6,4)})for(int phase=0;phase<5;phase++){
   int bit=1<<phase;bool straight=(masks.Item1&bit)!=0,left=(masks.Item2&bit)!=0;
   int sd=State(go:straight),ld=State(go:left);
   Check(KoreanSignalTransition.Lamp(2,sd,ld)==straight,"Logged straight phase "+phase);
   Check(KoreanSignalTransition.Lamp(3,sd,ld)==left,"Logged left phase "+phase);
  }
  // Invariant across prior transition histories and all native display bits.
  for(int native=0;native<16;native++)foreach(bool beginning in new[]{false,true}){
   var transition=new KoreanSignalTransition();transition.Update(4,0,60);
   int live=State(beginning:beginning,native:native);
   Check((transition.Update(live,1,60)&4)==0,"No green without lane permission "+native);
  }
  int[] slots={0,1,2,6};
  Check(DirectionalSlotRules.SuppressExtraGreen(7,true,slots,4),"Logged B: extra ID55 green is suppressed");
  Check(!DirectionalSlotRules.SuppressExtraGreen(6,true,slots,4),"ID50 follows left permission");
  Check(!DirectionalSlotRules.SuppressExtraGreen(2,true,slots,4),"ID15 follows straight permission");
  Check(!DirectionalSlotRules.SuppressExtraGreen(7,false,slots,4),"Pedestrian and accessory lights preserved");
  for(int slot=0;slot<8;slot++){
   bool nativeGreen=slot==2||slot==6||slot==7;
   bool stoppedIntensity=nativeGreen && slot!=2 && slot!=6;
   if(DirectionalSlotRules.SuppressExtraGreen(slot,nativeGreen,slots,4))stoppedIntensity=false;
   Check(!stoppedIntensity,"Stop clears every vehicle green slot including native extra "+slot);
  }
  Console.WriteLine("PASS: "+count+" signal permission, TLM phase, transition and left3 checks.");
 }
}
