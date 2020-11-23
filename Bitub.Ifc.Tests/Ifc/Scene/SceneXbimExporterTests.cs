using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System.Linq;

using Xbim.Ifc;

using System.IO;
using Google.Protobuf;

using System.Threading.Tasks;

using Bitub.Dto;
using Bitub.Ifc.Tests;

namespace Bitub.Ifc.Scene.Tests
{
    [TestClass]
    public class SceneXbimExporterTests : BaseTests<SceneXbimExporterTests>
    {
        private async Task DoIfcModelExport(string fileName, IfcSceneExportSettings settings)
        {
            IfcSceneExportSummary result;
            using (var store = IfcStore.Open(fileName))
            {
                var exporter = new IfcSceneExporter(new XbimTesselationContext(LoggerFactory), LoggerFactory);
                exporter.Settings = settings;

                using (var monitor = new CancelableProgressing(true))
                {
                    result = await exporter.Run(store, monitor);
                }
            }

            Assert.IsNotNull(result, "Result exists");
            Assert.IsTrue(result.ComponentCache.Count > 0, "There are exported components");
            Assert.IsTrue(result.ComponentCache.Values
                .All(c => c.Representations.SelectMany(r => r.Bodies).All(b => b.Faces.Count > 0)), "All bodies have faces");
            // Show default values too
            var formatter = new JsonFormatter(JsonFormatter.Settings.Default.WithFormatDefaultValues(true));

            using (var jsonStream = File.CreateText($"{Path.GetFileNameWithoutExtension(fileName)}.json"))
            {
                var json = formatter.Format(result.Scene);
                jsonStream.WriteLine(json);
                jsonStream.Close();
                logger.LogInformation($"JSON example has been written.");
            }

            using (var binStream = File.Create($"{Path.GetFileNameWithoutExtension(fileName)}.scene"))
            {
                var binScene = result.Scene.ToByteArray();
                binStream.Write(binScene, 0, binScene.Length);
                logger.LogInformation($"Binary scene of {binScene.Length} bytes has been written.");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc2x3-Slab-BooleanResult.ifc")]
        public async Task TestBooleanResultCorrectionQuaternion()
        {
            await DoIfcModelExport(
                @"Resources\Ifc2x3-Slab-BooleanResult.ifc",
                new IfcSceneExportSettings { Transforming = SceneTransformationStrategy.Quaternion });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-SampleHouse.ifc")]
        public async Task TestSampleHouseNoCorrectionQuaternion()
        {
            var filter = new Dto.Concept.CanonicalFilter(Dto.Concept.FilterMatchingType.SubOrEquiv, System.StringComparison.OrdinalIgnoreCase);
            filter.Filter.Add(new string[] { "Other", "Category" }.ToQualifier().ToClassifier());

            await DoIfcModelExport(
                @"Resources\Ifc4-SampleHouse.ifc",
                new IfcSceneExportSettings
                {
                    Transforming = SceneTransformationStrategy.Quaternion,
                    FeatureToClassifierFilter = filter
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Storey-With-4Walls.ifc")]
        public async Task TestStoreyWithWallsNoCorrectionQuaternion()
        {
            await DoIfcModelExport(
                @"Resources\Ifc4-Storey-With-4Walls.ifc",
                new IfcSceneExportSettings { Transforming = SceneTransformationStrategy.Quaternion });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Rotated-IfcSite-1st-floor.ifc")]
        public async Task TestSlabIfcSiteRotatedMostExtendedRegionCorrectionQuaternion()
        {
            await DoIfcModelExport(
                @"Resources\Ifc4-Rotated-IfcSite-1st-floor.ifc",
                new IfcSceneExportSettings 
                { 
                    Transforming = SceneTransformationStrategy.Quaternion, 
                    Positioning = ScenePositioningStrategy.MostExtendedRegionCorrection
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Base-Groundfloor.ifc")]
        public async Task TestWallsMostExtendedRegionCorrectionQuaternion()
        {
            await DoIfcModelExport(
                @"Resources\Ifc4-Base-Groundfloor.ifc",
                new IfcSceneExportSettings
                {
                    Transforming = SceneTransformationStrategy.Quaternion,
                    Positioning = ScenePositioningStrategy.MostExtendedRegionCorrection
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Rotated-1st-floor.ifc")]
        public async Task TestSlabMeanTranslationCorrectionMatrix()
        {
            await DoIfcModelExport(
                @"Resources\Ifc4-Rotated-1st-floor.ifc",
                new IfcSceneExportSettings
                {
                    Transforming = SceneTransformationStrategy.Matrix,
                    Positioning = ScenePositioningStrategy.MeanTranslationCorrection
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Multi-Body-House.ifc")]
        public async Task TestMultiBodyHouseTranslationCorrectionQuaternion()
        {
            await DoIfcModelExport(
                @"Resources\Ifc4-Multi-Body-House.ifc",
                new IfcSceneExportSettings
                {
                    Transforming = SceneTransformationStrategy.Quaternion,
                    Positioning = ScenePositioningStrategy.MeanTranslationCorrection                    
                });
        }

    }
}
