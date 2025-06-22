using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace EmberaEngine.SourceGen
{
    [Generator]
    public class ComponentRegistryGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var componentTypes = context.CompilationProvider.Select(static (compilation, _) =>
            {
                return GeneratorUtils.GetAllTypes(compilation.GlobalNamespace)
                    .Where(t => t.DeclaredAccessibility == Accessibility.Public && GeneratorUtils.IsDerivedFromComponent("Component", "EmberaEngine.Engine.Components", t))
                    .ToImmutableArray();
            });


            context.RegisterSourceOutput(componentTypes, GenerateComponentRegistryClass);
        }
        private void GenerateComponentRegistryClass(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> types)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using EmberaEngine.Engine.Components;");
            sb.AppendLine("using EmberaEngine.Engine.Utilities;");
            sb.AppendLine("using EmberaEngine.Engine.Serializing;");
            sb.AppendLine();
            sb.AppendLine("namespace EmberaEngine.Generated");
            sb.AppendLine("{");
            sb.AppendLine("    public static class ComponentGeneratedRegisterType");
            sb.AppendLine("    {");
            sb.AppendLine("        [ModuleInitializer]");
            sb.AppendLine("        public static void RegisterAll()");
            sb.AppendLine("        {");

            foreach (var type in types.Distinct(SymbolEqualityComparer.Default))
            {
                var fullTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var formatterName = type.Name + "Formatter";
                var formatterNamespace = "EmberaEngine.Engine.Serializing";
                var formatterFullName = $"{formatterNamespace}.{formatterName}";
                sb.AppendLine($"            ComponentRegistry.Register<{fullTypeName}>(() => new {formatterFullName}());");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource("ComponentGeneratedRegisterType.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

    }


}
