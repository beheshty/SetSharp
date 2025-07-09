using SetSharp.Helpers;
using System.Text;
using System.Text.RegularExpressions;

namespace SetSharp.CodeGeneration
{
    public class SettingsCodeBuilder
    {
        public static string GenerateClasses(string jsonContent, string @namespace = "SetSharp.Configuration")
        {
            var assemblyVersion = typeof(SettingsCodeBuilder).Assembly.GetName().Version.ToString();

            var root = SetSharpJsonParser.Parse(jsonContent);
            var sb = new StringBuilder();
            var queue = new Queue<(string className, Dictionary<string, object> properties)>();
            queue.Enqueue(("RootOptions", root));

            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");

            while (queue.Count > 0)
            {
                var (className, props) = queue.Dequeue();

                sb.AppendLine("    /// <summary>Auto-generated from appsettings.json</summary>");
                sb.AppendLine($"    [System.CodeDom.Compiler.GeneratedCode(\"SetSharp\", \"{assemblyVersion}\")]");
                sb.AppendLine($"    public class {className}");
                sb.AppendLine("    {");

                foreach (var item in props)
                {
                    string type = item.Value switch
                    {
                        string => "string",
                        int => "int",
                        double => "double",
                        bool => "bool",
                        Dictionary<string, object> obj => AddNestedClass(queue, item.Key, obj),
                        List<object> list => InferListType(queue, item.Key, list),
                        _ => "object"
                    };
                    var normalizedName = NormalizeName(item.Key);
                    if(normalizedName != item.Key)
                    {
                        sb.AppendLine($"        [Microsoft.Extensions.Configuration.ConfigurationKeyName(\"{item.Key}\")]");
                    }
                    sb.AppendLine($"        public {type} {NormalizeName(item.Key)} {{ get; set; }}");
                }

                sb.AppendLine("    }");
                sb.AppendLine();
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string AddNestedClass(Queue<(string, Dictionary<string, object>)> queue, string key, Dictionary<string, object> obj)
        {
            var className = $"{NormalizeName(key)}Options";
            queue.Enqueue((className, obj));
            return className;
        }

        private static string InferListType(Queue<(string, Dictionary<string, object>)> queue, string key, List<object> list)
        {
            if (list.Count == 0) return "List<object>";

            var first = list[0];

            return first switch
            {
                string => "List<string>",
                int => "List<int>",
                double => "List<double>",
                bool => "List<bool>",
                Dictionary<string, object> obj => $"List<{AddNestedClass(queue, $"{NormalizeName(key)}Item", obj)}>",
                _ => "List<object>"
            };
        }

        /// <summary>
        /// Cleans a string to make it a valid C# identifier (e.g., for a property name).
        /// </summary>
        /// <param name="input">The raw string to be normalized.</param>
        /// <returns>A valid C# identifier.</returns>
        private static string NormalizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "UnnamedProperty";
            }

            string sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_]", "");

            if (!string.IsNullOrEmpty(sanitized) && char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }

            if (string.IsNullOrEmpty(sanitized))
            {
                return "InvalidNameProperty";
            }

            return char.ToUpper(sanitized[0]) + sanitized.Substring(1);
        }
    }
}
