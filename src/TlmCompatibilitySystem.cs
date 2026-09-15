using System;using System.Collections.Generic;using Game;using Game.Common;using Game.Net;using Game.Tools;using Unity.Entities;using Unity.Collections;
namespace TrafficLightIntoKorea {
// Optional integration: never load or modify the other mod's assembly or components.
public partial class TlmCompatibilitySystem:GameSystemBase {
 private ComponentType[] _custom=new ComponentType[0];private string _providers="";private bool _available,_ready,_initial;private EntityQuery _all,_changed;
 private readonly List<Entity> _owners=new List<Entity>();private readonly Dictionary<Entity,TlmSettleState> _states=new Dictionary<Entity,TlmSettleState>();private int _cursor;
 internal static string Status="TLM/TTE absent";private int _repairs;
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose,GameMode mode){EndPlacement();_placementOwners.Clear();_ready=false;_owners.Clear();_states.Clear();_cursor=0;_initial=true;_repairs=0;base.OnGamePreload(purpose,mode);}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose,GameMode mode){base.OnGameLoadingComplete(purpose,mode);_ready=mode==GameMode.Game;_initial=true;
  if(!_available){var types=new List<ComponentType>();var names=new List<string>();var seen=new HashSet<string>();foreach(var a in AppDomain.CurrentDomain.GetAssemblies())foreach(var fullName in SignalControllerTypes.Names){var t=a.GetType(fullName,false);if(t==null||!typeof(IComponentData).IsAssignableFrom(t)||!seen.Add(fullName))continue;types.Add(ComponentType.ReadOnly(t));names.Add(SignalControllerTypes.Label(fullName)+" "+a.GetName().Version);}
   if(types.Count>0){_custom=types.ToArray();_providers=string.Join(" / ",names);_all=GetEntityQuery(ControllerQuery(false));_changed=GetEntityQuery(ControllerQuery(true));_available=true;Status=_providers;}
  }

 }
 private EntityQueryDesc ControllerQuery(bool changed){var required=new List<ComponentType>{ComponentType.ReadOnly<Node>(),ComponentType.ReadOnly<TrafficLights>(),ComponentType.ReadOnly<SubLane>(),ComponentType.ReadOnly<Game.Objects.SubObject>()};if(changed)required.Add(ComponentType.ReadOnly<Updated>());return new EntityQueryDesc{All=required.ToArray(),Any=_custom,None=new[]{ComponentType.Exclude<Deleted>(),ComponentType.Exclude<Temp>()}};}
 private bool Controlled(Entity e){foreach(var type in _custom)if(EntityManager.HasComponent(e,type))return true;return false;}
 private void Track(Entity e){if(!_states.ContainsKey(e)){_states.Add(e,new TlmSettleState());_owners.Add(e);}}
 protected override void OnUpdate(){if(!_ready||!_available||Mod.Settings==null||!Mod.Settings.Enabled)return;
  if(_initial){using(var a=_all.ToEntityArray(Allocator.Temp))foreach(var e in a)Track(e);_initial=false;}
  using(var a=_changed.ToEntityArray(Allocator.Temp))foreach(var e in a)Track(e);
  var timer=System.Diagnostics.Stopwatch.StartNew();int count=Math.Min(4,_owners.Count);
  for(int i=0;i<count&&_owners.Count>0&&timer.ElapsedMilliseconds<2;i++){
   if(_cursor>=_owners.Count)_cursor=0;var owner=_owners[_cursor];
   if(!EntityManager.Exists(owner)||EntityManager.HasComponent<Deleted>(owner)||!Controlled(owner)||!EntityManager.HasComponent<TrafficLights>(owner)){_states.Remove(owner);_owners[_cursor]=_owners[_owners.Count-1];_owners.RemoveAt(_owners.Count-1);continue;}_cursor++;
   int hash=17;bool hasMask=false;unchecked{foreach(var item in EntityManager.GetBuffer<SubLane>(owner,true)){var lane=item.m_SubLane;if(!EntityManager.Exists(lane)||EntityManager.HasComponent<Deleted>(lane)||EntityManager.HasComponent<Temp>(lane)||!EntityManager.HasComponent<LaneSignal>(lane))continue;var signal=EntityManager.GetComponentData<LaneSignal>(lane);hasMask=true;hash=hash*31+signal.m_GroupMask;if(EntityManager.HasComponent<Curve>(lane))hash=hash*31+EntityManager.GetComponentData<Curve>(lane).m_Bezier.GetHashCode();}}
   // Phase changes alter m_Signal, not this structural fingerprint. No per-phase rebuild.
   if(!_states[owner].Observe(hash,hasMask,UnityEngine.Time.realtimeSinceStartup))continue;
   // Read-only compatibility: request our caches only; do not rebuild native road objects.
   World.GetExistingSystemManaged<ApproachSignalSystem>()?.RefreshOwner(owner);World.GetExistingSystemManaged<DirectionalLightSystem>()?.RefreshOwner(owner);World.GetExistingSystemManaged<FarSignalSystem>()?.RefreshOwner(owner);World.GetExistingSystemManaged<LeftSignalSystem>()?.RefreshOwner(owner);World.GetExistingSystemManaged<TrafficSignSystem>()?.RefreshOwner(owner);_repairs++;
  }
  Status=_providers+" tracked="+_owners.Count+" reconciled="+_repairs;
 }
}
internal sealed class TlmSettleState {
 private int _candidate,_applied;private bool _seen,_hasApplied;private float _since;
 internal bool Observe(int fingerprint,bool hasMask,float now){if(!_seen||_candidate!=fingerprint){_candidate=fingerprint;_since=now;_seen=true;return false;}if(!hasMask||now-_since<.25f||(_hasApplied&&_applied==fingerprint))return false;_applied=fingerprint;_hasApplied=true;return true;}
}
}
