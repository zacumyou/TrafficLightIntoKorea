using System;
namespace TrafficLightIntoKorea {
// No Unity dependency: these decisions are tested independently of rendering.
internal static class SignRules {
 internal const string Prefix="CSKRTrafficLightSign";
 internal static readonly int[] Speeds={30,40,50,60,70,80,100};
 internal static readonly string[] Names={Prefix+"Speed30",Prefix+"Speed40",Prefix+"Speed50",Prefix+"Speed60",Prefix+"Speed70",Prefix+"Speed80",Prefix+"Speed100",Prefix+"StraightLeft",Prefix+"StraightThenStraightLeft",Prefix+"StraightLeftThenStraight",Prefix+"BanLeft",Prefix+"BanRight",Prefix+"BanStraight",Prefix+"BanUTurn"};
 internal static int Kind(string name){if(Array.IndexOf(Names,name)<0)return 0;return name.StartsWith(Prefix+"Speed",StringComparison.Ordinal)?1:name.StartsWith(Prefix+"Ban",StringComparison.Ordinal)?3:2;}
 internal static string Speed(float metresPerSecond){double kmh=metresPerSecond*1.8;foreach(int speed in Speeds)if(Math.Abs(kmh-speed)<.55)return Prefix+"Speed"+speed;return null;}
 // Custom phase masks use UI index bits. The last-to-first cycle boundary is
 // not another phase inside the configured sequence.
 internal static int Ordered(int straight,int left,int count){if(count<1||count>16)return 0;int first=0,last=0,changes=0;for(int group=1;group<=count;group++){int phase=Phase(straight,left,group);if(phase==0)continue;if(phase==2)return 0;if(first==0)first=phase;if(last!=0&&last!=phase)changes++;last=phase;}if(changes!=1)return 0;return first==1&&last==3?1:first==3&&last==1?2:0;}
 internal static int Phase(int straight,int left,int group){if(group<1||group>16)return 0;int bit=1<<(group-1);return ((straight&bit)!=0?1:0)|((left&bit)!=0?2:0);}
 internal static string Mode(int straight,int left,int observed){if(straight==0||left==0)return null;if(straight==left)return Names[7];if((left&~straight)!=0)return null;return observed==1?Names[8]:observed==2?Names[9]:null;}
 internal static int Observe(int previous,int straight,int left,int current,int next){int a=Phase(straight,left,current),b=Phase(straight,left,next);if(a==1&&b==3)return previous|1;if(a==3&&b==1)return previous|2;return previous;}
}
}
