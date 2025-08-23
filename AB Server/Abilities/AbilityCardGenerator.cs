using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace AB_Server.Abilities
{
    [Generator]
    public class AbilityCardGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization needed
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Find all classes that inherit from AbilityCard
            var abilityCardSymbol = context.Compilation.GetTypeByMetadataName("AB_Server.Abilities.AbilityCard");
            if (abilityCardSymbol == null) return;

            var normalAbilities = new List<(string className, int id)>();
            var correlationAbilities = new List<(string className, int id)>();
            var fusionAbilities = new List<(string className, int id)>();

            foreach (var typeSymbol in context.Compilation.GlobalNamespace.GetNamespaceMembers())
            {
                CollectAbilityCards(typeSymbol, abilityCardSymbol, normalAbilities, correlationAbilities, fusionAbilities);
            }

            // Generate the source code
            var source = GenerateSource(normalAbilities, correlationAbilities, fusionAbilities);
            context.AddSource("AbilityCardRegistration.g.cs", source);
        }

        private void CollectAbilityCards(INamespaceSymbol namespaceSymbol, INamedTypeSymbol abilityCardSymbol, 
            List<(string className, int id)> normalAbilities, List<(string className, int id)> correlationAbilities, List<(string className, int id)> fusionAbilities)
        {
            foreach (var member in namespaceSymbol.GetMembers())
            {
                if (member is INamespaceSymbol childNamespace)
                {
                    CollectAbilityCards(childNamespace, abilityCardSymbol, normalAbilities, correlationAbilities, fusionAbilities);
                }
                else if (member is INamedTypeSymbol typeSymbol)
                {
                    if (typeSymbol.IsAbstract || !typeSymbol.InheritsFrom(abilityCardSymbol))
                        continue;

                    var className = typeSymbol.Name;
                    var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();

                    // Handle fusion abilities
                    if (namespaceName.Contains("Fusions") || typeSymbol.BaseType?.Name == "FusionAbility")
                    {
                        var id = ExtractIdFromConstructor(typeSymbol);
                        fusionAbilities.Add((className, id));
                        continue;
                    }

                    if (namespaceName.Contains("Correlations"))
                    {
                        var id = ExtractIdFromConstructor(typeSymbol);
                        correlationAbilities.Add((className, id));
                    }
                    else
                    {
                        // For normal abilities, assign sequential IDs based on discovery order
                        var id = normalAbilities.Count;
                        normalAbilities.Add((className, id));
                    }
                }
            }
        }

        private int ExtractIdFromConstructor(INamedTypeSymbol typeSymbol)
        {
            // Look for constructor declarations
            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IMethodSymbol method && method.MethodKind == MethodKind.Constructor)
                {
                    // Check if this constructor calls base with an ID
                    var syntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                    if (syntax is ConstructorDeclarationSyntax constructorSyntax)
                    {
                        // Look for base constructor call
                        var baseCall = constructorSyntax.Initializer?.ArgumentList?.Arguments;
                        if (baseCall != null && baseCall.Count >= 3)
                        {
                            // The third argument should be the ID
                            var idArgument = baseCall[2];
                            if (idArgument.Expression is LiteralExpressionSyntax literal)
                            {
                                if (int.TryParse(literal.Token.ValueText, out int id))
                                {
                                    return id;
                                }
                            }
                        }
                    }
                }
            }

            // If no ID found in constructor, return -1 (will be handled later)
            return -1;
        }

        private string GenerateSource(List<(string className, int id)> normalAbilities, List<(string className, int id)> correlationAbilities, List<(string className, int id)> fusionAbilities)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// Auto-generated file - DO NOT EDIT");
            sb.AppendLine("using AB_Server.Abilities.Correlations;");
            sb.AppendLine("using AB_Server.Abilities.Fusions;");
            sb.AppendLine();
            sb.AppendLine("namespace AB_Server.Abilities");
            sb.AppendLine("{");
            sb.AppendLine("    partial class AbilityCard");
            sb.AppendLine("    {");
            
            // Generate normal abilities with sequential IDs
            sb.AppendLine("        public static (Func<int, Player, AbilityCard> constructor, Func<Bakugan, bool> validTarget)[] AbilityCtrs =");
            sb.AppendLine("        [");
            
            if (normalAbilities.Count == 0)
            {
                sb.AppendLine("            // No normal abilities found");
            }
            else
            {
                // Sort by ID and generate
                var sortedNormalAbilities = normalAbilities.OrderBy(x => x.id).ToList();
                foreach (var (className, id) in sortedNormalAbilities)
                {
                    sb.AppendLine($"            ((cID, owner) => new {className}(cID, owner, {id}), {className}.HasValidTargets),");
                }
            }
            
            sb.AppendLine("        ];");
            sb.AppendLine();
            
            // Generate correlation abilities with extracted IDs
            sb.AppendLine("        public static Func<int, Player, AbilityCard>[] CorrelationCtrs =");
            sb.AppendLine("        [");
            
            if (correlationAbilities.Count == 0)
            {
                sb.AppendLine("            // No correlation abilities found");
            }
            else
            {
                // Sort by ID and generate
                var sortedCorrelationAbilities = correlationAbilities.Where(x => x.id >= 0).OrderBy(x => x.id).ToList();
                foreach (var (className, id) in sortedCorrelationAbilities)
                {
                    sb.AppendLine($"            (cID, owner) => new {className}(cID, owner),");
                }
            }
            
            sb.AppendLine("        ];");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    partial class FusionAbility");
            sb.AppendLine("    {");
            
            // Generate fusion abilities with extracted IDs
            sb.AppendLine("        public static Func<int, Player, FusionAbility>[] FusionCtrs =");
            sb.AppendLine("        [");
            
            if (fusionAbilities.Count == 0)
            {
                sb.AppendLine("            // No fusion abilities found");
            }
            else
            {
                // Sort by ID and generate
                var sortedFusionAbilities = fusionAbilities.Where(x => x.id >= 0).OrderBy(x => x.id).ToList();
                foreach (var (className, id) in sortedFusionAbilities)
                {
                    sb.AppendLine($"            (cID, owner) => new {className}(cID, owner),");
                }
            }
            
            sb.AppendLine("        ];");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    public static class SymbolExtensions
    {
        public static bool InheritsFrom(this INamedTypeSymbol symbol, INamedTypeSymbol baseType)
        {
            var current = symbol.BaseType;
            while (current != null)
            {
                if (current.Equals(baseType, SymbolEqualityComparer.Default))
                    return true;
                current = current.BaseType;
            }
            return false;
        }
    }
}
