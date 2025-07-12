using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SetSharp.CodeGeneration;
using SetSharp.Helpers;
using SetSharp.ModelBuilder;
using SetSharp.Models;
using System.Text;
using Microsoft.CodeAnalysis.Diagnostics;
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

            IncrementalValuesProvider<(List<SettingClassInfo>? Classes, bool GenerateOptions, Diagnostic? Diagnostic)> settingsAndOptionsProvider =
                textFiles.Select((text, cancellationToken) =>
                {
                    var content = text.GetText(cancellationToken);
                    if (content is null)
                    {
                        return ((List<SettingClassInfo>?)null, true, null);
                    }

                    try
                    {
                        var json = SetSharpJsonParser.Parse(content.ToString());

                        bool generateOptions = ReadSetSharpSettingsIfProvided(json);

                        var modelBuilder = new ConfigurationModelBuilder();
                        var classes = modelBuilder.BuildFrom(json);
                        return (classes, generateOptions, (Diagnostic?)null);
                    }
                    catch (Exception e)
                    {
                        var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ParsingFailedError, Location.None, e.Message);
                        return ((List<SettingClassInfo>?)null, true, diagnostic);
                    }
                });

            var combinedProvider = settingsAndOptionsProvider.Combine(context.CompilationProvider);

            var finalProvider = combinedProvider.Select((source, cancellationToken) =>
            {
                var ((classes, generateOptions, diagnostic), compilation) = source;

                // If parsing already failed, just pass the error through.
                if (diagnostic != null)
                {
                    return new SourceGenerationModel(null, false, diagnostic);
                }

                var dependencyDiagnostic = CheckDependencies(compilation, generateOptions);

                if (dependencyDiagnostic != null)
                {
                    return new SourceGenerationModel(classes, generateOptions, dependencyDiagnostic);
                }

                return new SourceGenerationModel(classes, generateOptions, null);
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

                    if (model.GenerateOptions && model.Classes is { Count: > 0 })
                    {
                        var extensionsSourceCode = OptionsPatternGenerator.Generate(model.Classes);
                        spc.AddSource("OptionsExtensions.g.cs", SourceText.From(extensionsSourceCode, Encoding.UTF8));
                    }
                }
            });
        }

        private static bool ReadSetSharpSettingsIfProvided(Dictionary<string, object> json)
        {
            if (json.TryGetValue("SetSharp", out var setSharpObject)
                                            && setSharpObject is Dictionary<string, object> setSharpDict)
            {
                if (setSharpDict.TryGetValue("OptionPatternGenerationEnabled", out var enabledValue)
                    && bool.TryParse(enabledValue?.ToString(), out var parsedBool))
                {
                    return parsedBool;
                }
            }

            // Default to true if the setting is not present.
            return true;
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

