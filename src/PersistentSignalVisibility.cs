using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game;
using Game.Common;
using Game.Objects;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
namespace TrafficLightIntoKorea {
internal struct KoreanPersistentHidden:IComponentData {}
internal static class PersistentSignalVisibility {
 internal static bool Enabled; internal static bool HiddenOrOwned(EntityManager em,Entity entity)=>em.HasComponent<Hidden>(entity)||em.HasComponent<KoreanPersistentHidden>(entity);
 internal static void Hide(EntityManager em,Entity entity){
  if(Enabled&&!em.HasComponent<KoreanPersistentHidden>(entity))em.AddComponent<KoreanPersistentHidden>(entity);
  if(!em.HasComponent<Hidden>(entity)){em.AddComponent<Hidden>(entity);RuntimeDiagnostics.Count("visibility.hideWrite");}
 }
 internal static void Show(EntityManager em,Entity entity){
  if(em.HasComponent<KoreanPersistentHidden>(entity))em.RemoveComponent<KoreanPersistentHidden>(entity);
  if(em.HasComponent<Hidden>(entity)){em.RemoveComponent<Hidden>(entity);RuntimeDiagnostics.Count("visibility.showWrite");}
 }
}
// CS2 1.6.2f1 removes Hidden from ordinary subobjects of visible owners every
// Modification5. Preserve native processing, excluding only our owned hides.
// Do not alter Owner, native traffic simulation, or any asset's shared prefab.
public partial class PersistentSignalVisibilitySystem:GameSystemBase {
 private SubObjectHiddenSystem _native;
 private FieldInfo _field;
 private EntityQuery _original,_filtered,_owned;
 private bool _installed;
 private readonly List<Entity> _saving=new List<Entity>();
 protected override void OnCreate(){base.OnCreate();
  _owned=GetEntityQuery(ComponentType.ReadOnly<KoreanPersistentHidden>());
  try{
   _native=World.GetOrCreateSystemManaged<SubObjectHiddenSystem>();
   _field=typeof(SubObjectHiddenSystem).GetField("m_HiddenQuery",BindingFlags.Instance|BindingFlags.NonPublic);
   if(_field==null||_field.FieldType!=typeof(EntityQuery))throw new InvalidOperationException("Native hidden query contract changed");
   _original=(EntityQuery)_field.GetValue(_native);var descriptions=_original.GetEntityQueryDescs();
   if(descriptions.Length==0||descriptions.Any(d=>!d.All.Contains(ComponentType.ReadOnly<Hidden>())))throw new InvalidOperationException("Unexpected native hidden query");
   foreach(var d in descriptions)d.None=d.None.Concat(new[]{ComponentType.ReadOnly<KoreanPersistentHidden>()}).ToArray();
   _filtered=GetEntityQuery(descriptions);_field.SetValue(_native,_filtered);
   _installed=true;PersistentSignalVisibility.Enabled=true;RuntimeDiagnostics.Event("Persistent signal visibility installed; native hidden query excludes TLIK-owned hides");
  }catch(Exception error){PersistentSignalVisibility.Enabled=false;RuntimeDiagnostics.Event("Persistent visibility unavailable; retaining compatibility fallback: "+error.Message);}
 }
 internal void BeforeSave(){_saving.Clear();using(var entities=_owned.ToEntityArray(Allocator.Temp))foreach(var entity in entities){_saving.Add(entity);EntityManager.RemoveComponent<KoreanPersistentHidden>(entity);}}
 internal void AfterSave(){if(PersistentSignalVisibility.Enabled)foreach(var entity in _saving)if(EntityManager.Exists(entity)&&!EntityManager.HasComponent<Deleted>(entity)&&EntityManager.HasComponent<Hidden>(entity)&&!EntityManager.HasComponent<KoreanPersistentHidden>(entity))EntityManager.AddComponent<KoreanPersistentHidden>(entity);_saving.Clear();}
 internal void Stop(){
  PersistentSignalVisibility.Enabled=false;
  using(var entities=_owned.ToEntityArray(Allocator.Temp))foreach(var entity in entities){PersistentSignalVisibility.Show(EntityManager,entity);if(!EntityManager.HasComponent<BatchesUpdated>(entity))EntityManager.AddComponent<BatchesUpdated>(entity);}
  if(_installed){if(((EntityQuery)_field.GetValue(_native)).Equals(_filtered))_field.SetValue(_native,_original);_installed=false;}_saving.Clear();
 }
 protected override void OnDestroy(){Stop();base.OnDestroy();}
 protected override void OnUpdate(){}
}
}
