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
    public class IfcPropertySetRemovalTransformRequestTests : BaseTest<IfcPropertySetRemovalTransformRequestTests>, IProgress<ICancelableProgressState>
    {
        [TestInitialize]
        public void StartUp()
        {
            StartUpLogging();
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Ifc4-Storey-With-4Walls.ifc")]
        public async Task RemovePropertySetTest()
        {
            IfcStore.ModelProviderFactory.UseMemoryModelProvider();
            using (var source = IfcStore.Open(@"Resources\Ifc4-Storey-With-4Walls.ifc"))
            {
                var stampBefore = IfcSchemaValidationStamp.OfModel(source);

                Assert.AreEqual(4, source.Instances
                    .OfType<IIfcPropertySet>()
                    .Where(s => s.Name == "AllplanAttributes")
                    .Count());

                var request = new IfcPropertySetRemovalRequest(this.TestLoggingFactory)
                {
                    BlackListNames = new string[] { "AllplanAttributes" },
                    IsNameMatchingCaseSensitive = false,
                    // Common config
                    IsLogEnabled = true,
                    TargetStoreType = Xbim.IO.XbimStoreType.InMemoryModel,
                    EditorCredentials = EditorCredentials
                };

                var result = await request.Run(source, this);
                if (null != result.Cause)
                    TestLogger?.LogError("Exception: {0}, {1}, {2}", result.Cause, result.Cause.Message, result.Cause.StackTrace);

                Assert.AreEqual(TransformResult.Code.Finished, result.ResultCode);
                Assert.AreEqual(0, result.Target.Instances
                    .OfType<IIfcPropertySet>()
                    .Where(s => s.Name == "AllplanAttributes")
                    .Count());

                var stampAfter = IfcSchemaValidationStamp.OfModel(result.Target);
                Assert.AreEqual(stampBefore, stampAfter);
            }
        }

        public void Report(ICancelableProgressState value)
        {
            TestLogger.LogDebug($"State {value.State}: Percentage = {value.Percentage}; State object = {value.StateObject}");
        }
    }
}
