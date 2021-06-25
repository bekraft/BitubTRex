using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Bitub.Dto.Json;

namespace Bitub.Dto.BcfApi
{
    public class BcfTopic
    {
        [JsonPropertyName("guid")]
        public string Id { get; set; }
        [JsonPropertyName("creation_date")]
        public DateTime CreationDate { get; set; }
        [JsonPropertyName("creation_author")]
        public string CreationAuthor { get; set; }
        [JsonPropertyName("modified_date")]
        public DateTime ModifiedDate { get; set; }
        [JsonPropertyName("modified_author")]
        public string ModifiedAuthor { get; set; }
        [JsonPropertyName("authorization"), JsonConverter(typeof(JsonProxyConverter<BcfAuthorization, IBcfTopicAuthorization>))]
        public IBcfTopicAuthorization Authorization { get; set; }
        [JsonPropertyName("due_date")]
        public DateTime DueDate { get; set; }
        public string topic_type { get; set; }
        public string topic_status { get; set; }
        public object reference_links { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("priority")]
        public string Priority { get; set; }
        public int Index { get; set; }
        public List<object> labels { get; set; }
        public string assigned_to { get; set; }
        public object stage { get; set; }
        public string description { get; set; }
        public object bim_snippet { get; set; }
    }
}