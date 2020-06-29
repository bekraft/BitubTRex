using System.IO;
using System.Xml.Serialization;

using Bitub.Transfer.Scene;
using Bitub.Transfer.Spatial;

namespace Bitub.Ifc.Scene
{
    public enum SceneTransformationStrategy
    {
        Quaternion,
        Matrix
    }

    public enum ScenePositioningStrategy
    {
        NoCorrection,
        UserCorrection,
        MeanTranslationCorrection,
        MostPopulatedRegionCorrection,
        SignificantPopulationCorrection,
        MostExtendedRegionCorrection
    }

    [XmlRoot("IfcSceneWriterSettings", Namespace = "https://github.com/bekraft/BitubTRex/Bitub.Ifc.Scene")]
    public sealed class IfcSceneExportSettings
    {
        /// <summary>
        /// Translation correction strategy to be applied while transferring data.
        /// </summary>
        public ScenePositioningStrategy Positioning { get; set; } = default(ScenePositioningStrategy);

        /// <summary>
        /// Transformation strategy (default using global cooridinates).
        /// </summary>
        public SceneTransformationStrategy Transforming { get; set; } = default(SceneTransformationStrategy);

        /// <summary>
        /// Given user translation (in units) a priori. Only applies of positioning is set to UserCorrection.
        /// </summary>
        public XYZ UserModelCenter { get; set; } = new XYZ();

        /// <summary>
        /// Maximum threshold of centre bias of local coordinates relative to local origin.
        /// Percentage measure of greater axis extent of bounding box. By default disabled (positive infinity).
        /// </summary>
        public double MaxBodyOriginCentreBias { get; set; } = double.PositiveInfinity;

        /// <summary>
        /// The count of units per meter. Default is 1.0
        /// </summary>
        public double UnitsPerMeter { get; set; } = 1.0;

        /// <summary>
        /// The pre-classification function to group products into components. By default each product is unique.
        /// </summary>
        [XmlIgnore]
        public XbimEntityQualifierDelegate[] UserProductQualifier { get; set; } = new XbimEntityQualifierDelegate[] { };

        /// <summary>
        /// The representation contexts to transfer.
        /// </summary>
        public SceneContext[] UserRepresentationContext { get; set; } = new SceneContext[] { new SceneContext { Name = "Body" } };

        public IfcSceneExportSettings()
        {
        }

        public IfcSceneExportSettings(IfcSceneExportSettings settings)
        {
            foreach (var prop in GetType().GetProperties())
            {
                prop.SetValue(this, prop.GetValue(settings));
            }
        }

        public static IfcSceneExportSettings ReadFrom(string fileName)
        {
            var serializer = new XmlSerializer(typeof(IfcSceneExportSettings));
            return serializer.Deserialize(File.OpenText(fileName)) as IfcSceneExportSettings;
        }

        public void SaveTo(string fileName)
        {
            var serializer = new XmlSerializer(typeof(IfcSceneExportSettings));
            serializer.Serialize(File.CreateText(fileName), this);
        }
    }
}
