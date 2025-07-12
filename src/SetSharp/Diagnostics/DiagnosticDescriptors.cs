using Microsoft.CodeAnalysis;

namespace SetSharp.Diagnostics
{
    internal static class DiagnosticDescriptors
    {
        internal static readonly DiagnosticDescriptor ParsingFailedError = new(
           "SSG001", "App settings parsing failed", "{0}", "Error", DiagnosticSeverity.Error, true);

        internal static readonly DiagnosticDescriptor MissingOptionsDependencyError = new(
            "SSG002", "Missing Dependencies", "To generate IOptions extensions, the project must reference 'Microsoft.Extensions.Options.ConfigurationExtensions'. Please install the corresponding NuGet package.", "Usage", DiagnosticSeverity.Error, true);

        internal static readonly DiagnosticDescriptor MissingBaseDependencyError = new(
            "SSG003", "Missing Dependency", "The project must reference 'Microsoft.Extensions.Configuration.Abstractions' for basic functionality. Please install the corresponding NuGet package.", "Usage", DiagnosticSeverity.Error, true);
    }
}
