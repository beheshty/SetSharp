namespace SetSharp.CodeGeneration
{
    internal class ClassGenerationInfo
    {
        public string ClassName { get; }
        public string SectionPath { get; }
        public Dictionary<string, object> Properties { get; }

        public ClassGenerationInfo(string className, string sectionPath, Dictionary<string, object> properties)
        {
            ClassName = className;
            SectionPath = sectionPath;
            Properties = properties;
        }
    }
}
