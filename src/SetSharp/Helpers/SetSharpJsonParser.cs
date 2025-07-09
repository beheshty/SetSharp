using System.Text.RegularExpressions;

namespace SetSharp.Helpers
{
    /// <summary>
    /// Simple and minimal JSON parser for configuration source generation.
    /// </summary>
    internal static class SetSharpJsonParser
    {
        /// <summary>
        /// Parses a JSON object string into a dictionary.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>Dictionary representing the JSON object.</returns>
        internal static Dictionary<string, object> Parse(string json)
        {
            json = json.Trim();
            if (!json.StartsWith("{") || !json.EndsWith("}"))
                throw new FormatException("Expected JSON object");

            var result = new Dictionary<string, object>();
            var content = json.Substring(1, json.Length - 2);

            foreach (Match match in Regex.Matches(content, "\"([^\"]+)\":\\s*(\"[^\"]*\"|\\d+\\.?\\d*|true|false|null|\\[.*?\\]|\\{.*?\\})", RegexOptions.Singleline))
            {
                var key = match.Groups[1].Value;
                var raw = match.Groups[2].Value.Trim();

                object value = ParseValue(raw);
                result[key] = value;
            }

            return result;
        }

        private static object ParseValue(string raw)
        {
            if (raw.StartsWith("\"")) return raw.Trim('"');
            if (raw == "true") return true;
            if (raw == "false") return false;
            if (raw == "null") return null!;
            if (raw.StartsWith("{")) return Parse(raw);
            if (raw.StartsWith("[")) return ParseArray(raw);
            if (raw.Contains(".")) return double.Parse(raw);
            if (Regex.IsMatch(raw, @"^\d+$")) return int.Parse(raw);
            return raw;
        }

        internal static List<object> ParseArray(string arrayJson)
        {
            var elements = new List<object>();
            var inner = arrayJson.Trim().TrimStart('[').TrimEnd(']');

            var parts = Regex.Split(inner, "\\s*,\\s*(?![^\\[\\]]*\\]|[^\\{\\}]*\\})");

            foreach (var part in parts)
            {
                var item = part.Trim();
                if (string.IsNullOrEmpty(item)) continue;
                elements.Add(ParseValue(item));
            }

            return elements;
        }
    }
}
