namespace SetSharp.Models
{
    internal class SettingClassInfo
    {
        internal string ClassName { get; set; }
        internal string SectionPath { get; set; }
        internal List<SettingPropertyInfo> Properties { get; set; } = [];
        internal bool IsFromCollection { get; set; }
    }
}
