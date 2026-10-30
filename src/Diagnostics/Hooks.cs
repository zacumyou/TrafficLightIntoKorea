using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
using Colossal.UI.Binding;
using Unity.Entities;

namespace TrafficLightIntoKorea.Diagnostics
{
    public static class Hooks
    {
        private static long operations;
        public static void LoadReturned(System.Threading.Tasks.Task __result,MethodBase __originalMethod,ActionState __state){if(__state==null||__result==null)return;string method=__originalMethod.DeclaringType.FullName+"."+__originalMethod.Name;try{__result.ContinueWith(task=>{if(Probe.Session==__state.session)Probe.Emit("ASYNC_LOAD_END",new{id=__state.id,method,elapsedMs=Probe.Ms(Probe.Now-__state.start),cancelled=task.IsCanceled,faulted=task.IsFaulted,exception=task.Exception?.ToString()});},System.Threading.Tasks.TaskScheduler.Default);}catch{Probe.RecordFailure();}}
        public static void OperationBegin(MethodBase __originalMethod,object[] __args,out ActionState __state){__state=null;if(Probe.Session==null)return;try{__state=new ActionState{start=Probe.Now,session=Probe.Session,id=System.Threading.Interlocked.Increment(ref operations)};Probe.Emit("OPERATION_BEGIN",new{id=__state.id,method=__originalMethod.DeclaringType.FullName+"."+__originalMethod.Name,args=__args?.Select(Probe.Scalar).ToArray()});}catch{Probe.RecordFailure();}}
        public static void OperationEnd(MethodBase __originalMethod,ActionState __state,Exception __exception){if(__state==null||__state.session!=Probe.Session)return;try{Probe.Emit("OPERATION_END",new{id=__state.id,method=__originalMethod.DeclaringType.FullName+"."+__originalMethod.Name,elapsedMs=Probe.Ms(Probe.Now-__state.start),exception=__exception?.ToString(),asyncDispatch=__originalMethod is MethodInfo m&&typeof(System.Threading.Tasks.Task).IsAssignableFrom(m.ReturnType)});}catch{Probe.RecordFailure();}}
        public sealed class ActionState { internal long start, previous, id; internal string session; }
        public struct SystemState { internal long start; internal bool enabled; internal string name, session; }
        sealed class Name { internal string value, session; internal bool running; }
        static readonly ConditionalWeakTable<Delegate, Name> Names = new ConditionalWeakTable<Delegate, Name>();
        static readonly ConditionalWeakTable<SystemBase, Name> Systems = new ConditionalWeakTable<SystemBase, Name>();
        static readonly ConditionalWeakTable<ComponentSystemBase, Name> Running = new ConditionalWeakTable<ComponentSystemBase, Name>();
        static readonly List<WeakReference> Bindings = new List<WeakReference>();
        sealed class Output { internal string name, session; internal long start; internal int omitted; internal readonly List<object> values = new List<object>(); }
        [ThreadStatic] static Dictionary<object, Output> outputs;
        public static void BridgeRegister(string __0, Delegate __1)
        {
            try { Names.GetValue(__1, d => new Name { value = __0 }); } catch { Probe.RecordFailure(); }
        }
        public static void BridgeBegin(Delegate __1, object[] __2, bool __3, out ActionState __state)
        {
            __state = null; if (Probe.Session == null) return;
            if(Names.TryGetValue(__1,out var traceName)&&(traceName.value=="tlkIndividual.trace"||traceName.value=="tlkIndividual.diagnostic"))return;
            if(__1.Target is BindingBase traceBinding&&(traceBinding.path=="tlkIndividual.trace"||traceBinding.path=="tlkIndividual.diagnostic"))return;
            try
            {
                __state = new ActionState { start = Probe.Now, previous = Probe.Action, id = Probe.NewAction(), session = Probe.Session };
                Probe.Action = __state.id;
                var binding = __1.Target as BindingBase;
                Probe.Emit("UI_TRIGGER_BEGIN", new { binding = binding?.path ?? (Names.TryGetValue(__1, out var name) ? name.value : "unregistered-name"),
                    type = __1.Target?.GetType().FullName, requestedExecution = __3,
                    active = !(binding is RawTriggerBindingBase raw) || raw.active,
                    bridgeCallback = Probe.Method(__1.Method), arguments = __2?.Select(Probe.Scalar).ToArray(),
                    callbacks = binding == null ? null : Probe.Callbacks(binding), stack = new StackTrace(1, false).ToString() });
            }
            catch { Probe.RecordFailure(); }
        }
        public static void BridgeException(Exception __0)
        {
            if (Probe.Session == null) return;
            try { Probe.Emit("UI_EXCEPTION", new { exception = __0?.ToString() }); } catch { Probe.RecordFailure(); }
        }
        public static void ActionEnd(ActionState __state, Exception __exception)
        {
            if (__state == null) return;
            try { if (Probe.Session == __state.session) Probe.Emit("UI_TRIGGER_END", new { id = __state.id, elapsedMs = Probe.Ms(Probe.Now - __state.start), escapedException = __exception?.GetType().FullName }); }
            catch { Probe.RecordFailure(); }
            finally { Probe.Action = __state.previous; }
        }
        public static void InputValue(object[] __args)
        {
            if (Probe.Session == null || Probe.Action == 0) return;
            try { Probe.Emit("UI_ARGUMENT", new { value = Probe.Scalar(__args[0]) }); } catch { Probe.RecordFailure(); }
        }
        public static void BindingAttached(BindingBase __instance)
        {
            lock (Bindings) { if (Bindings.Count >= 20000) Bindings.RemoveAll(w => !w.IsAlive); if (Bindings.Count < 20000) Bindings.Add(new WeakReference(__instance)); }
            if (Probe.Session == null) return;
            try { Probe.Emit("BINDING_ATTACH", new { binding = __instance.path, type = __instance.GetType().FullName, callbacks = Probe.Callbacks(__instance) }); } catch { Probe.RecordFailure(); }
        }
        internal static void SnapshotBindings()
        {
            lock (Bindings)
                foreach (var w in Bindings)
                    if (w.Target is BindingBase b && b.attached)
                        Probe.Emit("BINDING_SNAPSHOT", new { binding = b.path, type = b.GetType().FullName, observers = b is EventBindingBase e ? (int?)e.observerCount : null, callbacks = Probe.Callbacks(b) });
        }
        static void RunningState(ComponentSystemBase instance, bool active)
        {
            var info = Running.GetValue(instance, s => new Name { value = s.GetType().FullName });
            if (info.session != Probe.Session || info.running != active)
            {
                info.session = Probe.Session; info.running = active;
                Probe.Emit("SYSTEM_RUN_STATE", new { system = info.value, world = instance.World.Name, runnable = active });
            }
        }
        public static void ShouldRun(ComponentSystemBase __instance, bool __result)
        {
            if (Probe.Session == null) return;
            try { RunningState(__instance, __instance.Enabled && __result); } catch { Probe.RecordFailure(); }
        }
        public static void Subscription(EventBindingBase __instance, MethodBase __originalMethod)
        {
            if (Probe.Session == null) return;
            try { Probe.Emit("UI_SUBSCRIPTION", new { binding = __instance.path, operation = __originalMethod.Name, observers = __instance.observerCount, active = __instance.active }); } catch { Probe.RecordFailure(); }
        }
        public static void SystemBegin(SystemBase __instance, out SystemState __state)
        {
            __state = default; if (Probe.Session == null) return;
            try { __state = new SystemState { start = Probe.Now, session = Probe.Session, enabled = __instance.Enabled,
                name = Systems.GetValue(__instance, s => new Name { value = s.World.Name + ":" + s.GetType().Assembly.GetName().Name + ":" + s.GetType().FullName }).value };
                if (!__state.enabled) RunningState(__instance, false); }
            catch { Probe.RecordFailure(); }
        }
        public static void SystemEnd(SystemState __state, Exception __exception)
        {
            if (__state.start == 0 || __state.session != Probe.Session) return;
            try
            {
                double ms = Probe.Ms(Probe.Now - __state.start); Probe.Aggregate(__state.name, ms, __state.enabled);
                if (ms >= 2 || __exception != null) Probe.Emit("SYSTEM_SLOW", new { system = __state.name, inclusiveMs = ms,
                    enabled = __state.enabled, exception = __exception?.GetType().FullName });
            }
            catch { Probe.RecordFailure(); }
        }
        public static void EnabledChanging(ComponentSystemBase __instance, bool __0)
        {
            if (Probe.Session == null) return;
            try { if (__instance.Enabled != __0) Probe.Emit("SYSTEM_ENABLED_REQUEST", new { system = __instance.GetType().FullName,
                from = __instance.Enabled, to = __0, stack = new StackTrace(1, false).ToString() }); } catch { Probe.RecordFailure(); }
        }
        public static void ToolState(Game.Tools.ToolSystem __instance, MethodBase __originalMethod)
        {
            if (Probe.Session == null) return;
            try { Probe.Emit("TOOL_STATE", new { operation = __originalMethod.Name, tool = __instance.activeTool?.GetType().FullName,
                prefab = __instance.activeTool == null ? null : __instance.activePrefab?.name, infoview = __instance.infoview?.name }); }
            catch { Probe.RecordFailure(); }
        }
        public static void OutputBegin(object __instance, string __0)
        {
            if (Probe.Session == null) return;
            try
            {
                if (__0 == null || (__0.IndexOf("tlk", StringComparison.OrdinalIgnoreCase) < 0 && __0.IndexOf("TrafficLightIntoKorea", StringComparison.OrdinalIgnoreCase) < 0)) return;
                if (outputs == null) outputs = new Dictionary<object, Output>();
                if (outputs.Count > 2048) { outputs.Clear(); Probe.RecordFailure(); }
                outputs[__instance] = new Output { name = __0, session = Probe.Session, start = Probe.Now };
            }
            catch { Probe.RecordFailure(); }
        }
        public static void OutputValue(object __instance, object[] __args)
        {
            if (Probe.Session == null) return;
            try { if (outputs != null && outputs.TryGetValue(__instance, out var o)) { if (o.values.Count < 128) o.values.Add(Probe.Scalar(__args[0])); else o.omitted++; } } catch { Probe.RecordFailure(); }
        }
        public static void OutputEnd(object __instance)
        {
            try
            {
                if (outputs == null || !outputs.TryGetValue(__instance, out var o)) return;
                outputs.Remove(__instance);
                if (Probe.Session != null && Probe.Session == o.session) Probe.Emit("UI_OUTPUT", new { binding = o.name, elapsedMs = Probe.Ms(Probe.Now - o.start), scalars = o.values, omittedScalars = o.omitted });
            }
            catch { Probe.RecordFailure(); }
        }
    }
}
