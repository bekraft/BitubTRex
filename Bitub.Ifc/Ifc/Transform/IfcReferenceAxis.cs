using System;
using System.Xml.Serialization;

using Xbim.Common.Geometry;

using Bitub.Dto.Spatial;
using Bitub.Ifc.Export;
using Bitub.Dto.Scene;

namespace Bitub.Ifc.Transform
{
    /// <summary>
    /// IFC alignment reference axis. Embeds a reference coordinate transformation by a start and target point of an
    /// alignment ray pointing to positive X axis. The Y and Z axis are computed respectively using a right-hand (CCW oriented) CRS starting from
    /// a default reference axis given by (<see cref="DefaultReferenceAxis"/>).
    /// </summary>
    public class IfcAlignReferenceAxis
    {
        #region Internals
        private XYZ _offset = new XYZ { X = 0, Y = 0, Z = 0 };
        private XYZ _target = new XYZ { X = 1, Y = 0, Z = 0 };
        #endregion

        /// <summary>
        /// Default reference axis pointing against gravity direction. In general, it is (0,0,1) such that positive 
        /// rotations are directed in CCW sense.
        /// </summary>
        public static XbimVector3D DefaultReferenceAxis = new XbimVector3D(0, 0, 1);

        /// <summary>
        /// The offset as global point given in Meter-scale.
        /// </summary>
        [XmlElement("Start")]
        public XYZ Offset
        {
            get {
                return _offset;
            }
            set {
                _offset = value;
                InternallyUpdateAxis();
            }
        }

        /// <summary>
        /// The derived target point of ray respectively to the offset and aligning ray. Given in Meter-scale.
        /// </summary>
        [XmlElement("End")]
        public XYZ Target
        {
            get {
                return _target;
            }
            set {
                _target = value;
                InternallyUpdateAxis();
            }
        }

        /// <summary>
        /// The alignment ray (local X-Axis direction) in Meter-scale.
        /// </summary>
        [XmlIgnore]
        public XbimVector3D AlignToRay
        {
            get => _target.ToXbimPoint3D() - _offset.ToXbimPoint3D();
        }

        /// <summary>
        /// The normalized alignment ray.
        /// </summary>
        [XmlIgnore]
        public XbimVector3D AlignToAxis 
        { 
            get => AlignToRay.Normalized(); 
        }

        /// <summary>
        /// Normalized tangent ray.
        /// </summary>
        [XmlIgnore]
        public XbimVector3D TangentAxis { get; private set; } = new XbimVector3D(0, 1.0, 0);

        /// <summary>
        /// The perpendicular reference axis.
        /// </summary>
        [XmlIgnore]
        public XbimVector3D ReferenceAxis { get; private set; } = new XbimVector3D(0, 0, 1.0);

        /// <summary>
        /// A new align-reference axis with default 
        /// coordinate system in global X and Z direction, respectively. Initial offset is zero.
        /// </summary>
        public IfcAlignReferenceAxis()
        {
        }

        /// <summary>
        /// Copy constructor wrapping another instance around given reference.
        /// </summary>
        /// <param name="alignReferenceAxis"></param>
        public IfcAlignReferenceAxis(IfcAlignReferenceAxis alignReferenceAxis)
        {
            if (null == alignReferenceAxis)
                throw new ArgumentNullException(nameof(alignReferenceAxis));

            _offset = alignReferenceAxis.Offset;
            _target = alignReferenceAxis.Target;
            ReferenceAxis = alignReferenceAxis.ReferenceAxis;
            TangentAxis = alignReferenceAxis.TangentAxis;
        }

        /// <summary>
        /// A new align-reference axis given a start and end coordinate.
        /// </summary>
        /// <param name="offset">The offset (start)</param>
        /// <param name="target">The alignment target (end)</param>
        public IfcAlignReferenceAxis(XYZ offset, XYZ target)
        {
            Offset = offset;
            Target = target;
            InternallyUpdateAxis();
        }

