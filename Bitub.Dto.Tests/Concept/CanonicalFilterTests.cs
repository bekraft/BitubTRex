using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Dto.Concept;

namespace Bitub.Dto.Tests.Concept
{
    public class CanonicalFilterTests : BaseTests<CanonicalFilterTests>
    {
        protected CanonicalFilter canonicalFilter;

        [SetUp]
        public void Setup()
        {
            InternallySetup();
        }

        [Test]
        public void FilterIsPassing()
        {

        }
    }
}
