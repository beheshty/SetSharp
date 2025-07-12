using Microsoft.CodeAnalysis;

namespace SetSharp.Models
{
    internal class SourceGenerationModel
    {
        internal SourceGenerationModel(List<SettingClassInfo>? classes, 
            bool generateOptions,
            Diagnostic? diagnostic)
        {
            Classes = classes;
            GenerateOptions = generateOptions;
            Diagnostic = diagnostic;
        }

        internal List<SettingClassInfo>? Classes { get; }
        internal bool GenerateOptions { get; }
        internal Diagnostic? Diagnostic { get; }
    }
}
