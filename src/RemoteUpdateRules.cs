namespace TrafficLightIntoKorea {
internal static class RemoteUpdateRules {
 internal const float Radius=1000f;
 internal static bool Near(bool cameraReady,float distanceSquared)=>!cameraReady||float.IsNaN(distanceSquared)||distanceSquared<=Radius*Radius;
 internal static bool Due(bool near,float now,ref float next,int stagger){
  if(near){next=0;return true;}
  if(now<next)return false;
  next=now+1f+(stagger&15)/30f;return true;
 }
}
}
