using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SetSharp.CodeGeneration;
using SetSharp.Helpers;
using SetSharp.ModelBuilder;
using SetSharp.Models;
using System.Text;
using SetSharp.Diagnostics;

namespace SetSharp
{
    [Generator]
    public class SetSharpSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<AdditionalText> textFiles =
                context.AdditionalTextsProvider.Where(static file => Path.GetFileName(file.Path).Equals("appsettings.json"));

            IncrementalValuesProvider<(List<SettingClassInfo>? Classes, SetSharpSettings SetSharpOptions, Diagnostic? Diagnostic)> settingsAndOptionsProvider =
                textFiles.Select((text, cancellationToken) =>
                {
                    var content = text.GetText(cancellationToken);
                    if (content is null)
                    {
                        return ((List<SettingClassInfo>?)null, new SetSharpSettings(), null);
                    }

                    try
                    {
                        var json = SetSharpJsonParser.Parse(content.ToString());

                        var setSharpOptions = ReadSetSharpSettings(json);

                        var modelBuilder = new ConfigurationModelBuilder();
                        var classes = modelBuilder.BuildFrom(json);
                        return (classes, setSharpOptions, (Diagnostic?)null);
                    }
                    catch (Exception e)
                    {
                        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ParsingFailedError, Location.None, e.Message);
                        return ((List<SettingClassInfo>?)null, new SetSharpSettings(), diagnostic);
                    }
                });

            var combinedProvider = settingsAndOptionsProvider.Combine(context.CompilationProvider);

            var finalProvider = combinedProvider.Select((source, cancellationToken) =>
            {
                var ((classes, setSharpOptions, diagnostic), compilation) = source;

                // If parsing already failed, just pass the error through.
                if (diagnostic != null)
                {
                    return new SourceGenerationModel(null, setSharpOptions, diagnostic);
                }

                var dependencyDiagnostic = CheckDependencies(compilation, setSharpOptions.OptionPatternGenerationEnabled);

                if (dependencyDiagnostic != null)
                {
                    return new SourceGenerationModel(classes, setSharpOptions, dependencyDiagnostic);
                }

                return new SourceGenerationModel(classes, setSharpOptions, null);
            });

            context.RegisterSourceOutput(finalProvider, (spc, model) =>
            {
                if (model.Diagnostic != null)
                {
                    spc.ReportDiagnostic(model.Diagnostic);
                }

                // Always generate the POCO classes if parsing was successful.
                if (model.Diagnostic is null)
                {
                    if (model.Classes != null)
                    {
                        var pocoSourceCode = PocoGenerator.Generate(model.Classes);
                        spc.AddSource("AppSettings.g.cs", SourceText.From(pocoSourceCode, Encoding.UTF8));
                    }

                    if (model.SetSharpSettings.OptionPatternGenerationEnabled && model.Classes is { Count: > 0 })
                    {
                        var extensionsSourceCode = OptionsPatternGenerator.Generate(model.Classes);
                        spc.AddSource("OptionsExtensions.g.cs", SourceText.From(extensionsSourceCode, Encoding.UTF8));
                    }
                }
            });
        }

        private static SetSharpSettings ReadSetSharpSettings(Dictionary<string, object> json)
        {
            var setSharpOptions = new SetSharpSettings();
            var settingOption = SetSharpJsonReader.Read(json, "SetSharp");
            if (settingOption is not null and Dictionary<string, object> setSharpDict)
            {

                if (setSharpDict.TryGetValue("OptionPatternGenerationEnabled", out var enabledValue)
                    && bool.TryParse(enabledValue?.ToString(), out var parsedBool))
                {
                    setSharpOptions.OptionPatternGenerationEnabled = parsedBool;
                }

            }
            return setSharpOptions;
        }

        /// <summary>
        /// Checks for required dependencies and returns a Diagnostic if any are missing.
        /// </summary>
        /// <returns>A Diagnostic object if a dependency is missing; otherwise, null.</returns>
        private Diagnostic? CheckDependencies(Compilation compilation, bool checkForOptionsPattern)
        {
            if (compilation.GetTypeByMetadataName("Microsoft.Extensions.Configuration.ConfigurationKeyNameAttribute") == null)
            {
                return Diagnostic.Create(DiagnosticDescriptors.MissingBaseDependencyError, Location.None);
            }

            if (compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableList") == null)
            {
                return Diagnostic.Create(DiagnosticDescriptors.MissingBaseDependencyError, Location.None);
            }

            if (!checkForOptionsPattern)
            {
                return null;
            }

            var iServiceCollectionType = compilation.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.IServiceCollection");
            var iConfigurationType = compilation.GetTypeByMetadataName("Microsoft.Extensions.Configuration.IConfiguration");

            if (iServiceCollectionType == null || iConfigurationType == null)
            {
                return Diagnostic.Create(DiagnosticDescriptors.MissingOptionsDependencyError, Location.None);
            }

            var extensionsType = compilation.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.OptionsConfigurationServiceCollectionExtensions");
            if (extensionsType == null)
            {
                return Diagnostic.Create(DiagnosticDescriptors.MissingOptionsDependencyError, Location.None);
            }

            bool hasConfigureMethod = extensionsType.GetMembers("Configure")
                .OfType<IMethodSymbol>()
                .Any(m => m.IsGenericMethod &&
                          m.IsExtensionMethod &&
                          m.Parameters.Length == 2 &&
                          SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, iServiceCollectionType) &&
                          SymbolEqualityComparer.Default.Equals(m.Parameters[1].Type, iConfigurationType));

            return hasConfigureMethod ? null : Diagnostic.Create(DiagnosticDescriptors.MissingOptionsDependencyError, Location.None);
        }
    }


}

