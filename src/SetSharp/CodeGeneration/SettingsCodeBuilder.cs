using SetSharp.Helpers;
using System.Text;

namespace SetSharp.CodeGeneration
{
    public class SettingsCodeBuilder
    {
        public static string GenerateClasses(string jsonContent, string @namespace = "SetSharp.Configuration")
        {
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
                sb.AppendLine("    [System.CodeDom.Compiler.GeneratedCode(\"SetSharp\", \"1.0.0\")]");
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

        private static string NormalizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            if (CSharpKeywords.Reserved.Contains(input))
                input += "Property";
            input = input.Replace(".", "");
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}