        /// <summary>
        /// New reference axis wrapping an existing transformation.
        /// </summary>
        /// <param name="m">The 4x4 transformation matrix</param>
        /// <param name="unitsPerMeter">The scale of 1 meter (i.e. 1000 for "mm")</param>
        public IfcAlignReferenceAxis(XbimMatrix3D m, double unitsPerMeter = 1.0f)
        {
            // Set offset and target by translation and EX            
            _offset = m.Translation.ToXYZ(XbimExtensions.ToXbimVector3D(1.0 / unitsPerMeter));
            _target = new XYZ
            {
                X = (float)m.M11 + _offset.X,
                Y = (float)m.M12 + _offset.Y,
                Z = (float)m.M13 + _offset.Z
            };

            // Set Y & Z from transformation directly
            TangentAxis = new XbimVector3D(m.M21, m.M22, m.M23);
            ReferenceAxis = new XbimVector3D(m.M31, m.M32, m.M33);
        }

        // Update tangent and reference axis
        private void InternallyUpdateAxis()
        {
            TangentAxis = DefaultReferenceAxis.CrossProduct(AlignToRay).Normalized();
            ReferenceAxis = AlignToRay.CrossProduct(TangentAxis).Normalized();                       
        }

        /// <summary>
        /// Translates the references axis offset by given delta offset
        /// </summary>
        /// <param name="deltaOffset">The shift</param>
        /// <param name="unitsPerMeter">The units per meter scale</param>
        public void Translate(XbimVector3D deltaOffset, float unitsPerMeter = 1.0f)
        {
            var shift = deltaOffset.ToXYZ(XbimExtensions.ToXbimVector3D(1.0 / unitsPerMeter));
            _offset = _offset.Add(shift);
            _target = _target.Add(shift);
        }

        /// <summary>
        /// Translates the references axis offset by given delta offset
        /// </summary>
        /// <param name="shift">The shift in meter</param>        
        public void Translate(XYZ shift)
        {
            _offset = _offset.Add(shift);
            _target = _target.Add(shift);
        }

        /// <summary>
        /// Computes an alignment axis to compensate the deviation between this axis and
        /// the given reference axis.
        /// </summary>
        /// <param name="targetAxis">Another reference axis</param>
        /// <returns>A new reference which reflects the given alignment axis when applied to transformation chain</returns>
        public IfcAlignReferenceAxis TransformAxisTo(IfcAlignReferenceAxis targetAxis)
        {
            var alignDirection = AlignToAxis;
            var x0 = alignDirection.DotProduct(targetAxis.AlignToAxis);
            var y0 = alignDirection.DotProduct(targetAxis.TangentAxis);
            var z0 = alignDirection.DotProduct(targetAxis.ReferenceAxis);

            // New reference placed at P2-P1 and X1 + d*(-z12) by mirroring at x2
            var deltaAxis = new IfcAlignReferenceAxis(new XYZ { }, new XbimVector3D(x0, -y0, -z0).Normalized().ToXYZ());            
            deltaAxis.Translate( targetAxis._offset.ToXbimPoint3D() - (_offset.ToXbimPoint3D() * deltaAxis.ToTransform3D()) );
            return deltaAxis;
        }

        /// <summary>
        /// Converts the reference axis into a transform matrix given a m-scale.
        /// </summary>
        /// <param name="scale">The conversion factor from 1m to model store unit</param>
        /// <returns>A transform matrix</returns>
        public XbimMatrix3D ToTransform3D(XbimVector3D scale)
        {
            var ex = AlignToAxis;
            return new XbimMatrix3D(
                ex.X, ex.Y, ex.Z, 0,
                TangentAxis.X, TangentAxis.Y, TangentAxis.Z, 0,
                ReferenceAxis.X, ReferenceAxis.Y, ReferenceAxis.Z, 0,
                _offset.X * scale.X, _offset.Y * scale.Y, _offset.Z * scale.Z, 1
            );
        }

        /// <summary>
        /// Converts the reference axis into a transform matrix given a m-scale.
        /// </summary>
        /// <returns>A transform matrix</returns>
        public XbimMatrix3D ToTransform3D()
        {
            var ex = AlignToAxis;
            return new XbimMatrix3D(
                ex.X, ex.Y, ex.Z, 0,
                TangentAxis.X, TangentAxis.Y, TangentAxis.Z, 0,
                ReferenceAxis.X, ReferenceAxis.Y, ReferenceAxis.Z, 0,
                _offset.X, _offset.Y, _offset.Z, 1
            );
        }

    }
}
