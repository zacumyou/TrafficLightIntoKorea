namespace TrafficLightIntoKorea {
internal static class DirectionalSlotRules {
 // Extra vehicle-green slots must not inherit the native aggregate green.
 // Pedestrian lamps and accessory lights are outside this rule.
 internal static bool SuppressExtraGreen(int slot,bool vehicleGreen,int[] mapped,int count){
  if(!vehicleGreen)return false;
  for(int role=0;role<count;role++)if(mapped[role]==slot)return false;
  return true;
 }
}
}
