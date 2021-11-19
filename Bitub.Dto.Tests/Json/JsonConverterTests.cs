using NUnit.Framework;

using System.Text.Json;

using Bitub.Dto.Spatial;
using Bitub.Dto.Spatial.Json;
using Bitub.Dto.Json;

namespace Bitub.Dto.Tests.Json
{
    public class JsonConverterTests
    {
        protected JsonSerializerOptions jsonTestOptions;        

        [SetUp]
        public void Setup()
        {
            jsonTestOptions = new JsonSerializerOptions();
            jsonTestOptions.Converters.Add(new JsonQualifierConverter(JsonNamingPolicy.CamelCase));
            jsonTestOptions.Converters.Add(new JsonXyzArrayConverter());
        }

        [Test]
        public void NamedQualifierConverterTests()
        {
            var namedQualifier = new[] { "A", "test" }.ToQualifier();
            var json = JsonSerializer.Serialize(namedQualifier, jsonTestOptions);
            Assert.IsNotNull(json);

            var result = JsonSerializer.Deserialize<Qualifier>(json, jsonTestOptions);
            Assert.AreEqual(namedQualifier, result);
        }

        [Test]
        public void GuidQualifierConverterTests()
        {
            var guidQualifier = System.Guid.NewGuid().ToQualifier();
            var json = JsonSerializer.Serialize(guidQualifier, jsonTestOptions);
            Assert.IsNotNull(json);

            var result = JsonSerializer.Deserialize<Qualifier>(json, jsonTestOptions);
            Assert.AreEqual(guidQualifier, result);
        }

        [Test]
        public void Base64QualifierConverterTests()
        {
            Assert.Fail("Not implemented");
        }

        [Test]
        public void XYZConverterTests()
        {
            var fixture = new XYZ { X = 1, Y = 2, Z = 3 };
            var json = JsonSerializer.Serialize(fixture, jsonTestOptions);
            Assert.IsNotNull(json);

            var result = JsonSerializer.Deserialize<XYZ>(json, jsonTestOptions);
            Assert.AreEqual(fixture, result);
        }
    }
}
