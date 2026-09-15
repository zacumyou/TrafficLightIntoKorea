using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
namespace TrafficLightIntoKorea {
// Optional, read-only adapters. No dependency on either controller at load time.
internal static class SignPhaseController {
 private sealed class Reader {internal Func<EntityManager,Entity,bool> Read;internal string Label;}
 private static Reader[] _readers;private static string _available;
 internal static string Available {get{if(_readers==null)Initialize();return _available;}}
 internal static string Provider(EntityManager em,Entity owner){if(_readers==null)Initialize();foreach(var reader in _readers)if(reader.Read(em,owner))return reader.Label;return null;}
 internal static bool Ordered(EntityManager em,Entity owner)=>Provider(em,owner)!=null;
 private static void Initialize(){var readers=new List<Reader>();var labels=new List<string>();
 foreach(var name in new[]{"C2VM.TrafficToolEssentials.Components.CustomTrafficLights","TrafficLightManager.Code.Components.CustomTrafficLights"})foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()){
  var type=assembly.GetType(name,false);if(type==null||!typeof(IComponentData).IsAssignableFrom(type))continue;
  var method=type.GetMethod("GetPatternOnly",Type.EmptyTypes);if(method==null)continue;
  var patternType=method.ReturnType;if(!patternType.IsEnum||!Enum.IsDefined(patternType,"CustomPhase"))continue;
  uint custom=Convert.ToUInt32(Enum.Parse(patternType,"CustomPhase"));
  var factory=typeof(SignPhaseController).GetMethod(nameof(Create),BindingFlags.Static|BindingFlags.NonPublic).MakeGenericMethod(type);
  string label=(name.StartsWith("C2VM.",StringComparison.Ordinal)?"TTE ":"TLM ")+assembly.GetName().Version;
  readers.Add(new Reader{Read=(Func<EntityManager,Entity,bool>)factory.Invoke(null,new object[]{method,custom}),Label=label});labels.Add(label);
 }_readers=readers.ToArray();_available=labels.Count==0?"native":string.Join(" / ",labels);}
 private static Func<EntityManager,Entity,bool> Create<T>(MethodInfo pattern,uint custom)where T:unmanaged,IComponentData{return (em,owner)=>em.Exists(owner)&&em.HasComponent<T>(owner)&&Convert.ToUInt32(pattern.Invoke(em.GetComponentData<T>(owner),null))==custom;}
}
}
