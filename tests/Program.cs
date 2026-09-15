using System;
using TrafficLightIntoKorea;
static class Program {
 static void Check(bool value,string label){if(!value)throw new Exception(label);}
 static void Main(){
  Check(RenderRecoveryRules.ProtectFar(true,null),"Automatic far visibility protected");
  Check(RenderRecoveryRules.ProtectFar(false,1),"Explicit far placement protected when automatic disabled");
  Check(!RenderRecoveryRules.ProtectFar(true,0),"Explicit far deletion is respected");
  Check(!RenderRecoveryRules.ProtectFar(false,null),"Global far off is respected");
  Check(!RenderRecoveryRules.KeepCustom(false,true,0),"Never-ready assets must retain native fallback");
  Check(RenderRecoveryRules.KeepCustom(true,true,.5f),"Highlight batch rebuild must not immediately show native");
  Check(!RenderRecoveryRules.KeepCustom(true,true,1.1f),"Persistent visible failures must fall back");
  Check(RenderRecoveryRules.KeepCustom(true,false,100),"Offscreen culling must not discard successful display");
  Check(RenderRecoveryRules.RetryDelay(0)==2,"Initial retry");
  Check(RenderRecoveryRules.RetryDelay(3)==10&&RenderRecoveryRules.RetryDelay(100)==10,"Recovery must continue beyond three failures at bounded frequency");
  Console.WriteLine("PASS: initial fallback, highlight grace, persistent failure, offscreen culling, sustained recovery.");
 }
}
