using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Bitub.Transfer;

using Bitub.Ifc.Transform;
using Bitub.Ifc.Validation;
using Bitub.Ifc.Transform.Requests;
using Bitub.Transfer.Spatial;

namespace Bitub.Ifc.Tests
{
    [TestClass]
    public class IfcPlacementTransformRequestTests : BaseTest<IfcPlacementTransformRequestTests>
    {
        [TestInitialize]
        public void StartUp()
        {
            StartUpLogging();
        }

        [TestMethod]
        public void IfcAxisAlignmentSerializationTest()
        {
            var axis1 = new IfcAxisAlignment()
            {
                SourceReferenceAxis = new IfcAlignReferenceAxis(new XYZ(), new XYZ { X = 1 }),
                TargetReferenceAxis = new IfcAlignReferenceAxis(new XYZ(), new XYZ { Y = 1 })
            };

            axis1.SaveToFile("TestAxisAlignment.xml");

            var axis2 = IfcAxisAlignment.LoadFromFile("TestAxisAlignment.xml");

            Assert.IsNotNull(axis2);
            Assert.IsNotNull(axis2.SourceReferenceAxis);
            Assert.IsTrue(axis2.SourceReferenceAxis.Offset.IsAlmostEqual(axis1.SourceReferenceAxis.Offset, Precision));
            Assert.IsTrue(axis2.SourceReferenceAxis.Target.IsAlmostEqual(axis1.SourceReferenceAxis.Target, Precision));
            Assert.IsNotNull(axis2.TargetReferenceAxis);
            Assert.IsTrue(axis2.TargetReferenceAxis.Offset.IsAlmostEqual(axis1.TargetReferenceAxis.Offset, Precision));
            Assert.IsTrue(axis2.TargetReferenceAxis.Target.IsAlmostEqual(axis1.TargetReferenceAxis.Target, Precision));
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Rotated-1st-floor.ifc")]
        [DeploymentItem(@"Resources\IfcAlignmentTestAxis1.xml")]
        public async Task OffsetShiftAndRotateTest1()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc4-Rotated-1st-floor.ifc"))
            {
                var stampBefore = IfcSchemaValidationStamp.OfModel(source);

                var testConfig = IfcAxisAlignment.LoadFromFile(@"Resources\IfcAlignmentTestAxis1.xml");
                Assert.IsNotNull(testConfig);
                Assert.IsNotNull(testConfig.SourceReferenceAxis);
                Assert.IsNotNull(testConfig.TargetReferenceAxis);

                var request = new IfcPlacementTransformRequest(this.TestLoggerFactory)                
                {
                    AxisAlignment = testConfig,
                    PlacementStrategy = IfcPlacementStrategy.ChangeRootPlacements,
                    // Common config
                    IsLogEnabled = true,
                    TargetStoreType = Xbim.IO.XbimStoreType.InMemoryModel,
                    EditorCredentials = EditorCredentials
                };

                using (var cp = new CancelableProgressing(true))
                {
                    cp.OnProgressChange += (s, o) => TestLogger.LogDebug($"State {o.State}: Percentage = {o.Percentage}; State object = {o.StateObject}");

                    using (var result = await request.Run(source, cp))
                    {
                        if (null != result.Cause)
                            TestLogger?.LogError("Exception: {0}, {1}, {2}", result.Cause, result.Cause.Message, result.Cause.StackTrace);

                        //Assert.AreEqual(TransformResult.Code.Finished, result.ResultCode);
                        // TODO Specific tests

                        var stampAfter = IfcSchemaValidationStamp.OfModel(result.Target);
                        //Assert.AreEqual(stampBefore, stampAfter);

                        result.Target.SaveAsIfc(new FileStream("Ifc4-Rotated-1st-floor-Transformed.ifc", FileMode.Create));
                    }
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-SampleHouse.ifc")]
        [DeploymentItem(@"Resources\IfcAlignmentTestAxis2.xml")]
        public async Task OffsetShiftAndRotateTest2_Change()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc4-SampleHouse.ifc"))
            {
                var stampBefore = IfcSchemaValidationStamp.OfModel(source);

                var testConfig = IfcAxisAlignment.LoadFromFile(@"Resources\IfcAlignmentTestAxis2.xml");
                Assert.IsNotNull(testConfig);
                Assert.IsNotNull(testConfig.SourceReferenceAxis);
                Assert.IsNotNull(testConfig.TargetReferenceAxis);

                var request = new IfcPlacementTransformRequest(this.TestLoggerFactory)
                {
                    AxisAlignment = testConfig,
                    PlacementStrategy = IfcPlacementStrategy.ChangeRootPlacements,
                    // Common config
                    IsLogEnabled = true,
                    TargetStoreType = Xbim.IO.XbimStoreType.InMemoryModel,
                    EditorCredentials = EditorCredentials
                };

                using (var cp = new CancelableProgressing(true))
                {
                    cp.OnProgressChange += (s, o) => TestLogger.LogDebug($"State {o.State}: Percentage = {o.Percentage}; State object = {o.StateObject}");

                    using (var result = await request.Run(source, cp))
                    {
                        if (null != result.Cause)
                            TestLogger?.LogError("Exception: {0}, {1}, {2}", result.Cause, result.Cause.Message, result.Cause.StackTrace);

                        var rootPlacement = result.Target.Instances.OfType<IIfcLocalPlacement>().Where(i => i.PlacementRelTo == null).FirstOrDefault();
                        Assert.IsNotNull(rootPlacement.PlacesObject);
                        Assert.IsTrue(rootPlacement.PlacesObject.Any(), "Root has objects");

                        //Assert.AreEqual(TransformResult.Code.Finished, result.ResultCode);
                        // TODO Specific tests

                        var stampAfter = IfcSchemaValidationStamp.OfModel(result.Target);
                        //Assert.AreEqual(stampBefore, stampAfter);

                        result.Target.SaveAsIfc(new FileStream("Ifc4-SampleHouse-Transformed.ifc", FileMode.Create));
                    }
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-SampleHouse.ifc")]
        [DeploymentItem(@"Resources\IfcAlignmentTestAxis2.xml")]
        public async Task OffsetShiftAndRotateTest2_New()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc4-SampleHouse.ifc"))
            {
                var stampBefore = IfcSchemaValidationStamp.OfModel(source);

                var testConfig = IfcAxisAlignment.LoadFromFile(@"Resources\IfcAlignmentTestAxis2.xml");
                Assert.IsNotNull(testConfig);
                Assert.IsNotNull(testConfig.SourceReferenceAxis);
                Assert.IsNotNull(testConfig.TargetReferenceAxis);

                var request = new IfcPlacementTransformRequest(this.TestLoggerFactory)
                {
                    AxisAlignment = testConfig,
                    PlacementStrategy = IfcPlacementStrategy.NewRootPlacement,
                    // Common config
                    IsLogEnabled = true,
                    TargetStoreType = Xbim.IO.XbimStoreType.InMemoryModel,
                    EditorCredentials = EditorCredentials
                };

                using (var cp = new CancelableProgressing(true))
                {
                    cp.OnProgressChange += (s, o) => TestLogger.LogDebug($"State {o.State}: Percentage = {o.Percentage}; State object = {o.StateObject}");

                    using (var result = await request.Run(source, cp))
                    {
                        if (null != result.Cause)
                            TestLogger?.LogError("Exception: {0}, {1}, {2}", result.Cause, result.Cause.Message, result.Cause.StackTrace);

                        var rootPlacement = result.Target.Instances.OfType<IIfcLocalPlacement>().Where(i => i.PlacementRelTo == null).FirstOrDefault();
                        Assert.IsNotNull(rootPlacement.PlacesObject);
                        Assert.IsFalse(rootPlacement.PlacesObject.Any(), "Root has no objects");

                        //Assert.AreEqual(TransformResult.Code.Finished, result.ResultCode);
                        // TODO Specific tests

                        var stampAfter = IfcSchemaValidationStamp.OfModel(result.Target);
                        //Assert.AreEqual(stampBefore, stampAfter);

                        result.Target.SaveAsIfc(new FileStream("Ifc4-SampleHouse-Transformed.ifc", FileMode.Create));
                    }
                }
            }
        }

        public void Report(ProgressStateToken value)
        {
            TestLogger.LogDebug($"State {value.State}: Percentage = {value.Percentage}; State object = {value.StateObject}");
        }
    }
}
