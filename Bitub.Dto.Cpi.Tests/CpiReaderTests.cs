using NUnit.Framework;

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
            
                Assert.That(fixture.ProjectID, Is.Not.Null);
                Assert.That(fixture.SourceApplication, Is.Not.Null);

                Assert.That(3, Is.EqualTo(fixture.Contents.Length));

                var dataSection = fixture.ObjectDataSection;
                Assert.That(dataSection, Is.Not.Null);

                var objectSection = fixture.ObjectSection;
                Assert.That(objectSection, Is.Not.Null);
            }
        }
    }
}