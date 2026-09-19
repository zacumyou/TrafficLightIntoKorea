using System.Collections.Generic;using Unity.Entities;
namespace TrafficLightIntoKorea {
public partial class FarSignalSystem {
 private readonly Dictionary<Entity,float> _missingSince=new Dictionary<Entity,float>();
 private readonly Dictionary<Entity,float> _retryAt=new Dictionary<Entity,float>();
 private readonly Dictionary<Entity,float> _retryUntil=new Dictionary<Entity,float>();
 private void RetryLater(Entity owner){float now=UnityEngine.Time.realtimeSinceStartup;if(!_retryUntil.TryGetValue(owner,out var until)||now>until+1){until=now+10;_retryUntil[owner]=until;}if(now<until)_retryAt[owner]=now+.5f;}
 private readonly List<Entity> _dueRetries=new List<Entity>();private void TickRetries(){if(_retryAt.Count==0)return;float now=UnityEngine.Time.realtimeSinceStartup;var due=_dueRetries;due.Clear();foreach(var pair in _retryAt)if(pair.Value<=now)due.Add(pair.Key);foreach(var owner in due){_retryAt.Remove(owner);Queue(owner);}}
 private void ResetRetries(){_missingSince.Clear();_retryAt.Clear();_retryUntil.Clear();}
}
}
