using System;
namespace TrafficLightIntoKorea {
internal static class LeftThreeLampRules {
 internal static bool Asset(string name)=>!string.IsNullOrEmpty(name)&&name.StartsWith("CSKR",StringComparison.OrdinalIgnoreCase)&&(name.IndexOf("3w1l",StringComparison.OrdinalIgnoreCase)>=0||name.IndexOf("3w2l",StringComparison.OrdinalIgnoreCase)>=0)&&name.IndexOf("LeftTrafficLight",StringComparison.OrdinalIgnoreCase)>=0;
 internal static bool Lamp(int role,int left)=>(role==0&&left!=0)||(role==1&&(left&2)!=0)||(role==2&&(left&4)!=0);
}
}
