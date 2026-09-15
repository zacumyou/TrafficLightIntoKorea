using Game.Objects;using Unity.Entities;using Unity.Mathematics;
namespace TrafficLightIntoKorea {
internal static class SignalVisualTransition {
 internal static TrafficLight Apply(ref KoreanSignalTransition transition,TrafficLight state,World world){
 if(state.m_GroupMask1==0)state.m_State=(TrafficLightState)((int)state.m_State&15);
 if(state.m_GroupMask0==0){transition=default;state.m_State=(TrafficLightState)((int)state.m_State&~15);return state;}
 if(Mod.Settings==null||!Mod.Settings.KoreanTransitions||((int)state.m_State&8)!=0){transition=default;return state;}
 uint frame=world.GetExistingSystemManaged<Game.Simulation.SimulationSystem>().frameIndex;uint duration=(uint)math.max(1,math.round(Mod.Settings.AmberSeconds*60f));
 state.m_State=(TrafficLightState)(((int)state.m_State&~15)|transition.Update((int)state.m_State&15,frame,duration));return state;
 }
}
}
