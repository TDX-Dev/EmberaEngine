using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmberaEngine.SourceGen
{
    static class GeneratorUtils
    {
        public static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
        {
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol nestedNs)
                {
                    foreach (var type in GetAllTypes(nestedNs))
                    {
                        yield return type;
                    }
                }
                else if (member is INamedTypeSymbol type)
                {
                    yield return type;
                    foreach (var nestedType in GetNestedTypes(type))
                    {
                        yield return nestedType;
                    }
                }
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
        {
            foreach (var nestedType in type.GetTypeMembers())
            {
                yield return nestedType;
                foreach (var deeperNested in GetNestedTypes(nestedType))
                {
                    yield return deeperNested;
                }
            }
        }

        public static bool IsDerivedFromComponent(string baseTypeName, string namespaceName, INamedTypeSymbol symbol)
        {
            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == baseTypeName && baseType.ContainingNamespace.ToDisplayString() == namespaceName)
                    return true;

                baseType = baseType.BaseType;
            }
            return false;
        }



    }
}
