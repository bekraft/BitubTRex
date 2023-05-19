using NUnit.Framework;

using System;
using System.Linq;
using System.Text.Json;

using Bitub.Dto.Json;
using Bitub.Dto.BcfApi;

namespace Bitub.Dto.Bcf.Tests
{
    [TestFixture]
    public class BcfJsonTests : TestBase<BcfJsonTests>
    {
        private JsonSerializerOptions jsonSerializerOptions;
        
        public BcfJsonTests() : base()
        { }

        [SetUp]
        public void Setup()
        {
            jsonSerializerOptions = new JsonSerializerOptions().AddBcfApiConverters();
        }

        [Test]
        public void RoundtripProjectActions()
        {
            var projectActions = new BcfAuthorization 
            { 
                ProjectActions = Enum.GetValues(typeof(BcfProjectActions)).Cast<BcfProjectActions>().Aggregate((a, b) => a | b)
            };
            var json = JsonSerializer.Serialize<IBcfProjectAuthorization>(projectActions, jsonSerializerOptions);
            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            var deserializedActions = JsonSerializer.Deserialize<BcfAuthorization>(json, jsonSerializerOptions);
            Assert.IsNotNull(deserializedActions);
            Assert.AreEqual(projectActions.ProjectActions, deserializedActions.ProjectActions);
        }

        [Test]
        public void RoundtripTopicActions()
        {
            var projectActions = new BcfAuthorization 
            { 
                TopicActions = Enum.GetValues(typeof(BcfTopicActions)).Cast<BcfTopicActions>().Aggregate((a, b) => a | b)
            };
            var json = JsonSerializer.Serialize<IBcfTopicAuthorization>(projectActions, jsonSerializerOptions);
            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            var deserializedActions = JsonSerializer.Deserialize<BcfAuthorization>(json, jsonSerializerOptions);
            Assert.IsNotNull(deserializedActions);
            Assert.AreEqual(projectActions.TopicActions, deserializedActions.TopicActions);
        }

        [Test]
        public void RoundtripCommentActions()
        {
            var projectActions = new BcfAuthorization 
            { 
                CommentActions = Enum.GetValues(typeof(BcfCommentActions)).Cast<BcfCommentActions>().Aggregate((a, b) => a | b)
            };
            var json = JsonSerializer.Serialize<IBcfCommentAuthorization>(projectActions, jsonSerializerOptions);
            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            var deserializedActions = JsonSerializer.Deserialize<BcfAuthorization>(json, jsonSerializerOptions);
            Assert.IsNotNull(deserializedActions);
            Assert.AreEqual(projectActions.CommentActions, deserializedActions.CommentActions);
        }

        [Test]
        public void RoundtripProjects()
        {
            var json = GetUtf8TextFrom("Bcf21.project.json").ToCompactJson();
            Assert.IsNotNull(json);

            var project = JsonSerializer.Deserialize<BcfProject>(json, jsonSerializerOptions);
            Assert.IsNotNull(project);
            Assert.AreEqual("F445F4F2-4D02-4B2A-B612-5E456BEF9137", project.Id.ToString().ToUpper());
            Assert.AreEqual("Example project 1", project.Name);
            Assert.AreEqual(BcfProjectActions.Read|BcfProjectActions.CreateDocument|BcfProjectActions.CreateTopic, project.ProjectAuthorization.ProjectActions);

            var jsonWritten = JsonSerializer.Serialize(project, jsonSerializerOptions);
            Assert.AreEqual(json, jsonWritten);
        }


    }
}