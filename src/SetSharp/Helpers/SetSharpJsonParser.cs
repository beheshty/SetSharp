using System.Globalization;
using System.Text;
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
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new FormatException("Input JSON cannot be null or empty.");
            }
            int index = 0;
            SkipWhitespace(json, ref index);
            var result = ParseValue(json, ref index) as Dictionary<string, object>;
            SkipWhitespace(json, ref index);

            if (index != json.Length)
            {
                throw new FormatException("Unexpected characters at the end of the JSON string.");
            }

            return result ?? throw new FormatException("The provided JSON is not a valid object.");
        }

        /// <summary>
        /// Parses a JSON array string into a list.
        /// </summary>
        /// <param name="json">The JSON string representing an array.</param>
        /// <returns>A list representing the JSON array.</returns>
        internal static List<object> ParseArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new FormatException("Input JSON cannot be null or empty.");
            }
            int index = 0;
            SkipWhitespace(json, ref index);
            var result = ParseValue(json, ref index) as List<object>;
            SkipWhitespace(json, ref index);

            if (index != json.Length)
            {
                throw new FormatException("Unexpected characters at the end of the JSON string.");
            }

            return result ?? throw new FormatException("The provided JSON is not a valid array.");
        }

        private static object ParseValue(string json, ref int index)
        {
            SkipWhitespace(json, ref index);
            char c = json[index];

            switch (c)
            {
                case '{':
                    return ParseObject(json, ref index);
                case '[':
                    return ParseList(json, ref index);
                case '"':
                    return ParseString(json, ref index);
                case 't':
                case 'f':
                    return ParseBoolean(json, ref index);
                case 'n':
                    return ParseNull(json, ref index);
                default:
                    if (char.IsDigit(c) || c == '-')
                    {
                        return ParseNumber(json, ref index);
                    }
                    throw new FormatException($"Unexpected character '{c}' at index {index}.");
            }
        }

        private static Dictionary<string, object> ParseObject(string json, ref int index)
        {
            var dict = new Dictionary<string, object>();
            index++; // Consume '{'

            while (index < json.Length)
            {
                SkipWhitespace(json, ref index);
                if (json[index] == '}')
                {
                    index++;
                    return dict;
                }

                string key = ParseString(json, ref index);
                SkipWhitespace(json, ref index);

                if (json[index] != ':') throw new FormatException($"Expected ':' after key \"{key}\" at index {index}.");
                index++;

                object value = ParseValue(json, ref index);
                dict[key] = value;
                SkipWhitespace(json, ref index);

                if (json[index] == ',')
                {
                    index++; // Consume the comma
                    SkipWhitespace(json, ref index);

                    // After a comma, a closing brace is illegal (a trailing comma).
                    if (json[index] == '}')
                    {
                        throw new FormatException($"Trailing comma found in object at index {index}.");
                    }
                }
                else if (json[index] == '}')
                {
                    index++;
                    return dict;
                }
                else
                {
                    throw new FormatException($"Expected ',' or '}}' in object at index {index}.");
                }
            }
            throw new FormatException("Unterminated JSON object.");
        }

        private static List<object> ParseList(string json, ref int index)
        {
            var list = new List<object>();
            index++; // Consume '['

            while (index < json.Length)
            {
                SkipWhitespace(json, ref index);
                if (json[index] == ']')
                {
                    index++;
                    return list;
                }

                object value = ParseValue(json, ref index);
                list.Add(value);
                SkipWhitespace(json, ref index);

                if (json[index] == ',')
                {
                    index++; // Consume the comma
                    SkipWhitespace(json, ref index);

                    // After a comma, a closing brace is illegal (a trailing comma).
                    if (json[index] == ']')
                    {
                        throw new FormatException($"Trailing comma found in object at index {index}.");
                    }
                }
                else if (json[index] == ']')
                {
                    index++;
                    return list;
                }
                else
                {
                    throw new FormatException($"Expected ',' or ']' in array at index {index}.");
                }
            }
            throw new FormatException("Unterminated JSON array.");
        }

        private static string ParseString(string json, ref int index)
        {
            var sb = new StringBuilder();
            index++; // Consume '\"'

            while (index < json.Length)
            {
                char c = json[index++];
                if (c == '"')
                {
                    return sb.ToString();
                }
                if (c == '\\')
                {
                    if (index >= json.Length) throw new FormatException("Unterminated escape sequence.");
                    char next = json[index++];
                    switch (next)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (index + 3 >= json.Length) throw new FormatException("Invalid Unicode escape sequence.");
                            string hex = json.Substring(index, 4);
                            sb.Append((char)Convert.ToInt32(hex, 16));
                            index += 4;
                            break;
                        default:
                            throw new FormatException($"Invalid escape sequence: \\{next}");
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            throw new FormatException("Unterminated string literal.");
        }

        private static object ParseNumber(string json, ref int index)
        {
            int startIndex = index;
            while (index < json.Length && "0123456789.-+eE".Contains(json[index]))
            {
                index++;
            }
            string numberStr = json.Substring(startIndex, index - startIndex);

            if (numberStr.Contains(".") || numberStr.ToLower().Contains("e"))
            {
                if (double.TryParse(numberStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                {
                    return d;
                }
            }
            else
            {
                if (int.TryParse(numberStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
                {
                    return i;
                }
                if (long.TryParse(numberStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
                {
                    return l;
                }
            }
            throw new FormatException($"Invalid number format: {numberStr}");
        }

        private static bool ParseBoolean(string json, ref int index)
        {
            if (index + 3 < json.Length && json.Substring(index, 4) == "true")
            {
                index += 4;
                return true;
            }
            if (index + 4 < json.Length && json.Substring(index, 5) == "false")
            {
                index += 5;
                return false;
            }
            throw new FormatException($"Invalid boolean literal at index {index}.");
        }

        private static object? ParseNull(string json, ref int index)
        {
            if (index + 3 < json.Length && json.Substring(index, 4) == "null")
            {
                index += 4;
                return null;
            }
            throw new FormatException($"Invalid null literal at index {index}.");
        }

        private static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }
        }
    }
}
