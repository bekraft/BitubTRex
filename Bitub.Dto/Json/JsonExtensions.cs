using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;

namespace Bitub.Dto.Json
{
    public static class JsonExtensions
    {
        public static readonly Regex JsonWhitespace = new Regex(@"\s(?=([^""]*""[^""]*"")*[^""]*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string ToCompactJson(this string text)
        {
            return JsonWhitespace.Replace(text, string.Empty);
        }
    }
}
