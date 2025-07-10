using SetSharp.Models;
using System.Text.RegularExpressions;

namespace SetSharp.ModelBuilder
{
    internal class ConfigurationModelBuilder
    {
        private readonly Queue<SettingClassInfo> _workQueue = new();
        private readonly List<SettingClassInfo> _completedModels = [];

        internal List<SettingClassInfo> BuildFrom(Dictionary<string, object> root)
        {
            var rootModel = new SettingClassInfo { ClassName = "RootOptions", SectionPath = "" };
            ProcessObject(rootModel, root);
            _completedModels.Add(rootModel);

            while (_workQueue.Count > 0)
            {
                var SettingClassInfo = _workQueue.Dequeue();
                _completedModels.Add(SettingClassInfo);
            }

            return _completedModels;
        }

        private void ProcessObject(SettingClassInfo SettingClassInfo, Dictionary<string, object> obj)
        {
            foreach (var item in obj)
            {
                var propertyModel = new SettingPropertyInfo
                {
                    OriginalJsonKey = item.Key,
                    PropertyName = NormalizeName(item.Key),
                    PropertyType = InferType(SettingClassInfo, item.Key, item.Value)
                };
                SettingClassInfo.Properties.Add(propertyModel);
            }
        }

        private string InferType(SettingClassInfo parentClass, string key, object value)
        {
            return value switch
            {
                string => "string",
                int => "int",
                long => "long",
                double => "double",
                bool => "bool",
                Dictionary<string, object> obj => CreateNestedClass(parentClass.SectionPath, key, obj),
                List<object> list => InferListType(parentClass.SectionPath, key, list),
                _ => "object"
            };
        }

        private string InferListType(string parentSectionPath, string key, List<object> list)
        {
            if (list.Count == 0) return "List<object>";

            // Infer list type from the first item
            var firstItem = list[0];
            string listTypeName = firstItem switch
            {
                string => "string",
                int => "int",
                long => "long",
                double => "double",
                bool => "bool",
                Dictionary<string, object> obj => CreateNestedClass(parentSectionPath, $"{key}Item", obj),
                _ => "object"
            };

            return $"List<{listTypeName}>";
        }

        private string CreateNestedClass(string parentSectionPath, string key, Dictionary<string, object> obj)
        {
            var className = $"{NormalizeName(key)}Options";
            var sectionPath = string.IsNullOrEmpty(parentSectionPath) ? key : $"{parentSectionPath}:{key}";

            var nestedSettingClassInfo = new SettingClassInfo
            {
                ClassName = className,
                SectionPath = sectionPath
            };

            ProcessObject(nestedSettingClassInfo, obj);
            _workQueue.Enqueue(nestedSettingClassInfo);

            return className;
        }

        private string NormalizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "UnnamedProperty";

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
