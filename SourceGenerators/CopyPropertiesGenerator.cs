using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

[Generator]
public class CopyPropertiesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsClassWithProperties(s),
                transform: static (ctx, _) => GetClassWithProperties(ctx))
            .Where(static m => m is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static bool IsClassWithProperties(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.Members.OfType<PropertyDeclarationSyntax>().Any();
    }

    private static ClassDeclarationSyntax GetClassWithProperties(GeneratorSyntaxContext context)
    {
        return (ClassDeclarationSyntax)context.Node;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> _, SourceProductionContext context)
    {
        var optionsInterface = compilation.GetTypeByMetadataName("BlazorDateRangePicker.IConfigurableOptions");
        if (optionsInterface == null)
            return;

        var source = GenerateCopyPropertiesMethod(optionsInterface);
        context.AddSource($"ConfigExtensions_CopyProperties.g.cs", SourceText.From(source, Encoding.UTF8));
    }
    private static string GenerateCopyPropertiesMethod(INamedTypeSymbol classSymbol)
    {
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var properties = new List<string>();

        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IPropertySymbol property && property.SetMethod != null)
            {
                properties.Add(property.Name);
            }
        }

        var sb = new StringBuilder($@"
using System;

#pragma warning disable BL0005
namespace {namespaceName}
{{
    public static class BlazorDateRangePickerConfigExtensions
    {{
        public static void CopyProperties(this DateRangePickerConfig source, DateRangePicker destination)
        {{
");

        foreach (var property in properties)
        {
            sb.AppendLine($"            if (destination.{property} == null) destination.{property} = source.{property};");
        }

        sb.AppendLine(@"
        }
    }
}
#pragma warning restore BL0005
");

        return sb.ToString();
    }
}
