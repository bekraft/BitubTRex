using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        public void Bas64QualifierConverterTests()
        {
            // TODO
        }
    }
}
