using Bitub.Dto.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bitub.Dto.Tests.Json
{
    [TestClass]
    public class JsonConverterTests
    {
        protected JsonSerializerOptions jsonTestOptions;        

        [TestInitialize]
        public void Setup()
        {
            jsonTestOptions = new JsonSerializerOptions();
            jsonTestOptions.Converters.Add(new JsonQualifierConverter(JsonNamingPolicy.CamelCase));
        }

        [TestMethod]
        public void NamedQualifierConverterTests()
        {
            var namedQualifier = new[] { "A", "test" }.ToQualifier();
            var json = JsonSerializer.Serialize(namedQualifier, jsonTestOptions);
            Assert.IsNotNull(json);

            var result = JsonSerializer.Deserialize<Qualifier>(json, jsonTestOptions);
            Assert.AreEqual(namedQualifier, result);
        }

        [TestMethod]
        public void GuidQualifierConverterTests()
        {
            var guidQualifier = System.Guid.NewGuid().ToQualifier();
            var json = JsonSerializer.Serialize(guidQualifier, jsonTestOptions);
            Assert.IsNotNull(json);

            var result = JsonSerializer.Deserialize<Qualifier>(json, jsonTestOptions);
            Assert.AreEqual(guidQualifier, result);
        }

        [TestMethod]
        public void Bas64QualifierConverterTests()
        {
            // TODO
        }
    }
}
