using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SetSharp.CodeGeneration;
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

            IncrementalValuesProvider<(string? source, Diagnostic? diagnostic)> sources =
                textFiles.Select((text, cancellationToken) =>
                {
                    var content = text.GetText(cancellationToken);
                    if (content is null)
                    {
                        return (null, null);
                    }

                    try
                    {
                        var sourceCode = SettingsCodeBuilder.GenerateClasses(content.ToString());
                        return (sourceCode, (Diagnostic?)null);
                    }
                    catch (Exception e)
                    {
                        var diagnostic = Diagnostic.Create(
                            new DiagnosticDescriptor("CG001", "App settings generation failed", e.ToString(), "Error", DiagnosticSeverity.Error, true),
                            Location.None);
                        return ((string?)null, diagnostic);
                    }
                });

            context.RegisterSourceOutput(sources, (spc, source) =>
            {
                if (source.diagnostic != null)
                {
                    spc.ReportDiagnostic(source.diagnostic);
                }

                if (!string.IsNullOrEmpty(source.source))
                {
                    spc.AddSource("AppSettings.g.cs", SourceText.From(source.source, Encoding.UTF8));
                }
            });
        }
    }
}
