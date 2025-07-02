using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace EmberaEngine.SourceGen
{
    [Generator]
    class ComponentFormatterGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var componentTypes = context.CompilationProvider.Select(static (compilation, _) =>
            {
                return GeneratorUtils.GetAllTypes(compilation.GlobalNamespace)
                    .Where(t => t.DeclaredAccessibility == Accessibility.Public && GeneratorUtils.IsDerivedFromComponent("Component", "EmberaEngine.Engine.Components", t))
                    .ToImmutableArray();
            });

            context.RegisterSourceOutput(componentTypes, (spc, types) =>
            {
                foreach (var type in types)
                {
                    var source = GenerateComponentFormatter(type);
                    spc.AddSource($"{type.Name}Formatter.g.cs", SourceText.From(source, Encoding.UTF8));
                }

                var allFormattersSource = GenerateFormatterRegistryPartial(types);
                spc.AddSource("GeneratedFormatters.g.cs", SourceText.From(allFormattersSource, Encoding.UTF8));
            });
        }


        private string GenerateComponentFormatter(INamedTypeSymbol type)
        {

            var namespaceName = "EmberaEngine.Engine.Serializing";
            var typename = type.Name;

            var members = type.GetMembers()
                .Where(m =>
                {
                    if (m is IFieldSymbol field)
                    {
                        if (field.IsStatic || field.DeclaredAccessibility != Accessibility.Public)
                            return false;

                        if (field.Type.TypeKind == TypeKind.Delegate)
                            return false;

                        return true;
                    }

                    if (m is IPropertySymbol prop)
                    {
                        if (prop.IsStatic || prop.IsIndexer ||
                            prop.DeclaredAccessibility != Accessibility.Public ||
                            prop.GetMethod == null || prop.SetMethod == null)
                            return false;

                        if (prop.Type.TypeKind == TypeKind.Delegate)
                            return false;

                        return true;
                    }

                    return false;
                })
                .ToArray();


            var fieldReads = new StringBuilder();
            var fieldWrites = new StringBuilder();

            foreach (var member in members)
            {
                var type1 = member switch
                {
                    IFieldSymbol field => field.Type,
                    IPropertySymbol prop => prop.Type,
                    _ => null
                };

                var name = member.Name;

                var readExpr = GetReaderExpression(type1);
                var writeExpr = GetWriterExpression(type1, $"value.{name}");

                fieldReads.AppendLine($"value.{name} = {readExpr};");
                fieldWrites.AppendLine($"{string.Format(writeExpr, $"value.{name}")};");
            }

            return $$"""

    using MessagePack;
    using MessagePack.Formatters;
    using {{type.ContainingNamespace.ToDisplayString()}};

    namespace {{namespaceName}} {
        public sealed class {{typename}}Formatter : IMessagePackFormatter<{{typename}}>
        {
            public void Serialize(ref MessagePackWriter writer, {{typename}} value, MessagePackSerializerOptions options)
            {
                try
                {
                    writer.WriteArrayHeader({{members.Length + 1}});
                    writer.Write((value.gameObject?.Id ?? Guid.Empty).ToString());
                    {{fieldWrites.ToString().TrimEnd()}}
                }
                catch
                {
                    writer.WriteArrayHeader(0); // Write an empty array if serialization fails
                }
            }

            public {{typename}} Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                try
                {
                    var count = reader.ReadArrayHeader();
                    var guid = reader.ReadString();
                    var value = new {{typename}}();

                    if (SceneSerializer.gameObjectGUIDReference.TryGetValue(guid, out var go))
                        value.gameObject = go;

                    {{fieldReads.ToString().TrimEnd()}}
                    return value;
                }
                catch
                {
                    return new {{typename}}(); // Return default instance on error
                }
            }
        }
    }

""";


        }

        private string GetWriterExpression(ITypeSymbol type, string valueExpression)
        {
            var typeName = type.ToDisplayString();

            if (_customTypeHandlers.TryGetValue(typeName, out var handler))
                return handler.writeExprFormat;

            return typeName switch
            {
                "int" => $"writer.Write({valueExpression})",
                "float" => $"writer.Write({valueExpression})",
                "double" => $"writer.Write({valueExpression})",
                "bool" => $"writer.Write({valueExpression})",
                "string" => $"writer.Write({valueExpression})",
                "System.Guid" => $"writer.Write({valueExpression})",
                _ => $"MessagePackSerializer.Serialize(ref writer, {valueExpression}, options)"
            };
        }


        private string GetReaderExpression(ITypeSymbol type)
        {
            var typeStr = type.ToDisplayString();

            if (_customTypeHandlers.TryGetValue(typeStr, out var handler))
                return handler.readExpr;

            return type.SpecialType switch
            {
                SpecialType.System_Int32 => "reader.ReadInt32()",
                SpecialType.System_Single => "reader.ReadSingle()",
                SpecialType.System_Double => "reader.ReadDouble()",
                SpecialType.System_Boolean => "reader.ReadBoolean()",
                SpecialType.System_String => "reader.ReadString()",
                _ => $"MessagePackSerializer.Deserialize<{typeStr}>(ref reader, options)"
            };
        }

        private string GenerateFormatterRegistryPartial(ImmutableArray<INamedTypeSymbol> types)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using MessagePack.Formatters;");
            sb.AppendLine("namespace EmberaEngine.Engine.Serializing");
            sb.AppendLine("{");
            sb.AppendLine("    public static class GeneratedFormatterRegistrar");
            sb.AppendLine("    {");
            sb.AppendLine($"       [ModuleInitializer]");
            sb.AppendLine("        public static void RegisterAll()");
            sb.AppendLine("        {");
            sb.AppendLine($"            FormatterRegistry.Formatters.Clear();");
            foreach (var type in types)
            {
                sb.AppendLine($"            FormatterRegistry.Register(new {type.Name}Formatter());");
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }





        private static readonly Dictionary<string, (string readExpr, string writeExprFormat)> _customTypeHandlers = new()
        {
        //    ["OpenTK.Mathematics.Vector2"] = (
        //        "new OpenTK.Mathematics.Vector2(reader.ReadSingle(), reader.ReadSingle())",
        //        """
        //writer.WriteArrayHeader(2);
        //writer.Write({0}.X);
        //writer.Write({0}.Y);
        //"""
        //    ),

        //    ["OpenTK.Mathematics.Vector3"] = (
        //        "new OpenTK.Mathematics.Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())",
        //        """
        //writer.WriteArrayHeader(3);
        //writer.Write({0}.X);
        //writer.Write({0}.Y);
        //writer.Write({0}.Z);
        //"""
        //    ),

        //    ["OpenTK.Mathematics.Vector4"] = (
        //        "new OpenTK.Mathematics.Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())",
        //        """
        //writer.WriteArrayHeader(4);
        //writer.Write({0}.X);
        //writer.Write({0}.Y);
        //writer.Write({0}.Z);
        //writer.Write({0}.W);
        //"""
        //    ),

        //    ["OpenTK.Mathematics.Color4"] = (
        //        "new OpenTK.Mathematics.Color4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())",
        //        """
        //writer.WriteArrayHeader(4);
        //writer.Write({0}.R);
        //writer.Write({0}.G);
        //writer.Write({0}.B);
        //writer.Write({0}.A);
        //"""
        //    ),
        };



    }
}
