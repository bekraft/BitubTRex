using Bitub.Dto.Scene;
using Bitub.Dto.Spatial;
using System.Runtime.InteropServices;

namespace Bitub.Dto.Math
{
    /// <summary>
    /// Simple 2-dim vector as math DTO.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec2
    {
        public float u; 
        public float v;
    }

    /// <summary>
    /// Simple 3-dim vector as math DTO.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;
    }

    /// <summary>
    /// Converting between Spatial DTOs and math DTOs
    /// </summary>
    public static class Converters
    {
        public static Vec2 ToVec2(this UV uv) => new Vec2 { u = uv.U, v = uv.V };
        public static Vec3 ToVec3(this XYZ xyz) => new Vec3 { x = xyz.X, y = xyz.Y, z = xyz.Z };

        public static UV ToUV(this Vec2 vec2) => new UV { U = vec2.u, V = vec2.v };
        public static XYZ ToXYZ(this Vec3 vec3) => new XYZ { X = vec3.x, Y = vec3.y, Z = vec3.z };
    }
}
