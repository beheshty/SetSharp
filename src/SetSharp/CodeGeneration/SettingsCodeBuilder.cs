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
            queue.Enqueue(new ClassGenerationInfo("RootOptions", "", root));

            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");

            while (queue.Count > 0)
            {
                var classInfo = queue.Dequeue();

                if (!string.IsNullOrEmpty(classInfo.SectionPath))
                {
                    sb.AppendLine($"    /// <summary>Represents the '{classInfo.SectionPath}' section from the configuration.</summary>");
                }
                else
                {
                    sb.AppendLine("    /// <summary>Represents the root of the configuration settings.</summary>");
                }

                sb.AppendLine($"    [System.CodeDom.Compiler.GeneratedCode(\"SetSharp\", \"{assemblyVersion}\")]");
                sb.AppendLine($"    public partial class {classInfo.ClassName}");
                sb.AppendLine("    {");
                if (!string.IsNullOrEmpty(classInfo.SectionPath))
                {
                    sb.AppendLine($"        /// <summary>The configuration section name for this class: \"{classInfo.SectionPath}\"</summary>");
                    sb.AppendLine($"        public const string SectionName = \"{classInfo.SectionPath}\";");
                    sb.AppendLine();
                }

                foreach (var item in classInfo.Properties)
                {
                    var childSectionPath = string.IsNullOrEmpty(classInfo.SectionPath)
                    ? item.Key
                    : $"{classInfo.SectionPath}:{item.Key}";

                    string type = item.Value switch
                    {
                        string => "string",
                        int => "int",
                        double => "double",
                        bool => "bool",
                        Dictionary<string, object> obj => AddNestedClass(queue, childSectionPath, item.Key, obj),
                        List<object> list => InferListType(queue, childSectionPath, item.Key, list),
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

        private static string AddNestedClass(Queue<ClassGenerationInfo> queue, string sectionPath, string originalKey, Dictionary<string, object> obj)
        {
            var className = $"{NormalizeName(originalKey)}Options";
            queue.Enqueue(new ClassGenerationInfo(className, sectionPath, obj));
            return className;
        }

        private static string InferListType(Queue<ClassGenerationInfo> queue, string sectionPath, string key, List<object> list)
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
                Dictionary<string, object> obj => AddNestedClass(queue, sectionPath, $"{key}Item", obj),
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