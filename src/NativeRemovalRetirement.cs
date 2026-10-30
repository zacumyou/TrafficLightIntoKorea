using Game.Common;using Game.Tools;using Unity.Entities;
namespace TrafficLightIntoKorea {
internal static class NativeRemovalRetirement {
 internal static bool ShouldPace(EntityManager em,Entity entity){
  // Signal -> junction, or mounted sign -> signal -> junction. No city-wide query.
  for(int i=0;i<4&&em.Exists(entity);i++){
   if(em.HasComponent<Game.Net.Node>(entity))return !em.HasComponent<Game.Net.TrafficLights>(entity)&&!em.HasComponent<Temp>(entity);
   if(!em.HasComponent<Owner>(entity))break;entity=em.GetComponentData<Owner>(entity).m_Owner;
  }
  return false;
 }
}
}
