using System;
using System.Collections.Generic;
namespace TrafficLightIntoKorea {
// Unmatched identifiers remain recoverable in JSON, but never enter the next
// automatic restore queue. Saving before completed scans must not retire keys.
internal sealed class RestoreNodeState {
 internal readonly HashSet<string> Pending=new HashSet<string>(StringComparer.Ordinal);
 internal readonly HashSet<string> Dormant=new HashSet<string>(StringComparer.Ordinal);
 private int _completedNonEmpty;
 internal void Clear(){Pending.Clear();Dormant.Clear();_completedNonEmpty=0;}
 internal void Load(IEnumerable<string> active,IEnumerable<string> dormant){Clear();foreach(var key in active??new string[0])if(!string.IsNullOrEmpty(key))Pending.Add(key);foreach(var key in dormant??new string[0])if(!string.IsNullOrEmpty(key)&&!Pending.Contains(key))Dormant.Add(key);}
 internal void CompletePass(int nodeCount){if(nodeCount==0)return;if(++_completedNonEmpty<2)return;Dormant.UnionWith(Pending);Pending.Clear();}
 internal string[] ActiveForSave(IEnumerable<string> selected){var active=new HashSet<string>(Pending,StringComparer.Ordinal);active.UnionWith(selected);var result=new string[active.Count];active.CopyTo(result);return result;}
 internal string[] DormantForSave(IEnumerable<string> active){var dormant=new HashSet<string>(Dormant,StringComparer.Ordinal);dormant.ExceptWith(active);var result=new string[dormant.Count];dormant.CopyTo(result);return result;}
}
}
