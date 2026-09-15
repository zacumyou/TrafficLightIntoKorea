using System;
using TrafficLightIntoKorea;
static class Program {
 static void Check(bool value,string name){if(!value)throw new Exception(name);}
 static void Main(){
  Check(NearPedestrianMergeRules.Compatible(1,0,4,4,.1f),"Unassigned combined head can borrow pedestrian state");
  Check(NearPedestrianMergeRules.Compatible(1,4,4,9,.75f),"Matching mask at boundary");
  Check(!NearPedestrianMergeRules.Compatible(1,2,4,1,0),"Different pedestrian groups stay separate");
  Check(!NearPedestrianMergeRules.Compatible(1,6,4,1,0),"Partial mask overlap is insufficient");
  Check(!NearPedestrianMergeRules.Compatible(0,0,4,1,0),"No vehicle head");
  Check(!NearPedestrianMergeRules.Compatible(1,0,0,1,0),"Unassigned pedestrian source");
  Check(!NearPedestrianMergeRules.Compatible(1,0,4,16.01f,0),"Outside local radius");
  Check(!NearPedestrianMergeRules.Compatible(1,0,4,1,.76f),"Different elevation");
  Check(NearPedestrianMergeRules.SameEnd(4,100,1,81),"Same crossing endpoint");
  Check(!NearPedestrianMergeRules.SameEnd(4,100,81,1),"Opposite ends must not merge");
  Check(!NearPedestrianMergeRules.SameEnd(17,100,1,81),"Combined pole too far from crossing");
  Check(!NearPedestrianMergeRules.SameEnd(4,4,1,81),"Ambiguous center stays separate");
  Check(!NearPedestrianMergeRules.SameEnd(4,100,4,4),"Ambiguous pedestrian location");
  // Measured pairs from the 2026-09-14 17:39:57 junction snapshot.
  Check(FarDestinationRules.Pedestrian(34,4,.05f,10.83627164f),"3.29185m far pedestrian must fit");
  Check(FarDestinationRules.Pedestrian(36,4,0,10.19874781f),"3.19386m far pedestrian must fit");
  Check(NearPedestrianMergeRules.SameEnd(16.822692f,291.5086f,.808181f,275.7916f,36),"First measured pair shares the same crossing endpoint");
  Check(NearPedestrianMergeRules.SameEnd(15.168687f,387.9455f,.932212f,273.0096f,36),"Second measured pair shares the same crossing endpoint");
  Check(!FarDestinationRules.Pedestrian(36,4,0,16.01f),"Outside 4m must stay separate");
  Check(FarDestinationRules.Corridor(16,32)==11,"16m to 32m transition needs wider corridor");
  Check(FarDestinationRules.Corridor(24,24)==3,"Equal-width roads preserve the narrow search");
  Check(FarDestinationRules.Corridor(0,32)==3,"Unknown road width must not widen the search");
  Check(FarDestinationRules.Corridor(16,100)==12,"Expansion must remain bounded");
  Check(!FarDestinationRules.Sidewalk(25,5,-1.5f,0,24),"Old corridor rejected the offset sidewalk");
  Check(FarDestinationRules.Sidewalk(25,5,-1.5f,0,24,11),"Width transition admits offset sidewalk");
  Check(!FarDestinationRules.Sidewalk(25,5,6,0,24,11),"Other junction arm outside longitudinal window rejected");
  Check(!NearPedestrianMergeRules.SameEnd(4,100,100,4,36),"Wider merge still rejects the opposite crossing end");
  Console.WriteLine("PASS: 26 merge, recorded-coordinate and width-transition regression checks.");
 }
}
