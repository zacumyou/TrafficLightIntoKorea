using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace TrafficLightIntoKorea {
// Both ambiguity indexing and matching are incremental. A duplicate at the end
// of a city must be discovered before any matching junction can be selected.
internal sealed class IncrementalNodeRestore<T> {
 private readonly T[] _nodes;
 private readonly string[] _keys;
 // Fixed distance bands avoid a whole-city sort. Modern matches still precede
 // speculative legacy matching, with stable order inside each distance band.
 private readonly List<int>[] _candidates=new List<int>[6];
 private readonly Func<T,int> _priority;
 private int _bucket,_candidateCount;
 private readonly HashSet<string> _pending, _seen=new HashSet<string>(StringComparer.Ordinal), _ambiguous=new HashSet<string>(StringComparer.Ordinal);
 private readonly Func<T,string> _key, _legacy;
 private readonly Func<T,string,string,bool> _select;
 private int _cursor;
 private bool _indexed, _needsLegacy;
 internal bool Done {get;private set;}
 internal IncrementalNodeRestore(T[] nodes,HashSet<string> pending,Func<T,string> key,Func<T,string> legacy,Func<T,string,string,bool> select,Func<T,int> priority=null){
  _nodes=nodes;_keys=new string[nodes.Length];_pending=pending;_key=key;_legacy=legacy;_select=select;_priority=priority;for(int i=0;i<_candidates.Length;i++)_candidates[i]=new List<int>();RefreshLegacy();
 }
 private void RefreshLegacy(){_needsLegacy=false;foreach(var key in _pending)if(!key.StartsWith("N2:",StringComparison.Ordinal)){_needsLegacy=true;break;}}
 // Limits are checked between nodes. Native ECS work on one node is indivisible.
 internal void Step(int maxNodes=128,double maxMilliseconds=1.0){
  long start=Stopwatch.GetTimestamp();
  for(int count=0;count<maxNodes&&!Done;count++){
   if(count>0&&(Stopwatch.GetTimestamp()-start)*1000.0/Stopwatch.Frequency>=maxMilliseconds)return;
   if(_pending.Count==0){Done=true;return;}
   if(!_indexed&&_cursor==_nodes.Length){_indexed=true;_cursor=0;if(_candidateCount==0){Done=true;return;}}
   if(_indexed){while(_bucket<_candidates.Length&&_cursor==_candidates[_bucket].Count){_bucket++;_cursor=0;}if(_bucket==_candidates.Length){Done=true;return;}}
   int index=_indexed?_candidates[_bucket][_cursor]:_cursor;_cursor++;var node=_nodes[index];
   if(!_indexed){var key=_key(node);if(key!=null&&(_needsLegacy||_pending.Contains(key))){_keys[index]=key;int band=Math.Max(0,Math.Min(2,_priority?.Invoke(node)??0));_candidates[band+(_pending.Contains(key)?0:3)].Add(index);_candidateCount++;if(!_seen.Add(key))_ambiguous.Add(key);}continue;}
   var savedKey=_keys[index];if(savedKey==null||_ambiguous.Contains(savedKey))continue;
   // The road can be removed or moved between the indexing and apply phases.
   if(!_needsLegacy&&!_pending.Contains(savedKey))continue;
   if(_key(node)!=savedKey)continue;
   var legacy=_needsLegacy?_legacy(node):null;
   if(!_pending.Contains(savedKey)&&(legacy==null||!_pending.Contains(legacy)))continue;
   if(!_select(node,savedKey,legacy))continue;
   _pending.Remove(savedKey);if(legacy!=null)_pending.Remove(legacy);RefreshLegacy();
   if(_pending.Count==0)Done=true;
   return; // At most one successful junction application per frame.
  }
 }
 internal static float RetryDelay(int passesWithoutProgress)=>Math.Min(30,5*(1<<Math.Min(3,Math.Max(0,passesWithoutProgress-1))));
}
internal static class RestoreDistancePriority {
 internal static int Band(double squaredDistance)=>squaredDistance>=0&&squaredDistance<=600*600?0:squaredDistance>=0&&squaredDistance<=1800*1800?1:2;
}
}
