using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

[Generator]
public class AddPropertiesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get all interface declarations named "IConfigurableOptions"
        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsIConfigurableOptionsInterface(node),
                transform: static (ctx, _) => GetInterfaceSymbol(ctx))
            .Where(symbol => symbol is not null);

        // Combine with the compilation for context
        var compilationAndInterfaces = context.CompilationProvider.Combine(interfaceDeclarations.Collect());

        // Register source generation
        context.RegisterSourceOutput(compilationAndInterfaces, GenerateSource);
    }

    private static bool IsIConfigurableOptionsInterface(SyntaxNode node) =>
        node is InterfaceDeclarationSyntax interfaceDecl && interfaceDecl.Identifier.Text == "IConfigurableOptions";

    private static INamedTypeSymbol? GetInterfaceSymbol(GeneratorSyntaxContext context)
    {
        var interfaceDecl = (InterfaceDeclarationSyntax)context.Node;
        var model = context.SemanticModel;
        return model.GetDeclaredSymbol(interfaceDecl) as INamedTypeSymbol;
    }

    private static void GenerateSource(SourceProductionContext context, (Compilation, ImmutableArray<INamedTypeSymbol?>) source)
    {
        var (_, interfaces) = source;

        // Extract the IConfigurableOptions interface symbol
        var configurableOptions = interfaces.FirstOrDefault();
        if (configurableOptions is null) return;

        // Generate the DateRangePickerConfig class
        var dateRangePickerConfigSource = $@"
namespace BlazorDateRangePicker
{{
    public partial class DateRangePickerConfig
    {{
{GenerateProperties(configurableOptions)}
    }}
}}";

        context.AddSource("DateRangePickerConfig.g.cs", dateRangePickerConfigSource);

        // Generate the DateRangePicker class
        var dateRangePickerSource = $@"
using Microsoft.AspNetCore.Components;
namespace BlazorDateRangePicker
{{
    public partial class DateRangePicker
    {{
{GenerateProperties(configurableOptions, addParameterAttribute: true)}
    }}
}}";

        context.AddSource("DateRangePicker.g.cs", dateRangePickerSource);
    }

    private static string GenerateProperties(INamedTypeSymbol configurableOptions, bool addParameterAttribute = false)
    {
        // Generate properties from the interface
        var propertiesBuilder = new StringBuilder();
        foreach (var member in configurableOptions.GetMembers().OfType<IPropertySymbol>())
        {
            var typeName = member.Type.ToDisplayString();
            var propertyName = member.Name;

            propertiesBuilder.AppendLine();
            // Add XML documentation if available
            var xmlDocumentation = member.GetDocumentationCommentXml();
            if (!string.IsNullOrEmpty(xmlDocumentation))
            {
                propertiesBuilder.AppendLine($"        /// {xmlDocumentation?.Replace("\n", "\n        /// ")}");
            }

            // Generate property
            if (addParameterAttribute && propertyName == "Attributes")
            {
                propertiesBuilder.AppendLine($"        [Parameter(CaptureUnmatchedValues = true)]");
            }
            else if (addParameterAttribute)
            {
                propertiesBuilder.AppendLine($"        [Parameter]");
            }
            propertiesBuilder.AppendLine($"        public {typeName} {propertyName} {{ get; set; }}");
        }

        var properties = propertiesBuilder.ToString();
        return properties;
    }
}
