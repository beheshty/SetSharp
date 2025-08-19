using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SetSharp.CodeGeneration;
using SetSharp.Diagnostics;
using SetSharp.Helpers;
using SetSharp.ModelBuilder;
using SetSharp.Models;
using SetSharp.Providers;
using System.Text;

namespace SetSharp
{
    [Generator]
    public class SetSharpSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var settingsProvider = GeneratorSettingsProvider.GetSettings(context);

            var settingsAndTextsProvider = settingsProvider.Combine(context.AdditionalTextsProvider.Collect());

            var pocoInfoProvider = settingsAndTextsProvider.Select((source, cancellationToken) =>
            {
                var (settings, allTexts) = source;

                var sourceFile = allTexts.FirstOrDefault(text =>
                    Path.GetFileName(text.Path).Equals(settings.SourceFile, StringComparison.OrdinalIgnoreCase));

                if (sourceFile is null)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.SourceFileNotFoundError, Location.None, settings.SourceFile);
                    return new SourceGenerationModel(null, settings, diagnostic);
                }

                var content = sourceFile.GetText(cancellationToken);
                if (content is null)
                {
                    return new SourceGenerationModel(null, settings, null);
                }

                try
                {
                    var json = SetSharpJsonParser.Parse(content.ToString());
                    var modelBuilder = new ConfigurationModelBuilder();
                    var classes = modelBuilder.BuildFrom(json);
                    return new SourceGenerationModel(classes, settings, null);
                }
                catch (Exception e)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ParsingFailedError, Location.None, e.Message);
                    return new SourceGenerationModel(null, settings, diagnostic);
                }
            });

            var finalProvider = pocoInfoProvider.Combine(context.CompilationProvider);

            context.RegisterSourceOutput(finalProvider, (spc, source) =>
            {
                var (model, compilation) = source;

                if (model.Diagnostic != null)
                {
                    spc.ReportDiagnostic(model.Diagnostic);
                    return;
                }

                var dependencyDiagnostic = CheckDependencies(compilation, model.SetSharpSettings.OptionPatternGenerationEnabled);
                if (dependencyDiagnostic != null)
                {
                    spc.ReportDiagnostic(dependencyDiagnostic);
                    return;
                }

                if (model.Classes != null)
                {
                    var pocoSourceCode = PocoGenerator.Generate(model.Classes);
                    spc.AddSource("AppSettings.g.cs", SourceText.From(pocoSourceCode, Encoding.UTF8));

                    if (model.SetSharpSettings.OptionPatternGenerationEnabled && model.Classes.Count > 0)
                    {
                        var extensionsSourceCode = OptionsPatternGenerator.Generate(model.Classes);
                        spc.AddSource("OptionsExtensions.g.cs", SourceText.From(extensionsSourceCode, Encoding.UTF8));
                    }
                }
            });
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

