using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System.Linq;

using Xbim.Ifc;

using System.IO;
using Google.Protobuf;

using System.Threading.Tasks;

using Bitub.Transfer;
using Bitub.Ifc.Tests;

namespace Bitub.Ifc.Scene.Tests
{
    [TestClass]
    public class SceneXbimExporterTests : BaseTest<SceneXbimExporterTests>
    {
        [TestInitialize]
        public void StartUp()
        {
            base.StartUpLogging();
        }

        private async Task TestIfcModelExport(string fileName, IfcSceneExportSettings settings)
        {
            IfcSceneExportSummary result;
            using (var store = IfcStore.Open(fileName))
            {
                var exporter = new IfcSceneExporter(new XbimTesselationContext(TestLoggerFactory), TestLoggerFactory);
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
                TestLogger.LogInformation($"JSON example has been written.");
            }

            using (var binStream = File.Create($"{Path.GetFileNameWithoutExtension(fileName)}.scene"))
            {
                var binScene = result.Scene.ToByteArray();
                binStream.Write(binScene, 0, binScene.Length);
                TestLogger.LogInformation($"Binary scene of {binScene.Length} bytes has been written.");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc2x3-Slab-BooleanResult.ifc")]
        public async Task TestBooleanResultCorrectionQuaternion()
        {
            await TestIfcModelExport(
                @"Resources\Ifc2x3-Slab-BooleanResult.ifc",
                new IfcSceneExportSettings { Transforming = SceneTransformationStrategy.Quaternion });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-SampleHouse.ifc")]
        public async Task TestSampleHouseNoCorrectionQuaternion()
        {
            await TestIfcModelExport(
                @"Resources\Ifc4-SampleHouse.ifc", 
                new IfcSceneExportSettings { Transforming = SceneTransformationStrategy.Quaternion });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Storey-With-4Walls.ifc")]
        public async Task TestStoreyWithWallsNoCorrectionQuaternion()
        {
            await TestIfcModelExport(
                @"Resources\Ifc4-Storey-With-4Walls.ifc",
                new IfcSceneExportSettings { Transforming = SceneTransformationStrategy.Quaternion });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Rotated-IfcSite-1st-floor.ifc")]
        public async Task TestSlabIfcSiteRotatedMostExtendedRegionCorrectionQuaternion()
        {
            await TestIfcModelExport(
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
            await TestIfcModelExport(
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
            await TestIfcModelExport(
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
            await TestIfcModelExport(
                @"Resources\Ifc4-Multi-Body-House.ifc",
                new IfcSceneExportSettings
                {
                    Transforming = SceneTransformationStrategy.Quaternion,
                    Positioning = ScenePositioningStrategy.MeanTranslationCorrection                    
                });
        }

    }
}
