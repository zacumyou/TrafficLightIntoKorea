namespace TrafficLightIntoKorea {
internal static class MovementSignalState {
 // The controller's live lane permission wins over an aggregate object mask.
 internal static int Resolve(bool go,bool yield,bool safeStop,bool allowYield,bool beginning,int native){
  if(go || (allowYield && yield && !beginning))return 4;
  if(safeStop)return 2;
  if(native==0)return 0;
  if((native&8)!=0)return native&~4;
  return 1;
 }
}
}
