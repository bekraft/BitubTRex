using System;
using System.Linq;
using System.Collections.Generic;

using Bitub.Dto.Scene;

using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;

namespace Bitub.Ifc.Transform
{
    public static class XbimExtensions
    {
        #region General context

        private static double PrecisionClamp(double value, double scale, double precision)
        {
            return PrecisionClamp(value * scale, precision);
        }

        private static double PrecisionClamp(double value, double precision)
        {
            return Math.Abs(value) < precision ? 0 : value;
        }

        public static XbimVector3D ToXbimVector3D(double value)
        {
            return new XbimVector3D(value, value, value);
        }

        public static XbimVector3D ToXbimVector3D(this IEnumerable<double> doubles)
        {
            var xyz = doubles.Take(3).ToArray();
            return new XbimVector3D(xyz[0], xyz[1], xyz[2]);
        }

        public static XbimVector3D ToXbimVector3D(this IEnumerable<float> floats)
        {
            var xyz = floats.Take(3).ToArray();
            return new XbimVector3D(xyz[0], xyz[1], xyz[2]);
        }

        #endregion

        #region XbimPoint3D & XbimVector3D context

        public static XbimVector3D ToXbimVector3D(this XbimPoint3D p)
        {
            return new XbimVector3D(p.X, p.Y, p.Z);
        }

        public static System.Numerics.Vector3 ToNumericVector(this XbimPoint3D p)
        {
            return new System.Numerics.Vector3((float)p.X, (float)p.Y, (float)p.Z);
        }

