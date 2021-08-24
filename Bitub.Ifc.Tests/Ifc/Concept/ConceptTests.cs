using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xbim.Common.Step21;
using Xbim.Ifc4.Interfaces;

using Bitub.Dto;
using Bitub.Dto.Concept;
using Bitub.Ifc.Tests;

namespace Bitub.Ifc.Concept.Tests
{
    [TestClass]
    public class ConceptTests : TestBase<ConceptTests>
    {
        protected CanonicalFilter existsWallFilter;
        protected CanonicalFilter subOrEquivWallFilter;

        [TestInitialize]
        public void Setup()
        {
            var wallClassifier1 = new[] { "Ifc4", "IfcWall" }.ToQualifier().ToClassifier();
            existsWallFilter = new CanonicalFilter(FilterMatchingType.Exists, StringComparison.OrdinalIgnoreCase);
            existsWallFilter.Filter.Add(wallClassifier1);

            subOrEquivWallFilter = new CanonicalFilter(FilterMatchingType.SubOrEquiv, StringComparison.OrdinalIgnoreCase);
            subOrEquivWallFilter.Filter.Add(wallClassifier1);
        }

        [TestMethod]
        public void IsIfcWallPassingFilter()
        {
            Assert.AreEqual(3, IfcAssemblyScope.SchemaAssemblyScope.Count);
            var products = XbimSchemaVersion.Ifc4.ToImplementingClassifiers<IIfcProduct>().ToArray();

            var walls = products.Where(p => existsWallFilter.IsPassedBy(p, out _) ?? true).ToArray();
            Assert.AreEqual(3, walls.Length);
        }
    }
}
