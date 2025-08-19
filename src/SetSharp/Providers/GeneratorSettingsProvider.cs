using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SetSharp.Definitions;
using SetSharp.Models;

namespace SetSharp.Providers
{
    internal class GeneratorSettingsProvider
    {
        internal static IncrementalValueProvider<SetSharpSettings> GetSettings(IncrementalGeneratorInitializationContext context)
        {
            return context.AnalyzerConfigOptionsProvider.Select(ParseSettings);
        }

        internal static SetSharpSettings ParseSettings(AnalyzerConfigOptionsProvider provider, CancellationToken cancellationToken)
        {
            var settings = new SetSharpSettings();

            if (provider.GlobalOptions.TryGetValue(MSBuildPropertyKeys.OptionPatternGenerationEnabled, out var enabledValue)
                && bool.TryParse(enabledValue, out var parsedBool))
            {
                settings.OptionPatternGenerationEnabled = parsedBool;
            }

            if (provider.GlobalOptions.TryGetValue(MSBuildPropertyKeys.SourceFile, out var sourceFileValue)
                && !string.IsNullOrWhiteSpace(sourceFileValue))
            {
                settings.SourceFile = sourceFileValue;
            }

            return settings;
        }
    }
}
