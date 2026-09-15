using System;
using Unity.Collections;
using Unity.Entities;
namespace TrafficLightIntoKorea {
// Snapshot once per pass, then inspect at most 64 source entities per update.
// Late signal-group initialization is covered by finite post-load reconciliation.
internal sealed class SignalStartupScan:IDisposable {
 private NativeArray<Entity> _snapshot;private int _cursor,_pass,_version=-1;private float _next;private bool _requested=true;
 internal void Request(){_requested=true;}
 internal void Reset(){Dispose();_cursor=0;_pass=0;_version=-1;_next=0;_requested=true;}
 internal void Tick(EntityQuery query,Func<Entity,Entity> owner,Action<Entity> enqueue,float now){
  if(_version!=Catalog.Version){_version=Catalog.Version;_requested=true;}
  if(!_snapshot.IsCreated&&(_requested||(_pass<3&&now>=_next))){_snapshot=query.ToEntityArray(Allocator.Persistent);_cursor=0;_requested=false;}
  if(!_snapshot.IsCreated)return;
  for(int i=0;i<64&&_cursor<_snapshot.Length&&SignalWorkBudget.Available;i++)enqueue(owner(_snapshot[_cursor++]));
  if(_cursor==_snapshot.Length){Dispose();_pass++;_next=now+(_pass==1?2f:8f);}
 }
 public void Dispose(){if(_snapshot.IsCreated)_snapshot.Dispose();}
}
}