        public static System.Numerics.Vector3 ToNumericVector(this XbimVector3D v)
        {
            return new System.Numerics.Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static XbimPoint3D ToTranslateOnto(this XbimVector3D v, XbimPoint3D p)
        {
            return new XbimPoint3D(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
        }

        public static XbimPoint3D ToXbimPoint3D(this XbimVector3D v)
        {
            return new XbimPoint3D(v.X, v.Y, v.Z);
        }

        public static IEnumerable<double> ToDouble(this XbimVector3D v, double scale = 1.0)
        {
            return new double[] { v.X * scale, v.Y * scale, v.Z * scale };
        }

        public static IEnumerable<double> ToDoubleMeter(this XbimVector3D v, IModelFactors f)
        {
            var precisionInM = f.Precision * f.LengthToMetresConversionFactor;
            return new double[] 
            { 
                PrecisionClamp(v.X, f.LengthToMetresConversionFactor, precisionInM), 
                PrecisionClamp(v.Y, f.LengthToMetresConversionFactor, precisionInM),
                PrecisionClamp(v.Z, f.LengthToMetresConversionFactor, precisionInM)
            };
        }

        public static IEnumerable<double> ToDoubleModelScale(this XbimVector3D v, IModelFactors f)
        {
            return new double[]
            {
                PrecisionClamp(v.X, f.OneMeter, f.Precision),
                PrecisionClamp(v.Y, f.OneMeter, f.Precision),
                PrecisionClamp(v.Z, f.OneMeter, f.Precision)
            };
        }

        public static IEnumerable<Xbim.Ifc4.MeasureResource.IfcReal> ToIfcReal4(this XbimVector3D v)
        {
            return new Xbim.Ifc4.MeasureResource.IfcReal[] { v.X, v.Y, v.Z };
        }

        /// <summary>
        /// Converts the vector into a length measure enumerable using a scaling value.
        /// </summary>
        /// <param name="v">The vector</param>
        /// <param name="scale">The model scale</param>
        /// <returns>Scaled length measure values</returns>
        public static IEnumerable<Xbim.Ifc4.MeasureResource.IfcLengthMeasure> ToIfcLengthMeasure4(this XbimVector3D v, double scale = 1.0)
        {
            return new Xbim.Ifc4.MeasureResource.IfcLengthMeasure[] { v.X * scale, v.Y * scale, v.Z * scale };
        }

        /// <summary>
        /// Converts the vector into a length measure enumerable using the model's internal length factors.
        /// </summary>
        /// <param name="v">The vector</param>
        /// <param name="f">The model factors</param>
        /// <returns>Length measure values in compliance to the model length unit</returns>
        public static IEnumerable<Xbim.Ifc4.MeasureResource.IfcLengthMeasure> ToIfcLengthMeasure4(this XbimVector3D v, IModelFactors f)
        {
            return new Xbim.Ifc4.MeasureResource.IfcLengthMeasure[]
            {
                PrecisionClamp(v.X, f.OneMeter, f.Precision),
                PrecisionClamp(v.Y, f.OneMeter, f.Precision),
                PrecisionClamp(v.Z, f.OneMeter, f.Precision)
            };
        }

        public static IEnumerable<Xbim.Ifc2x3.MeasureResource.IfcReal> ToIfcReal2x3(this XbimVector3D v)
        {
            return new Xbim.Ifc2x3.MeasureResource.IfcReal[] { v.X, v.Y, v.Z };
        }

        /// <summary>
        /// Converts the vector into a length measure enumerable using a scaling value.
        /// </summary>
        /// <param name="v">The vector</param>
        /// <param name="scale">The model scale</param>
        /// <returns>Scaled length measure values</returns>
        public static IEnumerable<Xbim.Ifc2x3.MeasureResource.IfcLengthMeasure> ToIfcLengthMeasure2x3(this XbimVector3D v, double scale = 1.0)
        {
            return new Xbim.Ifc2x3.MeasureResource.IfcLengthMeasure[] { v.X * scale, v.Y * scale, v.Z * scale };
        }

        /// <summary>
        /// Converts the vector into a length measure enumerable using the model's internal length factors.
        /// </summary>
        /// <param name="v">The vector</param>
        /// <param name="f">The model factors</param>
        /// <returns>Length measure values in compliance to the model length unit</returns>
        public static IEnumerable<Xbim.Ifc2x3.MeasureResource.IfcLengthMeasure> ToIfcLengthMeasure2x3(this XbimVector3D v, IModelFactors f)
        {
            return new Xbim.Ifc2x3.MeasureResource.IfcLengthMeasure[]
            {
                PrecisionClamp(v.X, f.OneMeter, f.Precision),
                PrecisionClamp(v.Y, f.OneMeter, f.Precision),
                PrecisionClamp(v.Z, f.OneMeter, f.Precision)
            };
        }

        #endregion

        #region IIfcCartesianPoint & IIfcDirection context

        public static XbimVector3D ToXbimVector3D(this IIfcCartesianPoint p)
        {
            var mf = p.Model.ModelFactors;
            return new XbimVector3D(
                PrecisionClamp(p.Coordinates[0], mf.Precision), 
                PrecisionClamp(p.Coordinates[1], mf.Precision),
                p.Coordinates.Count > 2 ? PrecisionClamp(p.Coordinates[2], mf.Precision) : 0
            ); 
        }

        public static XbimPoint3D ToXbimPoint3D(this IIfcCartesianPoint p)
        {
            var mf = p.Model.ModelFactors;
            return new XbimPoint3D(
                PrecisionClamp(p.Coordinates[0], mf.Precision),
                PrecisionClamp(p.Coordinates[1], mf.Precision),
                p.Coordinates.Count > 2 ? PrecisionClamp(p.Coordinates[2], mf.Precision) : 0
            );
        }

        public static XbimVector3D ToXbimVector3D(this IIfcDirection d)
        {
            var mf = d.Model.ModelFactors;
            return new XbimVector3D(
                PrecisionClamp(d.DirectionRatios[0], mf.Precision),
                PrecisionClamp(d.DirectionRatios[1], mf.Precision),
                d.DirectionRatios.Count > 2 ? PrecisionClamp(d.DirectionRatios[2], mf.Precision) : 0
            );
        }

        #endregion
    }
}
