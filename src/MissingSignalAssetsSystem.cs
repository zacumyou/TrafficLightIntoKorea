using System;using System.Collections.Generic;using Game;using Game.Prefabs;using Game.SceneFlow;using Game.UI;using Game.UI.Localization;using Unity.Collections;using Unity.Entities;
namespace TrafficLightIntoKorea {
public partial class MissingSignalAssetsSystem:GameSystemBase {
 private bool _ready,_finished;private float _checkAt;private string _pendingMissing;private EntityQuery _prefabQuery;
 protected override void OnCreate(){base.OnCreate();_prefabQuery=GetEntityQuery(ComponentType.ReadOnly<PrefabData>(),ComponentType.Exclude<Game.Common.Deleted>());}
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose,GameMode mode){_ready=false;_finished=false;base.OnGamePreload(purpose,mode);}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose,GameMode mode){base.OnGameLoadingComplete(purpose,mode);_ready=mode==GameMode.Game;_finished=false;_pendingMissing=null;_checkAt=UnityEngine.Time.realtimeSinceStartup+10f;}
 protected override void OnUpdate(){
  if(!_ready||_finished||Mod.Settings==null||!Mod.Settings.Enabled||UnityEngine.Time.realtimeSinceStartup<_checkAt)return;
  var app=GameManager.instance?.userInterface?.appBindings;if(app==null){_checkAt=UnityEngine.Time.realtimeSinceStartup+2f;return;}
  var prefabs=World.GetExistingSystemManaged<PrefabSystem>();if(prefabs==null)return;
  // Imported/published PrefabIDs include an asset hash. A name-only ID cannot find them.
  // Read live prefab entities, not placed objects or a catalog cached before loading.
  var available=new HashSet<string>(StringComparer.Ordinal);
  using(var entities=_prefabQuery.ToEntityArray(Allocator.Temp))foreach(var entity in entities)
   if(prefabs.TryGetPrefab<StaticObjectPrefab>(entity,out var prefab)&&prefab!=null)available.Add(prefab.name);
  var missing=new List<string>();foreach(var name in RequiredSignalAssets.Names())if(!available.Contains(name))missing.Add(name);
  if(missing.Count==0){_finished=true;RuntimeDiagnostics.Event("Required signal assets verified: all present");return;}
  var signature=string.Join("\n",missing);
  if(_pendingMissing!=signature){_pendingMissing=signature;_checkAt=UnityEngine.Time.realtimeSinceStartup+10f;return;}
  try{
   app.ShowMessageDialog(new MessageDialog(LocalizedString.Value("TRAFFIC LIGHT INTO KOREA"),
    LocalizedString.Value("신호등 에셋 구독이 필요합니다.\n\n기본 신호등 에셋 일부를 찾을 수 없습니다. Paradox Mods에서 이 모드에 필요한 신호등 에셋 팩을 구독하고 현재 플레이세트에서 활성화한 뒤 게임을 다시 실행해 주세요.\n이미 구독했다면 다운로드 완료 여부를 확인해 주세요."),
    LocalizedString.Value("찾을 수 없는 에셋 ("+missing.Count+")\n"+string.Join("\n",missing)),true,LocalizedString.Value("확인")),_=>{});
   _finished=true;RuntimeDiagnostics.Event("Required signal assets missing: "+string.Join(", ",missing));
  }catch(Exception error){_checkAt=UnityEngine.Time.realtimeSinceStartup+10f;RuntimeDiagnostics.Event("Asset subscription dialog unavailable: "+error.Message);}
 }
}
}
