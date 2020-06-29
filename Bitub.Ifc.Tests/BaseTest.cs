using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System.Linq;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Bitub.Ifc;
using Bitub.Ifc.Transform;
using Xbim.Common.Geometry;

namespace Bitub.Ifc.Tests
{
    public class BaseTest<T>
    {
        protected readonly double Precision = 1e-5;

        protected ILoggerFactory TestLoggingFactory { get; private set; }
        protected ILogger TestLogger { get; private set; }

        protected XbimEditorCredentials EditorCredentials = new XbimEditorCredentials
        {
            ApplicationDevelopersName = "Bitub",
            ApplicationFullName = "Testing Bitub.Ifc",
            ApplicationIdentifier = "Bitub.Ifc",
            ApplicationVersion = "1.0",
            EditorsFamilyName = "One",
            EditorsGivenName = "Some",
            EditorsOrganisationName = "Selfemployed"
        };

        protected void StartUpLogging()
        {
            TestLoggingFactory = new LoggerFactory().AddConsole();
            TestLogger = TestLoggingFactory.CreateLogger<T>();
            TestLogger.LogInformation($"Starting {GetType()} ...");
        }

        protected void IsSameArrayElements(object[] asserted, object[] actual)
        {
            foreach(var x in actual)
            {
                if (!asserted.Any(a => a.Equals(x)))
                    Assert.Fail($"{x} hasn't been found in ({string.Join(",", asserted)})");
            }
        }

        protected void AssertIdentityPlacement(IIfcLocalPlacement localPlacement)
        {
            if (localPlacement.RelativePlacement is IIfcAxis2Placement3D a)
            {
                if (null != a.Axis)
                    Assert.IsTrue(a.Axis.ToXbimVector3D().IsEqual(new XbimVector3D(0, 0, 1), Precision), "Axis fails" );
                if (null != a.RefDirection)
                    Assert.IsTrue(a.Axis.ToXbimVector3D().IsEqual(new XbimVector3D(1, 0, 0), Precision), "RefDirection fails");
                if (a.Location is IIfcCartesianPoint p)
                {
                    Assert.IsTrue(p.ToXbimVector3D().IsEqual(XbimVector3D.Zero, Precision), "Location fails");
                }
                else
                {
                    Assert.Fail($"Wrong type Location type '{a.Location?.ExpressType.ExpressName}'");
                }
            }
            else
            {
                Assert.Fail($"Wrong type RelativePlacement type '{localPlacement.RelativePlacement?.ExpressType.ExpressName}'");
            }
        }
    }
}
