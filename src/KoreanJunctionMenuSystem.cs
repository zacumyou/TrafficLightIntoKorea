using System;using Game;using Game.Prefabs;using Unity.Collections;using Unity.Entities;using UnityEngine;
namespace TrafficLightIntoKorea {
public partial class KoreanJunctionMenuSystem:GameSystemBase {
 private bool _done;private float _next;internal const string Name="CSKRKoreanJunctionTool";
 protected override void OnUpdate(){if(_done||UnityEngine.Time.realtimeSinceStartup<_next)return;_next=UnityEngine.Time.realtimeSinceStartup+2;var ps=World.GetExistingSystemManaged<PrefabSystem>();
  using(var q=EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PrefabData>(),ComponentType.ReadOnly<UIObjectData>()))
  using(var a=q.ToEntityArray(Allocator.Temp))foreach(var e in a){if(!ps.TryGetPrefab<PrefabBase>(e,out var p)||p==null||!p.TryGet<NetUpgrade>(out var upgrade)||upgrade.m_SetState==null||Array.IndexOf(upgrade.m_SetState,NetPieceRequirements.TrafficLights)<0||!p.TryGet<UIObject>(out var original)||original.m_Group==null)continue;
   var tool=ScriptableObject.CreateInstance<KoreanJunctionToolPrefab>();tool.name=Name;var ui=tool.AddComponent<UIObject>();ui.m_Group=original.m_Group;ui.m_Priority=original.m_Priority+1;
   string location=typeof(KoreanJunctionMenuSystem).Assembly.Location;
   if(Game.SceneFlow.GameManager.instance.modManager.TryGetExecutableAsset(typeof(Mod).Assembly,out var asset))location=asset.path;
   string directory=System.IO.Path.GetDirectoryName(location);
   Game.SceneFlow.GameManager.instance.userInterface.view.uiSystem.AddHostLocation("tlk",directory,false);
   ui.m_Icon="coui://tlk/KoreanJunction.svg";
   if(ps.AddPrefab(tool)){var edit=ScriptableObject.CreateInstance<IndividualSignalToolPrefab>();edit.name="CSKRIndividualSignalTool";var editUI=edit.AddComponent<UIObject>();editUI.m_Group=original.m_Group;editUI.m_Priority=original.m_Priority+2;editUI.m_Icon="coui://tlk/IndividualSignal.svg";if(!ps.AddPrefab(edit))UnityEngine.Object.Destroy(edit);_done=true;RuntimeDiagnostics.Event("junction-menu registered next to "+p.name+" group="+original.m_Group.name);}else UnityEngine.Object.Destroy(tool);break;
  }
 }
}
}
