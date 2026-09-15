namespace TrafficLightIntoKorea {
internal struct KoreanSignalTransition {
 private bool _green,_amber;private uint _started;
 internal int Update(int native,uint frame,uint duration){
  if(native==0||(native&8)!=0){this=default;return native;}
  // Preparation (red + yellow) is not permission to enter the junction.
  if((native&4)!=0){_green=true;_amber=false;return 4;}
  if(_green){_green=false;_amber=true;_started=frame;}
  if(_amber){if(unchecked(frame-_started)<duration)return 2;_amber=false;}
  return 1;
 }
 internal static bool Lamp(int role,int straight,int left){switch(role){case 0:return (straight&1)!=0;case 1:return (straight&2)!=0||((left&2)!=0&&(straight&4)==0);case 2:return (straight&4)!=0;case 3:return (left&4)!=0;default:return false;}}
}
}
