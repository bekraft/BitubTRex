using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Google.Protobuf;

namespace Bitub.Transfer.TRex
{
    public class JobRequestBuilder
    {
        internal JobRequest JobRequest { get; private set; }

        public JobRequestBuilder(string name)
        {
            JobRequest = new JobRequest { Name = name.ToQualifier() };
        }

        public void FromJson(string jsonContent)
        {
            var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            JobRequest = parser.Parse<JobRequest>(jsonContent);
        }

        public string ToJson()
        {
            var formatter = new JsonFormatter(JsonFormatter.Settings.Default);            
            return formatter.Format(JobRequest);
        }
    }
}
