namespace SetSharp.Models
{
    internal class SettingClassInfo
    {
        public string ClassName { get; set; }
        public string SectionPath { get; set; }
        public List<SettingPropertyInfo> Properties { get; } = new List<SettingPropertyInfo>();
    }
}
