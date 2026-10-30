using System;using System.Collections.Generic;
namespace TrafficLightIntoKorea {
// Runtime display scheduling only. Selection and persistence live independently.
internal sealed class DeferredVisualQueue<T> {
 private readonly HashSet<T> _waiting=new HashSet<T>();private readonly SignQueue<T> _round=new SignQueue<T>();
 internal int Count=>_waiting.Count;
 internal bool Contains(T value)=>_waiting.Contains(value);
 internal void Add(T value){if(_waiting.Add(value))_round.Enqueue(value);}
 internal bool Remove(T value)=>_waiting.Remove(value);
 internal void Clear(){_waiting.Clear();_round.Clear();}
 internal void Tick(Func<T,bool> live,Func<T,bool> nearby,Action<T> activate,int budget=32){
  int count=Math.Min(Math.Max(0,budget),_round.Count);
  for(int i=0;i<count;i++){var value=_round.Dequeue();if(!_waiting.Contains(value))continue;
   if(!live(value)){_waiting.Remove(value);continue;}
   if(nearby(value)){_waiting.Remove(value);activate(value);return;}
   _round.Enqueue(value);
  }
 }
}
internal static class BulkVisualRules {
 internal static int Band(bool cameraReady,float squaredDistance)=>!cameraReady||float.IsNaN(squaredDistance)?0:squaredDistance<=600*600?0:squaredDistance<=1200*1200?1:2;
 internal static bool Nearby(bool cameraReady,float squaredDistance)=>Band(cameraReady,squaredDistance)<2;
}
}
