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

            var queue = new Queue<ClassGenerationInfo>();
            queue.Enqueue(new ClassGenerationInfo("RootOptions", null, root));

            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");

            while (queue.Count > 0)
            {
                var classInfo = queue.Dequeue();

                if (!string.IsNullOrEmpty(classInfo.OriginalJsonKey))
                {
                    sb.AppendLine($"    /// <summary>Represents the '{classInfo.OriginalJsonKey}' section from the configuration.</summary>");
                }
                else
                {
                    sb.AppendLine("    /// <summary>Represents the root of the configuration settings.</summary>");
                }

                sb.AppendLine($"    [System.CodeDom.Compiler.GeneratedCode(\"SetSharp\", \"{assemblyVersion}\")]");
                sb.AppendLine($"    public partial class {classInfo.ClassName}");
                sb.AppendLine("    {");

                foreach (var item in classInfo.Properties)
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
                    if (normalizedName != item.Key)
                    {
                        sb.AppendLine($"        [Microsoft.Extensions.Configuration.ConfigurationKeyName(\"{item.Key}\")]");
                    }
                    sb.AppendLine($"        public {type} {normalizedName} {{ get; set; }}");
                }

                sb.AppendLine("    }");
                sb.AppendLine();
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string AddNestedClass(Queue<ClassGenerationInfo> queue, string originalKey, Dictionary<string, object> obj)
        {
            var className = $"{NormalizeName(originalKey)}Options";
            queue.Enqueue(new ClassGenerationInfo(className, originalKey, obj));
            return className;
        }

        private static string InferListType(Queue<ClassGenerationInfo> queue, string key, List<object> list)
        {
            if (list.Count == 0) return "List<object>";

            var firstItem = list[0];
            string listTypeName = firstItem switch
            {
                string => "string",
                int => "int",
                long => "long",
                double => "double",
                bool => "bool",
                Dictionary<string, object> obj => AddNestedClass(queue, $"{key}Item", obj),
                _ => "object"
            };

            return $"List<{listTypeName}>";
        }

        /// <summary>
        /// Cleans a string to make it a valid C# identifier
        /// </summary>
        private static string NormalizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "UnnamedProperty";
            }

            string sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_]", "");

            if (string.IsNullOrEmpty(sanitized))
            {
                return "InvalidNameProperty";
            }

            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }

            return char.ToUpper(sanitized[0]) + sanitized.Substring(1);
        }
    }
}