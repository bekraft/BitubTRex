using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json.Serialization;

namespace Bitub.Dto.BcfApi
{
    public class BcfAuthorization : IBcfProjectAuthorization, IBcfTopicAuthorization, IBcfCommentAuthorization
    {
        [JsonPropertyName("project_actions"), JsonConverter(typeof(JsonBcfProjectActionsConverter))]
        public BcfProjectActions ProjectActions { get; set; }
        [JsonPropertyName("topic_actions"), JsonConverter(typeof(JsonBcfTopicActionsConverter))]
        public BcfTopicActions TopicActions { get; set; }
        [JsonPropertyName("comment_actions"), JsonConverter(typeof(JsonBcfCommentActionsConverter))]
        public BcfCommentActions CommentActions { get; set; }
    }
}
