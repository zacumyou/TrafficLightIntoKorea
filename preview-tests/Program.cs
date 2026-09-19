using System;using System.Linq;using System.Collections.Generic;using System.IO;
using TrafficLightIntoKorea;using Unity.Entities;using Unity.Mathematics;using Game.Common;using Game.Objects;using Game.Prefabs;using Game.Tools;
class Program {
 static int checks;static void Check(bool value,string name){checks++;if(!value)throw new Exception(name);}
 static void Main(){
  var system=new ManualSignalPreviewSystem();var em=system.EntityManager;var life=new KoreanVisualLifecycleSystem{EM=em};system.World.Lifecycle=life;
  Entity Make()=>em.CreateEntity(new EntityArchetype{Valid=true});var owner=Make();var source=Make();var prefab=Make();em.SetComponentData(prefab,new ObjectData{m_Archetype=new EntityArchetype{Valid=true}});em.SetComponentData(source,new TrafficLight{State=7});
  var tree=new HashSet<Entity>();
  Entity[] Previews()=>em.Data.Keys.Where(e=>em.HasComponent<FarSignal>(e)&&!em.HasComponent<Deleted>(e)).ToArray();
  // Native SearchSystem's relevant contract: Created => Add, Updated => Update.
  void Search(){foreach(var e in em.Data.Keys.ToArray()){if(em.HasComponent<Deleted>(e)){tree.Remove(e);continue;}if(!em.HasComponent<FarSignal>(e)||!em.HasComponent<Updated>(e))continue;if(em.HasComponent<Created>(e)){if(!tree.Add(e))throw new Exception("Duplicate Add");}else if(!tree.Contains(e))throw new Exception("Item not found (NativeQuadTree.Update)");}}
  void Cleanup(){foreach(var e in em.Data.Keys.ToArray()){em.RemoveComponent<Created>(e);em.RemoveComponent<Updated>(e);}}
  void Frame(){system.Tick();life.Tick();Search();Cleanup();}
  system.Begin(owner,source,prefab,new quaternion(1));system.Move(new float3(1,2,3),true);Check(Previews().Length==0,"UI requests must not allocate");Cleanup();Frame();Check(Previews().Length==1&&tree.Count==1,"First frame registers before cleanup");var preview=Previews()[0];
  for(int i=0;i<100;i++){system.Move(new float3(i,2,3),true);Frame();}Check(Previews()[0]==preview&&tree.Count==1,"Continuous movement retains registered entity");
  system.Move(default,false);Frame();Check(em.HasComponent<Hidden>(preview),"Raycast miss hides preview");system.Move(new float3(2,3,4),true);Frame();Check(!em.HasComponent<Hidden>(preview),"Raycast recovery reveals existing preview");
  system.Move(new float3(float.NaN,0,0),true);Frame();Check(em.HasComponent<Hidden>(preview),"Nonfinite hit hidden");
  system.Cancel();Frame();Check(Previews().Length==0&&tree.Count==0,"Cancel removes registered preview");
  system.Begin(owner,source,prefab,default);system.Move(default,true);system.Cancel();Frame();Check(Previews().Length==0,"Cancel before first frame never creates");
  system.Begin(owner,source,prefab,default);system.Move(default,true);Frame();system.Begin(owner,source,prefab,default);system.Move(new float3(3,0,0),true);Frame();Check(Previews().Length==1&&tree.Count==1,"Replacement removes old and registers new");
  system.BeforeSave();life.Tick();Search();Cleanup();Frame();Check(Previews().Length==0&&tree.Count==0,"Save removes preview and pending request");
  system.Begin(owner,source,prefab,default);system.Move(default,true);system.BeforeSave();Frame();Check(Previews().Length==0,"Save before creation cancels request");
  system.Begin(owner,source,prefab,default);system.Move(default,true);Frame();em.AddComponent<Deleted>(source);Frame();Check(Previews().Length==0,"Source deletion retires preview");
  // Reproduce old timing: UI creation then cleanup, followed by Updated movement.
  var broken=Make();em.AddComponent<FarSignal>(broken);em.AddComponent<Created>(broken);em.AddComponent<Updated>(broken);Cleanup();em.AddComponent<Updated>(broken);bool reproduced=false;try{Search();}catch(Exception e){reproduced=e.Message.Contains("NativeQuadTree.Update");}Check(reproduced,"Harness detects original unregistered update");
  var root=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../"));var mod=File.ReadAllText(Path.Combine(root,"src/Mod.cs"));Check(mod.Contains("UpdateBefore<ManualSignalPreviewSystem,KoreanVisualLifecycleSystem>(SystemUpdatePhase.Modification5)")&&mod.Contains("UpdateBefore<KoreanVisualLifecycleSystem,ApproachSignalSystem>")&&mod.Contains("UpdateBefore<ApproachSignalSystem,FarSignalSystem>")&&mod.Contains("UpdateBefore<FarSignalSystem,TrafficSignSystem>")&&mod.Contains("UpdateBefore<TrafficSignSystem,Game.Objects.SearchSystem>"),"Production registration runs preview before SearchSystem");
  Console.WriteLine($"PASS: {checks} preview lifecycle checks including original-crash reproduction. ECS/search harness, not live game.");
 }
}
