using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitub.Dto.Math
{
    public static class Algebra
    {
        #region Vec 3 ops
        #endregion

        #region Vec 3 ops

        public static Vec3 Dot(this Vec3 lhs, Vec3 rhs) => new Vec3 { x = lhs.x * rhs.x, y =  lhs.y * rhs.y, z = lhs.z * rhs.z };

        public static Vec3 Add(this Vec3 lhs, Vec3 rhs) => new Vec3 { x = rhs.x + lhs.x, y = rhs.y + lhs.y, z = rhs.z + lhs.z };

        public static Vec3 Sub(this Vec3 lhs, Vec3 rhs) => new Vec3 { x = lhs.x -  rhs.x, y = lhs.y - rhs.y, z = lhs.z - rhs.z };

        #endregion

    }
}
