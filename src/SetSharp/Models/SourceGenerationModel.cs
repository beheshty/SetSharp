using Microsoft.CodeAnalysis;

namespace SetSharp.Models
{
    internal class SourceGenerationModel
    {
        internal SourceGenerationModel(List<SettingClassInfo>? classes, 
            SetSharpSettings setSharpSettings,
            Diagnostic? diagnostic)
        {
            Classes = classes;
            SetSharpSettings = setSharpSettings;
            Diagnostic = diagnostic;
        }

        internal List<SettingClassInfo>? Classes { get; }
        internal SetSharpSettings SetSharpSettings { get; }
        internal Diagnostic? Diagnostic { get; }
    }
}
