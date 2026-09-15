using System;
namespace TrafficLightIntoKorea {
internal static class FarOffsetRules {
 internal static float Rotation(float value)=>float.IsNaN(value)||float.IsInfinity(value)?0:Math.Max(-45f,Math.Min(45f,value));
 internal static float Clamp(float value)=>float.IsNaN(value)||float.IsInfinity(value)?0:Math.Max(-3f,Math.Min(3f,value));
}
}
