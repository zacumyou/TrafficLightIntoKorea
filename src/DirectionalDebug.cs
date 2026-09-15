using System.Text;using Game.Prefabs;using Game.Rendering;using Unity.Entities;
namespace TrafficLightIntoKorea {
public partial class DirectionalLightSystem {
 internal void Describe(Entity entity,StringBuilder text){
  if(!_bindings.TryGetValue(entity,out var binding))return;
  var info=binding.Info;
  text.AppendLine("DIRECTIONAL entity="+entity+" source="+binding.Source+" known="+info.Known+" asset="+AssetName(entity)+" enabled="+Mod.Settings.IndependentLights+" displayFrame="+info.Frame+" currentFrame="+UnityEngine.Time.frameCount+" straight="+info.StraightDisplay+" left="+info.LeftDisplay);
  if(Live(info.Owner)&&EntityManager.HasComponent<Game.Net.TrafficLights>(info.Owner)){
   var node=EntityManager.GetComponentData<Game.Net.TrafficLights>(info.Owner);
   text.AppendLine("CONTROLLER state="+node.m_State+" current="+node.m_CurrentSignalGroup+" next="+node.m_NextSignalGroup);
  }
  foreach(var lane in info.Straight)DescribeLane(text,lane,"straight");
  foreach(var lane in info.Left)DescribeLane(text,lane,"left");
  if(!EntityManager.HasComponent<PrefabRef>(entity))return;
  var prefab=EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab;
  if(!EntityManager.HasBuffer<SubMesh>(prefab))return;
  foreach(var part in EntityManager.GetBuffer<SubMesh>(prefab,true)){
   var mesh=part.m_SubMesh;var map=binding.LeftThree||binding.NormalThree?ThreeMap(mesh,binding.LeftThree):Map(mesh,binding.Four);
   var ps=World.GetExistingSystemManaged<PrefabSystem>();
   text.AppendLine("DIRECTIONAL_MESH "+(ps.TryGetPrefab<RenderPrefab>(mesh,out var p)?p.name:mesh.ToString())+" slots="+string.Join(",",map));
   if(p!=null&&p.TryGet<EmissiveProperties>(out var props)&&props.m_MultiLights!=null){
    int count=binding.LeftThree||binding.NormalThree?3:Mod.Settings.SecondHead?8:4;
    for(int slot=0;slot<props.m_MultiLights.Count;slot++){
     var light=props.m_MultiLights[slot];
     text.AppendLine("DIRECTIONAL_SLOT slot="+slot+" layer="+light.layerId+" purpose="+light.purpose+" suppressExtraGreen="+(map[0]>=0&&DirectionalSlotRules.SuppressExtraGreen(slot,light.purpose==EmissiveProperties.Purpose.TrafficLight_Green,map,count)));
    }
   }
  }
 }
 private void DescribeLane(StringBuilder text,Entity lane,string movement){
  if(!Live(lane)||!EntityManager.HasComponent<Game.Net.LaneSignal>(lane)){text.AppendLine("DIRECTIONAL_LANE "+lane+" missing");return;}
  var state=EntityManager.GetComponentData<Game.Net.LaneSignal>(lane);
  text.AppendLine("DIRECTIONAL_LANE "+lane+" movement="+movement+" mask="+state.m_GroupMask+" live="+state.m_Signal);
 }
}
}
