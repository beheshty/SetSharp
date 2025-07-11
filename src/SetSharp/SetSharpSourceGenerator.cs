using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SetSharp.CodeGeneration;
using SetSharp.Helpers;
using SetSharp.ModelBuilder;
using SetSharp.Models;
using System.Text;

namespace SetSharp
{
    [Generator]
    public class SetSharpSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<AdditionalText> textFiles =
                context.AdditionalTextsProvider.Where(static file => Path.GetFileName(file.Path).Equals("appsettings.json"));

            IncrementalValuesProvider<(List<SettingClassInfo>? classes, Diagnostic? diagnostic)> modelProvider =
                textFiles.Select((text, cancellationToken) =>
                {
                    var content = text.GetText(cancellationToken);
                    if (content is null)
                    {
                        return (null, null);
                    }

                    try
                    {
                        var json = SetSharpJsonParser.Parse(content.ToString());
                        var modelBuilder = new ConfigurationModelBuilder();
                        var classes = modelBuilder.BuildFrom(json);
                        return (classes, (Diagnostic?)null);
                    }
                    catch (Exception e)
                    {
                        var diagnostic = Diagnostic.Create(
                            new DiagnosticDescriptor("CG001", "App settings generation failed", e.ToString(), "Error", DiagnosticSeverity.Error, true),
                            Location.None);
                        return ((List<SettingClassInfo>?)null, diagnostic);
                    }
                });

            context.RegisterSourceOutput(modelProvider, (spc, source) =>
            {
                if (source.diagnostic != null)
                {
                    spc.ReportDiagnostic(source.diagnostic);
                }

                if (source.classes != null)
                {
                    var pocoSourceCode = PocoGenerator.Generate(source.classes);
                    spc.AddSource("AppSettings.g.cs", SourceText.From(pocoSourceCode, Encoding.UTF8));
                }
            });

            context.RegisterSourceOutput(modelProvider, (spc, source) =>
            {
                if (source.classes != null && source.classes.Any())
                {
                    var extensionsSourceCode = OptionsPatternGenerator.Generate(source.classes);
                    spc.AddSource("OptionsExtensions.g.cs", SourceText.From(extensionsSourceCode, Encoding.UTF8));
                }
            });
        }
    }
}
