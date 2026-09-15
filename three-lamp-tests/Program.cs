using System;
using TrafficLightIntoKorea;
static class Program {
 static int count;
 static void Check(bool condition,string message){if(!condition)throw new Exception(message);count++;}
 static void Main(){
 Check(ThreeLampRules.TurnOnly(3,2,6),"T stem left and right");
 Check(ThreeLampRules.TurnOnly(3,1,2),"Left only");
 Check(ThreeLampRules.TurnOnly(3,1,4),"Right only");
 Check(!ThreeLampRules.TurnOnly(3,3,7),"Through road stays unchanged");
 Check(!ThreeLampRules.TurnOnly(4,2,6),"Four way stays unchanged");
 Check(!ThreeLampRules.TurnOnly(3,0,6),"No usable lanes");
 Check(!ThreeLampRules.TurnOnly(3,1,0),"Unknown movement");
 string primary=ThreeLampRules.DefaultVehicle;
 Check(ThreeLampRules.Score(primary,"","")==2000,"Requested default");
 Check(ThreeLampRules.Score("CSKR3w2lTrafficLightCarLeft01","","")==-1,"Left after TrafficLight is not arrow head");
 Check(ThreeLampRules.Score("CSKR3w2lTrafficLightCar01","","")==-1,"Round green rejected");
 Check(ThreeLampRules.Score("CSKR4w2lLeftTrafficLightCar01","","")==-1,"Four lamps rejected");
 Check(ThreeLampRules.Score("CSKR3w1lLeftTrafficLightCar01","","")>=0,"Single head fallback");
 Check(ThreeLampRules.Score("CSKR3w2lLeftTrafficLightCarCrosswalk01","","")>=0,"Combined arrow candidate");
 Check(ThreeLampRules.Compatible(1,false)&&!ThreeLampRules.Compatible(1,true),"Vehicle type isolation");
 Check(ThreeLampRules.Compatible(5,true)&&!ThreeLampRules.Compatible(5,false),"Pedestrian capability preserved");
 Check(ThreeLampRules.StraightOnly(1),"Straight plus ignored right uses round green");
 Check(ThreeLampRules.StraightOnly(5),"Explicit straight and right");
 Check(!ThreeLampRules.StraightOnly(3),"Straight and left stays four lamps");
 Check(!ThreeLampRules.StraightOnly(2),"Left only must not use round green");
 Check(!ThreeLampRules.StraightOnly(0),"Unknown must not select round green");
 Check(ThreeLampRules.Score("CSKR3w2lTrafficLightCar01","","",false)==2000,"Round default");
 Check(ThreeLampRules.Score(primary,"","",false)==-1,"Arrow excluded from round candidates");
 Check(ThreeLampRules.Score("CSKR3w2lTrafficLightCarLeft01","","",false)>=0,"Left mounting is allowed for round head");
 Check(ThreeLampRules.Score("CSKR3w2lTrafficLightCarCrosswalk01","","",false)>=0,"Round combined candidate");
 Check(LeftThreeLampRules.Asset("CSKR3w2lLeftTrafficLightCar01"),"Arrow asset detection"); Check(!LeftThreeLampRules.Asset("CSKR3w2lTrafficLightCarLeft01"),"Ordinary three lamp excluded"); Check(LeftThreeLampRules.Lamp(0,4)&&LeftThreeLampRules.Lamp(2,4)&&!LeftThreeLampRules.Lamp(1,4),"Red plus left green"); Check(LeftThreeLampRules.Lamp(0,1)&&!LeftThreeLampRules.Lamp(2,1),"Stop red only"); Check(LeftThreeLampRules.Lamp(0,2)&&LeftThreeLampRules.Lamp(1,2)&&!LeftThreeLampRules.Lamp(2,2),"Left clearance red plus amber"); Check(ThreeLampRules.PreferFour(4),"Four-way must prefer four lamps even without straight movement"); Check(ThreeLampRules.PreferFour(5),"Five-way prefers four lamps"); Check(!ThreeLampRules.PreferFour(3),"T-junction retains movement selection"); Check(!ThreeLampRules.PreferFour(2),"Two-road connection not forced"); Console.WriteLine("PASS: "+count+" approach and three-lamp asset selection checks.");
 }
}
