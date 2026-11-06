using System;
using System.Collections.Generic;
namespace TrafficLightIntoKorea {
// Retry archived keys on each load. Only genuinely unmatched keys are parked;
// a matched junction awaiting native signals must remain recoverable.
internal sealed class RestoreNodeState {
 internal readonly HashSet<string> Pending=new HashSet<string>(StringComparer.Ordinal);
 internal readonly HashSet<string> Dormant=new HashSet<string>(StringComparer.Ordinal);
 private int _completedNonEmpty;private readonly HashSet<string> _matched=new HashSet<string>(StringComparer.Ordinal);
 internal void Clear(){Pending.Clear();Dormant.Clear();_matched.Clear();_completedNonEmpty=0;}
 internal void Load(IEnumerable<string> active,IEnumerable<string> dormant){Clear();foreach(var key in active??new string[0])if(!string.IsNullOrEmpty(key))Pending.Add(key);foreach(var key in dormant??new string[0])if(!string.IsNullOrEmpty(key))Pending.Add(key);}
 internal void Matched(string key){if(key!=null&&Pending.Contains(key))_matched.Add(key);}
 internal void Forget(string key){if(key==null)return;Pending.Remove(key);Dormant.Remove(key);_matched.Remove(key);}
 internal void CompletePass(int nodeCount){if(nodeCount==0)return;if(++_completedNonEmpty<2)return;var missing=new HashSet<string>(Pending,StringComparer.Ordinal);missing.ExceptWith(_matched);Dormant.UnionWith(missing);Pending.ExceptWith(missing);}
 internal string[] ActiveForSave(IEnumerable<string> selected){var active=new HashSet<string>(Pending,StringComparer.Ordinal);active.UnionWith(selected);var result=new string[active.Count];active.CopyTo(result);return result;}
 internal string[] DormantForSave(IEnumerable<string> active){var dormant=new HashSet<string>(Dormant,StringComparer.Ordinal);dormant.ExceptWith(active);var result=new string[dormant.Count];dormant.CopyTo(result);return result;}
}
}
