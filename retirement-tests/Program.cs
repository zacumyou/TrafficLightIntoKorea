using System;using System.Linq;using Game.Common;using Game.Tools;using TrafficLightIntoKorea;
class Program {
 static int checks;static void Check(bool ok,string label){checks++;if(!ok)throw new Exception(label);}
 static void Main(){
  var system=new KoreanVisualLifecycleSystem();var em=system.EntityManager;
  var node=em.Create();em.AddComponent<Game.Net.Node>(node);em.AddComponent<Game.Net.TrafficLights>(node);
  var signal=em.Create();em.AddComponentData(signal,new Owner{m_Owner=node});
  var sign=em.Create();em.AddComponentData(sign,new Owner{m_Owner=signal});
  Check(!NativeRemovalRetirement.ShouldPace(em,signal),"ordinary replacement keeps immediate hide");
  em.RemoveComponent<Game.Net.TrafficLights>(node);
  Check(NativeRemovalRetirement.ShouldPace(em,signal)&&NativeRemovalRetirement.ShouldPace(em,sign),"native removal paces both signal and mounted sign");
  em.AddComponent<Temp>(node);Check(!NativeRemovalRetirement.ShouldPace(em,sign),"preview nodes never trigger native removal pacing");em.RemoveComponent<Temp>(node);
  var orphan=em.Create();Check(!NativeRemovalRetirement.ShouldPace(em,orphan),"unrelated props unaffected");
  var displays=Enumerable.Range(0,40).Select(_=>em.Create()).ToArray();foreach(var e in displays){system.Retire(e,true);system.Retire(e,true);}
  Check(displays.All(e=>em.HasComponent<KoreanRetired>(e)),"queued displays stop mod updates immediately");
  Check(displays.All(e=>!em.HasComponent<Hidden>(e)&&!em.HasComponent<BatchesUpdated>(e)&&!em.HasComponent<Deleted>(e)),"native deletion requests do not rebuild hidden render batches");
  system.BeforeSave();Check(displays.All(e=>em.HasComponent<Temp>(e)),"all paced displays excluded from save");
  system.AfterSave();Check(displays.All(e=>!em.HasComponent<Temp>(e)),"save exclusion cleared after save");
  for(int i=0;i<40;i++){int before=em.DeletedWrites;system.Tick();Check(em.DeletedWrites-before==1,"one actual deletion per tick");}
  Check(displays.All(e=>em.HasComponent<Deleted>(e)),"all queued displays eventually removed");
  int writes=em.DeletedWrites;system.Tick();Check(em.DeletedWrites==writes,"duplicate requests never duplicate deletion");
  var preview=em.Create();system.Retire(preview);Check(em.HasComponent<Hidden>(preview)&&em.HasComponent<BatchesUpdated>(preview),"preview cancellation retains immediate hide");system.Tick();Check(em.HasComponent<Deleted>(preview),"normal retire queue still drains");
  var stale=em.Create();var next=em.Create();system.Retire(stale,true);system.Retire(next,true);em.Destroy(stale);system.Tick();Check(em.HasComponent<Deleted>(next),"already destroyed children skipped without wasting live deletion slot");
  var oldWorld=em.Create();system.Retire(oldWorld,true);system.Preload();system.Tick();Check(!em.HasComponent<Deleted>(oldWorld),"preload clears previous world work");
  Console.WriteLine($"PASS {checks} production retirement/owner tests; ECS fixture, not game renderer timing.");
 }
}
