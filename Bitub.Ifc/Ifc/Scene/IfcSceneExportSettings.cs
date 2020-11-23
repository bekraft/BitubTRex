using System.IO;
using System.Xml.Serialization;

using Bitub.Dto;
using Bitub.Dto.Scene;
using Bitub.Dto.Spatial;
using Bitub.Dto.Concept;

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
        public float MaxBodyOriginCentreBias { get; set; } = float.PositiveInfinity;

        /// <summary>
        /// The count of units per meter. Default is 1.0
        /// </summary>
        public float UnitsPerMeter { get; set; } = 1.0f;

        /// <summary>
        /// Classify by property paths. Each path consists of two or one fragment. Given two will embed values of a <>pset.property</c> will embed
        /// <c>pset.property.value</c> if existing. Given a single fragment <c>property</c> will embed <c>.property.value</c>
        /// </summary>
        public Qualifier[] ClassifyByPropertyPath { get; set; } = new Qualifier[] { };

        /// <summary>
        /// The representation contexts to transfer.
        /// </summary>
        public SceneContext[] UserRepresentationContext { get; set; } = new SceneContext[] { new SceneContext { Name = "Body" } };

        /// <summary>
        /// If set passing features with <see cref="DataOp.Equals"/> assignement will be set as classifier.
        /// </summary>
        public CanonicalFilter FeatureToClassifierFilter { get; set; }

        /// <summary>
        /// If set and a feature is accepted it will be attached to the scene object.
        /// </summary>
        public CanonicalFilterRule FeatureFilterRule { get; set; }

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
