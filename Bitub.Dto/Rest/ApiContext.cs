using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Threading.Tasks;

namespace Bitub.Dto.Rest
{
    public abstract class ApiContext
    {
        #region Internals
        private AuthenticationHeaderValue authHeaderValue;
        private JsonSerializerOptions jsonOptions;

        private readonly object contextMonitor = new object();
        #endregion

        public readonly string clientId;

        protected internal ApiContext(string clientId)
        {
            this.clientId = clientId;
            this.jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                WriteIndented = false
            };
        }

        protected internal ApiContext(ApiContext otherContext)
        {
            this.clientId = otherContext.clientId;
            this.jsonOptions = otherContext.jsonOptions;
        }

        public AuthenticationHeaderValue AuthSchemeAndToken
        {
            get { lock (contextMonitor) return authHeaderValue; }
            set { lock (contextMonitor) authHeaderValue = value; }
        }

        abstract protected internal string ToRootUri(string resouceUri);

        abstract protected internal string ToResourceUri(string resouceUri);

        abstract protected Task<DtoResult<IDtoAuthenticated>> Authenticate();
        
        public string BaseURL { get; protected set; }

        public JsonSerializerOptions JsonOptions
        {
            get { lock (contextMonitor) return jsonOptions; }
            set { lock (contextMonitor) jsonOptions = value; }
        }

        public virtual T FromJson<T>(string json)
        {
            var options = JsonOptions;
            return JsonSerializer.Deserialize<T>(json, options);
        }

        public virtual string ToJson<T>(T dto)
        {
            var options = JsonOptions;
            return JsonSerializer.Serialize(dto, options);
        }
    }
}
