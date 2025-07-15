using SetSharp.Models;
using System.Text.RegularExpressions;

namespace SetSharp.ModelBuilder
{
    internal class ConfigurationModelBuilder
    {
        private readonly List<SettingClassInfo> _completedModels = [];

        internal List<SettingClassInfo> BuildFrom(Dictionary<string, object> root)
        {
            var rootModel = new SettingClassInfo { ClassName = "RootOptions", SectionPath = "" };
            ProcessObject(rootModel, root);
            _completedModels.Add(rootModel);

            return _completedModels;
        }

        private void ProcessObject(SettingClassInfo SettingClassInfo, Dictionary<string, object> obj)
        {
            foreach (var item in obj)
            {
                if (string.Equals(item.Key, "SetSharp", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
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
            string HandleNestedObject(Dictionary<string, object> obj)
            {
                var sectionPath = string.IsNullOrEmpty(parentClass.SectionPath) ? key : $"{parentClass.SectionPath}:{key}";
                return CreateNestedClass(sectionPath, key, obj, isFromCollection: false);
            }

            return value switch
            {
                string => "string",
                int => "int",
                long => "long",
                double => "double",
                bool => "bool",
                Dictionary<string, object> obj => HandleNestedObject(obj),
                List<object> list => InferListType(parentClass.SectionPath, key, list),
                _ => "object"
            };
        }

        private string InferListType(string parentSectionPath, string key, List<object> list)
        {
            if (list.Count == 0) return "List<object>";

            string HandleListItemObject(Dictionary<string, object> obj)
            {
                var sectionPath = string.IsNullOrEmpty(parentSectionPath) ? key : $"{parentSectionPath}:{key}";
                var classNameKey = $"{key}Item";
                return CreateNestedClass(sectionPath, classNameKey, obj, isFromCollection: true);
            }

            // Infer list type from the first item
            var firstItem = list[0];
            string listTypeName = firstItem switch
            {
                string => "string",
                int => "int",
                long => "long",
                double => "double",
                bool => "bool",
                Dictionary<string, object> obj => HandleListItemObject(obj),
                _ => "object"
            };

            return $"List<{listTypeName}>";
        }

        private string CreateNestedClass(string sectionPath, string classNameKey, Dictionary<string, object> obj, bool isFromCollection = false)
        {
            var className = $"{NormalizeName(classNameKey)}Options";

            var nestedSettingClassInfo = new SettingClassInfo
            {
                ClassName = className,
                SectionPath = sectionPath,
                IsFromCollection = isFromCollection
            };

            ProcessObject(nestedSettingClassInfo, obj);
            _completedModels.Add(nestedSettingClassInfo);

            return className;
        }

        private string NormalizeName(string input)
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
