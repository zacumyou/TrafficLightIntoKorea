using Game;
using Game.Modding;
using Game.SceneFlow;
namespace TrafficLightIntoKorea.Diagnostics {
// No Harmony hooks, global UI interception, or injected DOM listeners.
internal static class Controller {
 private static bool initialized,loaded,requested;
 internal static bool InGame=>loaded&&GameManager.instance!=null&&GameManager.instance.gameMode==GameMode.Game&&!GameManager.instance.isGameLoading;
 internal static void Initialize(IMod mod){initialized=true;loaded=false;requested=Mod.Settings.DetailedDiagnostics;}
 internal static void Configure(bool enabled){requested=enabled;Refresh();}
 internal static void Preload(){loaded=false;Probe.Enable(false);}
 internal static void Loaded(GameMode mode){loaded=mode==GameMode.Game;Refresh();}
 private static void Refresh(){if(!initialized)return;bool active=requested&&InGame;if(active==(Probe.Session!=null))return;if(active){Probe.Start();Probe.Enable(true);ToggleStallTrace.Arm();Probe.Emit("GAME_SESSION_READY",new{version=typeof(Mod).Assembly.GetName().Version.ToString(3)});}else Probe.Enable(false);}
 internal static void Tick(){Refresh();if(InGame)Probe.Frame();}
 internal static void Shutdown(){initialized=false;loaded=false;requested=false;Probe.Stop();}
}
public partial class InGameDiagnosticsSystem:GameSystemBase {
 protected override void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose,GameMode mode){Controller.Preload();base.OnGamePreload(purpose,mode);}
 protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose,GameMode mode){base.OnGameLoadingComplete(purpose,mode);Controller.Loaded(mode);}
 protected override void OnUpdate(){Controller.Tick();}
}
}
