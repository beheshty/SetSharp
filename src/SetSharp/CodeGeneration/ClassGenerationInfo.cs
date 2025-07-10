namespace SetSharp.CodeGeneration
{
    internal class ClassGenerationInfo
    {
        public string ClassName { get; }
        public string? OriginalJsonKey { get; }
        public Dictionary<string, object> Properties { get; }

        public ClassGenerationInfo(string className, string? originalJsonKey, Dictionary<string, object> properties)
        {
            ClassName = className;
            OriginalJsonKey = originalJsonKey;
            Properties = properties;
        }
    }
}
