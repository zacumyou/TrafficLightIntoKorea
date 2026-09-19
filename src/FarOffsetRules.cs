using System;
namespace TrafficLightIntoKorea {
internal static class FarOffsetRules {
 internal static bool CanPlace(float x,float y,float z)=>!float.IsNaN(x)&&!float.IsNaN(y)&&!float.IsNaN(z)&&x*x+z*z<=3600f&&Math.Abs(y)<=12f; internal static float FarRotation(float value)=>float.IsNaN(value)||float.IsInfinity(value)?0:Math.Max(-90f,Math.Min(90f,value)); internal static float Rotation(float value)=>float.IsNaN(value)||float.IsInfinity(value)?0:Math.Max(-45f,Math.Min(45f,value));
 internal static float Clamp(float value)=>float.IsNaN(value)||float.IsInfinity(value)?0:Math.Max(-3f,Math.Min(3f,value));
}
}
