using System;
namespace TrafficLightIntoKorea {internal static class LaneClearance {
 internal static float DistanceSquared(float px,float pz,float ax,float az,float bx,float bz){float dx=bx-ax,dz=bz-az;float t=Math.Max(0,Math.Min(1,((px-ax)*dx+(pz-az)*dz)/Math.Max(.0001f,dx*dx+dz*dz)));float x=px-ax-t*dx,z=pz-az-t*dz;return x*x+z*z;}
}}
