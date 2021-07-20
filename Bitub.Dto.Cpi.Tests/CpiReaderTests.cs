using NUnit.Framework;

using System.IO;

using Bitub.Dto.Cpi;

namespace Bitub.Dto.Cpi.Tests
{
    public class CpiReaderTests : TestBase<CpiReaderTests>
    {
        [Test]
        public void ReadExample1()
        {
            using (var resource = GetEmbeddedFileStream("Example-1.cpixml"))
            {
                var fixture = new CpiResourceReader(resource);
            
                Assert.IsNotNull(fixture.ProjectID);
                Assert.IsNotNull(fixture.SourceApplication);

                Assert.AreEqual(3, fixture.Contents.Length);

                var dataSection = fixture.ObjectDataSection;
                Assert.IsNotNull(dataSection);

                var objectSection = fixture.ObjectSection;
                Assert.IsNotNull(objectSection);
            }
        }
    }
}