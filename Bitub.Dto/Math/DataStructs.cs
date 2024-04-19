using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bitub.Dto.Math
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec2
    {
        public float u; 
        public float v;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;
    }
}
