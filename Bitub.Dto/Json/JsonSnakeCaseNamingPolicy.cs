using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;

using System.Text.RegularExpressions;

namespace Bitub.Dto.Json
{
    public sealed class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public static readonly Regex lowerUpperCaseMatcher = new Regex("(([a-z]|[0-9])?[A-Z])", RegexOptions.Compiled);

        public override string ConvertName(string name)
        {
            var matches = lowerUpperCaseMatcher.Matches(name);
            var converted = new StringBuilder(name.ToLowerInvariant());
            int pos_delta = 0;
            foreach (Match match in matches)
            {
                if (match.Index > 0)
                {
                    converted.Insert(match.Index + pos_delta, '_');
                    pos_delta += 1;
                }
            }
            return converted.ToString();
        }
    }
}
