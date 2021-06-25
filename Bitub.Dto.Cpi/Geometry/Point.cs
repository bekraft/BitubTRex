using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

using Bitub.Dto.Spatial;

namespace Bitub.Dto.Cpi.Geometry
{
    public class Point
    {
        public Point()
        { }

        public Point(int nr, float x, float y, float z)
        {
            Nr = nr;
            X = x;
            Y = y;
            Z = z;
        }

        [XmlAttribute("nr")]
        public int Nr { get; set; }
        [XmlAttribute("x")]
        public float X { get; set; }
        [XmlAttribute("y")]
        public float Y { get; set; }
        [XmlAttribute("z")]
        public float Z { get; set; }
    }

    public static class RefXYZExtensions
    {
        public static XYZ ToXYZ(this Point refsXYZ)
        {
            return new XYZ(refsXYZ.X, refsXYZ.Y, refsXYZ.Z);
        }

        public static Point ToPoint(this XYZ xyz, int nr = 0)
        {
            return new Point(nr, xyz.X, xyz.Y, xyz.Z);
        }
    }
}
