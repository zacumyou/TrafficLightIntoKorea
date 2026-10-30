using System;
using System.Collections.Generic;
using System.Linq;
namespace TrafficLightIntoKorea {
// A per-city selection policy, separate from explicit individual signal choices.
internal sealed class BulkFarPolicy {
 private readonly HashSet<string> _nearOnly=new HashSet<string>(StringComparer.Ordinal);
 internal void Set(string key,bool nearOnly){if(string.IsNullOrEmpty(key))return;if(nearOnly)_nearOnly.Add(key);else _nearOnly.Remove(key);}
 internal bool NearOnly(string key)=>key!=null&&_nearOnly.Contains(key);
 internal void Clear()=>_nearOnly.Clear();
 internal void Load(string[] keys){Clear();foreach(var key in keys??new string[0])Set(key,true);}
 internal string[] Save(IEnumerable<string> active)=>active.Where(NearOnly).Distinct(StringComparer.Ordinal).ToArray();
 internal static bool Allows(bool automatic,bool nearOnly,int mode)=>mode==1||(mode!=0&&automatic&&!nearOnly);
}
}
