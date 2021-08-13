using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System.Linq;

using Xbim.Ifc;

using System.IO;
using Google.Protobuf;

using System.Threading.Tasks;

using Bitub.Dto;
using Bitub.Ifc.Tests;

namespace Bitub.Ifc.Export.Tests
{
    [TestClass]
    public class ComponentModelExportTests : TestBase<ComponentModelExportTests>
    {
        private ExportPreferences testPreferences = new ExportPreferences
        {
            BodyExportType = SceneBodyExportType.FaceBody
        };

        private async Task InternallyRunExport(string fileName, ExportPreferences settings)
        {
            Dto.Scene.ComponentScene result;
            using (var store = IfcStore.Open(fileName))
            {
                var exporter = new ComponentModelExporter(new XbimTesselationContext(LoggerFactory), LoggerFactory);
                exporter.Preferences = settings;

                using (var monitor = new CancelableProgressing(true))
                {
                    result = await exporter.RunExport(store, monitor);
                }
            }

            Assert.IsNotNull(result, "Result exists");
            Assert.IsTrue(result.Components.Count > 0, "There are exported components");
            Assert.IsTrue(result.Components.SelectMany(c => c.Shapes).All(s => null != s.ShapeBody && null != s.Material), "All shapes have bodies and materials");
            Assert.IsTrue(result.ShapeBodies.All(r => r.Bodies.SelectMany(b => r.Bodies).All(b => b.FaceBody.Faces.Count > 0)), "All bodies have faces");
            // Show default values too
            var formatter = new JsonFormatter(JsonFormatter.Settings.Default.WithFormatDefaultValues(true));

            using (var jsonStream = File.CreateText($"{Path.GetFileNameWithoutExtension(fileName)}.json"))
            {
                var json = formatter.Format(result);
                jsonStream.WriteLine(json);
                jsonStream.Close();
                logger.LogInformation($"JSON example has been written.");
            }

            using (var binStream = File.Create($"{Path.GetFileNameWithoutExtension(fileName)}.scene"))
            {
                var binScene = result.ToByteArray();
                binStream.Write(binScene, 0, binScene.Length);
                logger.LogInformation($"Binary scene of {binScene.Length} bytes has been written.");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc2x3-Slab-BooleanResult.ifc")]
        public async Task BooleanResultCorrectionQuaternion()
        {
            await InternallyRunExport(
                @"Resources\Ifc2x3-Slab-BooleanResult.ifc",
                new ExportPreferences(testPreferences) 
                { 
                    Transforming = SceneTransformationStrategy.Quaternion 
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-SampleHouse.ifc")]
        public async Task SampleHouseUserCorrectionQuaternion()
        {
            var filter = new Dto.Concept.CanonicalFilter(Dto.Concept.FilterMatchingType.SubOrEquiv, System.StringComparison.OrdinalIgnoreCase);
            filter.Filter.Add(new string[] { "Other", "Category" }.ToQualifier().ToClassifier());

            await InternallyRunExport(
                @"Resources\Ifc4-SampleHouse.ifc",
                new ExportPreferences(testPreferences)
                {
                    Transforming = SceneTransformationStrategy.Quaternion,
                    Positioning = ScenePositioningStrategy.UserCorrection,
                    UserModelCenter = new Dto.Spatial.XYZ(10, 0, 0)
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Storey-With-4Walls.ifc")]
        public async Task StoreyWithWallsNoCorrectionQuaternion()
        {
            await InternallyRunExport(
                @"Resources\Ifc4-Storey-With-4Walls.ifc",
                new ExportPreferences(testPreferences) 
                { 
                    Transforming = SceneTransformationStrategy.Quaternion 
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Rotated-IfcSite-1st-floor.ifc")]
        public async Task SlabIfcSiteRotatedMostExtendedRegionCorrectionQuaternion()
        {
            await InternallyRunExport(
                @"Resources\Ifc4-Rotated-IfcSite-1st-floor.ifc",
                new ExportPreferences(testPreferences)
                { 
                    Transforming = SceneTransformationStrategy.Quaternion, 
                    Positioning = ScenePositioningStrategy.MostExtendedRegionCorrection
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Base-Groundfloor.ifc")]
        public async Task WallsMostExtendedRegionCorrectionQuaternion()
        {
            await InternallyRunExport(
                @"Resources\Ifc4-Base-Groundfloor.ifc",
                new ExportPreferences(testPreferences)
                {
                    Transforming = SceneTransformationStrategy.Quaternion,
                    Positioning = ScenePositioningStrategy.MostExtendedRegionCorrection
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Rotated-1st-floor.ifc")]
        public async Task SlabMeanTranslationCorrectionMatrix()
        {
            await InternallyRunExport(
                @"Resources\Ifc4-Rotated-1st-floor.ifc",
                new ExportPreferences(testPreferences)
                {
                    Transforming = SceneTransformationStrategy.Matrix,
                    Positioning = ScenePositioningStrategy.MeanTranslationCorrection
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Multi-Body-House.ifc")]
        public async Task MultiBodyHouseTranslationCorrectionQuaternion()
        {
            await InternallyRunExport(
                @"Resources\Ifc4-Multi-Body-House.ifc",
                new ExportPreferences(testPreferences)
                {
                    Transforming = SceneTransformationStrategy.Quaternion,
                    Positioning = ScenePositioningStrategy.MeanTranslationCorrection                    
                });
        }

        [TestMethod]
        [DeploymentItem(@"Resources\mapped-shape-with-transformation.ifc")]
        public async Task MappedRepresentationItem()
        {
            await InternallyRunExport(
                @"Resources\mapped-shape-with-transformation.ifc",
                new ExportPreferences(testPreferences)
                {
                    Transforming = SceneTransformationStrategy.Quaternion,
                    Positioning = ScenePositioningStrategy.NoCorrection
                });
        }
    }
}
