using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Dto.Spatial;
using Google.Protobuf.Collections;

namespace Bitub.Dto.Scene
{
    public static class PrimitiveMeshBuilder
    {
        /// <summary>
        /// Default deviation of angular gradients set to 2 deg.
        /// </summary>
        public const double defaultAngularDeviation = Math.PI * 2 / 180;

        /// <summary>
        /// Default length deviation set to 5/1000.
        /// </summary>
        public const double defaultLengthDeviation = 0.005;

        #region Internals

        // Get the segment height given a radius and partition angle
        private static double GetSag(double radius, double radAngularPartition)
        {
            return radius * (1 - Math.Cos(radAngularPartition / 2));
        }

        // Gets the segment minimum partition angle given a sagment maximum height
        private static double GetAlpha(double radius, double sag)
        {
            return 2 * Math.Acos(1 - sag / radius);
        }

        // Get a XYZ on the perimeter of a circle given a radius and a center
        private static XYZ GetCircleXY(XYZ c, double radius, double rad)
        {
            return new XYZ
            {
                X = c.X + (float)(radius * Math.Cos(rad)),
                Y = c.Y + (float)(radius * Math.Sin(rad)),
                Z = c.Z
            };
        }

        // Get a XYZ on the perimeter of a sphere given radii, center and spherical coordinates
        private static XYZ GetSphereXYZ(XYZ c, double rX, double rY, double rZ, double radAlpha, double radGamma)
        {
            return new XYZ
            {
                X = c.X + (float)(rX * Math.Cos(radAlpha) * Math.Sin(radGamma)),
                Y = c.Y + (float)(rY * Math.Sin(radAlpha) * Math.Sin(radGamma)),
                Z = c.Z + (float)(rZ * Math.Cos(radGamma)) 
            };
        }

        private static IEnumerable<uint[]> GenerateMeshRing(uint offset1, uint offset2, uint cycleSize)
        {
            for (uint k = (uint)offset1; k < cycleSize; ++k)
            {
                yield return new uint[] { k, k + 1, k + offset2 };
                yield return new uint[] { k + 1, k + 1 + offset2, k + offset2 };
            }
        }

        // Creates a new tri mesh body
        private static Body NewTriMeshBody(out Mesh mesh)
        {
            mesh = new Mesh
            {
                Type = FacetType.TriMesh,
                Orient = Orientation.Ccw
            };

            return new Body
            {
                MeshBody = new MeshBody
                {
                    IsShell = true,
                    IsConvex = true,
                    Tess = mesh
                }
            };
        }

        /// <summary>
        /// Computes a sequence of points placed on the perimeter of a circle.
        /// </summary>
        /// <param name="center">The center</param>
        /// <param name="radius">The radius</param>
        /// <param name="radAngularPartition">The angular desired maximum partitioning</param>
        /// <param name="lengthDeviation">The desited maximum length deviation between exact and approximated perimeter</param>
        /// <returns>A sequence of points of length modulo 2 (at least two)</returns>
        private static IEnumerable<XYZ> GetCircleXYZs(XYZ center, double radius, 
            double radAngularPartition, double lengthDeviation)
        {
            if (GetSag(radius, radAngularPartition) > lengthDeviation)
                radAngularPartition = GetAlpha(radius, lengthDeviation);

            var count = Math.Ceiling(Math.PI / radAngularPartition);
            var rad = Math.PI / count;

            for (int k = 0; k < count * 2; ++k)
            {
                yield return GetCircleXY(center, radius, k * rad);
            }
        }
        
        /// <summary>
        /// Creates a new cylinder with a reference point on the center of the circular extrusion base.
        /// </summary>
        /// <param name="height">Extrusion height</param>
        /// <param name="radius">Cylinder radius</param>
        /// <returns>Tuple of body and associated point array</returns>
        private static (Body, PtArray) NewCylinderZ(XYZ center, float height, float radius, 
            double angularDeviation = defaultAngularDeviation, double lengthDeviation = defaultLengthDeviation)
        {
            var ptArray = new PtArray();

            int countA = 0;
            var points = GetCircleXYZs(center, radius, angularDeviation, lengthDeviation).ToArray();
            ptArray.AppendXYZs(points, ref countA);
            int countB = 0;
            points.ForEach(p => p.Z += height);
            ptArray.AppendXYZs(points, ref countB);

            Mesh mesh;
            var body = NewTriMeshBody(out mesh);
            for (uint k = 0; k < countA; ++k)
            {
                mesh.Vertex.Add(k);
                mesh.Vertex.Add(k + 1);
                mesh.Vertex.Add(k + (uint)countA);

                mesh.Vertex.Add(k + 1);
                mesh.Vertex.Add(k + (uint)countA + 1);
                mesh.Vertex.Add(k);
            }
            return (body, ptArray);
        }

