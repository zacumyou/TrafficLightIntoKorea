using System;
namespace TrafficLightIntoKorea {
[Serializable] public sealed class SignalChoice { public string key;public string nearRoadName="",farRoadName="";public string nearAsset="",farAsset="";public int farMode=-1;public bool nearHidden;public float[] manualPosition,manualRotation;public float farOffset,farRotation,farLateral;public float nearOffset,nearRotation,nearLateral;public string[] nearSigns,farSigns;public string[] nearProps,farProps; }
[Serializable] public sealed class SignalSidecar { public int schema=1;public string saveName="";public string[] dormantNodes=new string[0];public string[] retainedNodes=new string[0];public string[] nodes=new string[0];public SignalChoice[] signals=new SignalChoice[0]; }
}
