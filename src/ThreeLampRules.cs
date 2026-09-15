using System;
namespace TrafficLightIntoKorea {
internal static class ThreeLampRules {
 internal const string DefaultVehicle="CSKR3w2lLeftTrafficLightCar01";
 internal static bool Compatible(int type,bool crossing)=>(type&3)!=0&&((type&12)!=0)==crossing;
 internal static bool TurnOnly(int roads,int validLanes,int movements)=>roads==3&&validLanes>0&&(movements&1)==0&&(movements&6)!=0;
 internal static bool StraightOnly(int movements)=>(movements&1)!=0&&(movements&2)==0;
 internal static bool PreferFour(int roads)=>roads>=4;
 internal static int Score(string name,string preferred,string original,bool leftArrow=true){
 if(!name.StartsWith("CSKR",StringComparison.OrdinalIgnoreCase)||name.IndexOf("Flashing",StringComparison.OrdinalIgnoreCase)>=0)return -1;
 bool two=name.IndexOf("3w2l",StringComparison.OrdinalIgnoreCase)>=0,one=name.IndexOf("3w1l",StringComparison.OrdinalIgnoreCase)>=0;if(!two&&!one)return -1;
 // Left after TrafficLight describes placement; Left before it denotes the arrow head.
 if((name.IndexOf("LeftTrafficLight",StringComparison.OrdinalIgnoreCase)>=0)!=leftArrow)return -1;
 if(string.Equals(name,leftArrow?DefaultVehicle:"CSKR3w2lTrafficLightCar01",StringComparison.Ordinal))return 2000;
 if(string.Equals(name,preferred,StringComparison.Ordinal))return 1000;
 if(!string.IsNullOrEmpty(preferred)&&(name==preferred.Replace("4w2l","3w2l")||name==preferred.Replace("4w1l","3w1l")))return 900;
 int score=two?100:50;int at=original.IndexOf("TrafficLight",StringComparison.Ordinal);if(at>=0&&name.EndsWith(original.Substring(at),StringComparison.Ordinal))score+=400;
 if(name.IndexOf("Short",StringComparison.OrdinalIgnoreCase)>=0)score-=25;
 return score;
 }
}
}