        private static (Body, PtArray) NewEllipsoid(XYZ center, float radiusXY, float radiusZ, 
            double angularDeviation = defaultAngularDeviation, double lengthDeviation = defaultLengthDeviation)
        {
            var ptArray = new PtArray();

            double gamma = angularDeviation;
            if (GetSag(radiusZ, gamma) > lengthDeviation)
                gamma = GetAlpha(radiusZ, lengthDeviation);

            double alpha = angularDeviation;
            if (GetSag(radiusXY, alpha) > lengthDeviation)
                alpha = GetAlpha(radiusXY, lengthDeviation);

            int zRings = Math.Max(2, (int)Math.Ceiling(Math.PI / gamma));
            gamma = Math.PI / zRings;

            int xySegments = (int)Math.Ceiling(Math.PI / alpha) * 2;
            alpha = Math.PI / alpha;

            // Add peak first
            ptArray.Xyz.AddRange(new[] 
            { 
                center.X, 
                center.Y, 
                center.Z + radiusZ 
            });

            int lastIndex = 1;
            for (int k = 1; k < zRings; ++k)
            {
                ptArray.AppendXYZs(
                    Enumerable
                        .Range(0, xySegments)
                        .Select(i => GetSphereXYZ(center, radiusXY, radiusXY, radiusZ, i * alpha, k * gamma)),
                    ref lastIndex);
            }

            // Add bottom last
            ptArray.Xyz.AddRange(new[] 
            { 
                center.X, 
                center.Y, 
                center.Z - radiusZ 
            });

            Mesh mesh;
            var body = NewTriMeshBody(out mesh);

            // Create mesh between rings
            for (int k = 0; k < zRings - 1; ++k)
            {
                GenerateMeshRing(
                    (uint)((k + 1) * xySegments + 1), 
                    (uint)(k * xySegments) + 1, 
                    (uint)xySegments).ForEach(t => mesh.Vertex.AddRange(t));
            }

            // Create fan meshes at top and bottom of ellipsoid
            var lastRingIndex = lastIndex - xySegments;
            for (int k = 0; k < xySegments; ++k)
            {
                // top fan
                mesh.Vertex.AddRange(new uint[] 
                { 
                    0, 
                    (uint)k + 1, 
                    (uint)k + 2 
                });

                // bottom fan
                mesh.Vertex.AddRange(new uint[]
                {
                    (uint)lastIndex, 
                    (uint)(k + lastRingIndex + 1), 
                    (uint)(k + lastRingIndex)
                });
            }


            return (body, ptArray);
        }

        #endregion

        public static Component NewEllipsoidComponent(this ComponentScene scene, XYZ center, double radiusXY, double radiusZ,
            double angularDeviation = defaultAngularDeviation, double lengthDeviation = defaultLengthDeviation)
        {
            throw new NotImplementedException();
        }

        public static Component NewCylinderZComponent(this ComponentScene scene, XYZ baseCenter, double radius, double height,
            double angularDeviation = defaultAngularDeviation, double lengthDeviation = defaultLengthDeviation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Appends a given sequence to the <see cref="PtArray"/> in context.
        /// </summary>
        /// <param name="ptArray">The point array.</param>
        /// <param name="points">Points to add</param>
        /// <param name="count">Reference to counter</param>
        /// <returns>Modified point array in context</returns>
        public static PtArray AppendXYZs(this PtArray ptArray, IEnumerable<XYZ> points, ref int count)
        {
            foreach (var xyz in points)
            {
                ptArray.Xyz.Add(xyz.X);
                ptArray.Xyz.Add(xyz.Y);
                ptArray.Xyz.Add(xyz.Z);
                ++count;

            }
            return ptArray;
        }
    }
}
