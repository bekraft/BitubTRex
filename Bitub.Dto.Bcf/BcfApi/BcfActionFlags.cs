using System;
using System.Linq;
using System.Text.Json.Serialization;

using Bitub.Dto.Json;

namespace Bitub.Dto.BcfApi
{
    [Flags]
    [JsonConverter(typeof(JsonEnumFlagAsArrayConverter<BcfProjectActions>))]
    public enum BcfProjectActions
    { 
        None = 0,
        Read = 0x01,
        Update = 0x03,
        CreateTopic = 0x05,
        CreateDocument = 0x09
    }

    public interface IBcfProjectAuthorization
    {
        [JsonPropertyName("project_actions"), JsonConverter(typeof(JsonBcfProjectActionsConverter))]        
        BcfProjectActions ProjectActions { get; set; }
    }

    [Flags]
    public enum BcfTopicActions
    {
        None = 0,
        Read = 0x01,
        Update = 0x03,
        UpdateBimSnippet = 0x05,
        UpdateRelatedTopics = 0x09,
        UpdateDocumentReferences = 0x11,
        UpdateFiles = 0x21,
        CreateComment = 0x41,
        CreateViewpoint = 0x81,
        Delete = 0xff
    }

    public interface IBcfTopicAuthorization
    {
        [JsonPropertyName("topic_actions"), JsonConverter(typeof(JsonBcfTopicActionsConverter))]
        BcfTopicActions TopicActions { get; set; }
    }

    [Flags]
    public enum BcfCommentActions
    {
        None = 0,
        Read = 0x01,
        Update = 0x03,
        Delete = 0x0f
    }   

    public interface IBcfCommentAuthorization
    {
        [JsonPropertyName("comment_actions"), JsonConverter(typeof(JsonBcfCommentActionsConverter))]
        BcfCommentActions CommentActions { get; set; }
    }
}
