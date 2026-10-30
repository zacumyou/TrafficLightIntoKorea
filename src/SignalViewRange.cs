using Game.Rendering;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
internal struct SignalViewRange {
 private bool _ready;private float3 _position;
 private static World _world;private static int _frame=-1;private static SignalViewRange _cached;
 internal static SignalViewRange Read(World world){if(_world==world&&_frame==UnityEngine.Time.frameCount)return _cached;_world=world;_frame=UnityEngine.Time.frameCount;var camera=world.GetExistingSystemManaged<CameraUpdateSystem>();_cached=new SignalViewRange{_ready=camera!=null&&camera.activeCameraController!=null&&math.all(math.isfinite(camera.position)),_position=camera==null?default:camera.position};return _cached;}
 internal bool Near(float3 position)=>RemoteUpdateRules.Near(_ready,math.distancesq(position.xz,_position.xz));
}
}
