namespace Bitub.Dto.Scene
{
    public static class GeometryExtensions
    {
        public static System.Numerics.Quaternion ToNetQuaternion(this Quaternion q)
        {
            return new System.Numerics.Quaternion((float)q.X, (float)q.Y, (float)q.Z, (float)q.W);
        }
    }
}
