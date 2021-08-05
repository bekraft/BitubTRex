using System;
using System.IO;
using System.Linq;

using System.Xml.Serialization;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Xbim.Common.Geometry;
using Xbim.Common;

namespace Bitub.Ifc.Transform
{
    [XmlRoot(ElementName = "Alignment", Namespace = "https://github.com/bekraft/BitubTRex/Bitub.Ifc.Transform")]
    public class IfcAxisAlignment
    {
        internal readonly static XmlSerializer serializer = new XmlSerializer(typeof(IfcAxisAlignment));

        [XmlElement(ElementName = "Source")]
        public IfcAlignReferenceAxis SourceReferenceAxis { get; set; } = new IfcAlignReferenceAxis();

        [XmlElement(ElementName = "Target")]
        public IfcAlignReferenceAxis TargetReferenceAxis { get; set; } = new IfcAlignReferenceAxis();

        /// <summary>
        /// New axis alignment as idendity transformation.
        /// </summary>
        public IfcAxisAlignment()
        {
        }

        /// <summary>
        /// Copy constructor using the specification of another alignment
        /// </summary>
        /// <param name="axisAlignment"></param>
        public IfcAxisAlignment(IfcAxisAlignment axisAlignment)
        {
            if (null == axisAlignment)
                throw new ArgumentNullException(nameof(axisAlignment));

            SourceReferenceAxis = new IfcAlignReferenceAxis(axisAlignment.SourceReferenceAxis);
            TargetReferenceAxis = new IfcAlignReferenceAxis(axisAlignment.TargetReferenceAxis);
        }

        #region IFC entity creation

        /// <summary>
        /// Appends a new root local placement and changes the transformation graph.
        /// </summary>
        /// <param name="s">The model store</param>
        /// <returns>Added placement</returns>
        public IIfcLocalPlacement NewRootIfcLocalPlacement(IModel s)
        {
            // Compute delta alignment
            var virtualParent = SourceReferenceAxis.TransformAxisTo(TargetReferenceAxis);

            // Generate new root placement
            switch (s.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    return s.NewIfc2x3ObjectPlacementTo(virtualParent);
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return s.NewIfc4ObjectPlacementTo(virtualParent);
                default:
                    throw new ArgumentException($"Unhandled schema version {s.SchemaVersion}");
            }
        }

        /// <summary>
        /// Changes an existing local placement to be aligned to target axis.
        /// </summary>
        /// <param name="placement">A placement to be aligned</param>
        /// <param name="transform">The associated transform in model units</param>
        /// <returns>The given placement with (possible) additional axis components</returns>
        public IIfcLocalPlacement ChangeIfcLocalPlacement(IIfcLocalPlacement placement, XbimMatrix3D transform)
        {
            // Compute delta alignment
            var virtualParent = SourceReferenceAxis.TransformAxisTo(TargetReferenceAxis);
            // Multiply out with selected placement
            var scale = XbimExtensions.ToXbimVector3D(placement.Model.ModelFactors.OneMeter);
            var finalAlignment = new IfcAlignReferenceAxis(transform * virtualParent.ToTransform3D(scale), placement.Model.ModelFactors.OneMeter);

            switch (placement.Model.SchemaVersion)
            {
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc2X3:
                    return (placement as Xbim.Ifc2x3.GeometricConstraintResource.IfcLocalPlacement).ChangeIfc2x3ObjectPlacementTo(finalAlignment, true);
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4:
                case Xbim.Common.Step21.XbimSchemaVersion.Ifc4x1:
                    return (placement as Xbim.Ifc4.GeometricConstraintResource.IfcLocalPlacement).ChangeIfc4ObjectPlacementTo(finalAlignment, true);
                default:
                    throw new ArgumentException($"Unhandled schema version {placement.Model.SchemaVersion}");
            }
        }

        #endregion

        /// <summary>
        /// Loads a complete alignment from file.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>A new alignment based on given file content</returns>
        public static IfcAxisAlignment LoadFromFile(string fileName)
        {
            using(var reader = new FileStream(fileName, FileMode.Open))
            {
                return serializer.Deserialize(reader) as IfcAxisAlignment;                
            }            
        }

        /// <summary>
        /// Saves this alignment to file (overwrite in case of already existing file).
        /// </summary>
        /// <param name="fileName">The file name</param>
        public void SaveToFile(string fileName)
        {            
            using (var writer = new StreamWriter(new FileStream(fileName, FileMode.Create), System.Text.Encoding.UTF8))
            {                
                serializer.Serialize(writer, this);
                writer.Close();
            }
        }

    }
}
