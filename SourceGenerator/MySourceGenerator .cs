using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Nws.AbpSourceGenerator;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
namespace ConsoleApp
{
    [Generator]
    public class MySourceGenerator : ISourceGenerator
    {
        const string nsPlaceholder = "{{NAMESPACE}}";
        const string clsPlaceholder = "{{CLASSS_NAME}}";
        const string propertiesPlaceholder = "{{PROPERTIES}}";
        const string classTemplate = @$"namespace {nsPlaceholder};

public partial class {clsPlaceholder}
{{
{propertiesPlaceholder}
}}";

        // Tempate for output: public Type Name { get; set; }
        const string propertyTemplate = "    public {0} {1} {{ get; set; }}";


        public void Initialize(GeneratorInitializationContext context)
        {
            // Method intentionally left empty.
        }

        public void Execute(GeneratorExecutionContext context)
        {
            const string nsPlaceholder = "{{NAMESPACE}}";
            const string clsPlaceholder = "{{CLASSS_NAME}}";
            const string propertiesPlaceholder = "{{PROPERTIES}}";
            const string classTemplate = @$"namespace {nsPlaceholder};

public partial class {clsPlaceholder}
{{
{propertiesPlaceholder}
}}";

            // Tempate for output: public Type Name { get; set; }
            const string propertyTemplate = "    public {0} {1} {{ get; set; }}";

            var entityTypeAttribute = typeof(PropertiesFromAttribute<>);
            var compilation = context.Compilation;


            Debugger.Launch();
            //DBG
            var entityClasses = compilation.SyntaxTrees
            .SelectMany(st => st.GetRoot().DescendantNodes())
            .OfType<ClassDeclarationSyntax>()
            .Where(cls => compilation.GetSemanticModel(cls.SyntaxTree).GetDeclaredSymbol(cls)
                .GetAttributes()
                .Any(attr => attr.AttributeClass.MetadataName == entityTypeAttribute.Name));

            foreach (var entityClass in entityClasses)
            {
                var semanticModel = compilation.GetSemanticModel(entityClass.SyntaxTree);
                var entitySymbol = semanticModel.GetDeclaredSymbol(entityClass);

                // Lấy loại entity từ attribute EntityType
                var entityType = entitySymbol.GetAttributes().First(attr => attr.AttributeClass.MetadataName == entityTypeAttribute.Name);
                var proper2 = entityType.ConstructorArguments.Select(x => x.Values.Select(y => y.Value));
                var proper = entityType.NamedArguments
                    .Select(x =>
                    (x.Key, x.Value.Values.Select(x => x.Value.ToString())));
                //foreach (var item in proper)
                //{
                //    var values = item.Values.Select(x => x.Value.ToString());
                //}
                //var entityAtt = entityType.ConstructorArguments[0].Value as INamedTypeSymbol;
                //var properties = entityAtt.GetMembers().OfType<IPropertySymbol>()
                //.Where(prop => prop.DeclaredAccessibility == Accessibility.Public)
                //.ToList();
                //                  Tạo tên của class DTO
                //var dtoClassName = entityType.Name + "Dto";
            }
            //DBG

            IEnumerable<(ISymbol dtoSymbol, ITypeSymbol entitySymbol, List<IEnumerable<string>> accept)> dtoClassToAttributeTypeMappings = compilation.SyntaxTrees
               .SelectMany(st => st.GetRoot().DescendantNodes())
               .OfType<ClassDeclarationSyntax>()
               .Where(
                x => compilation
                    .GetSemanticModel(x.SyntaxTree)
                    .GetDeclaredSymbol(x)
                    .GetAttributes()
                    .Any(attr => attr.AttributeClass.MetadataName == entityTypeAttribute.Name))
               .Select(
                x =>
                {
                    var propertysFromAttribute = compilation
                        .GetSemanticModel(x.SyntaxTree)
                        .GetDeclaredSymbol(x)
                        .GetAttributes()
                        .First(attr => attr.AttributeClass.MetadataName == entityTypeAttribute.Name);
                    return (
                   dtoSymbol: context.Compilation
                       .GetSemanticModel(x.SyntaxTree)
                       .GetDeclaredSymbol(x),
                   entitySymbol: propertysFromAttribute
                       .AttributeClass.TypeArguments.First(),
                   accept: propertysFromAttribute
                       .ConstructorArguments.Select(x => x.IsNull
                       ? null
                       :x.Values.Select(y => y.Value.ToString())).ToList());
                }
                )
               .ToList();
            foreach (var (dtoSymbol, entitySymbol, accept) in dtoClassToAttributeTypeMappings)
            {
                var properties = entitySymbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(prop => prop.DeclaredAccessibility == Accessibility.Public)
                    .ToList();
                var propertyDeclarations = string.Join("\n", properties.Where(properties =>
                {
                    //Chạy Logic của Onlies
                    if (accept[1] != null && accept[1].Count() > 0)
                    {
                        foreach (var itemProper in accept[1])
                        {
                            return itemProper == properties.MetadataName;

                        }
                    }
                    //Chạy Logic của Ignore
                    else
                    {
                        foreach (var itemProper in accept[0])
                        {
                            return itemProper != properties.MetadataName;
                        }

                    }
                    return false;
                }).Select(
                    x => string.Format(propertyTemplate, x.Type, x.Name)
                ));

                var dtoClassName = dtoSymbol.MetadataName;
                var dtoClassNamespace = dtoSymbol.ContainingNamespace.ToDisplayString();

                var generatedCode = classTemplate
                    .Replace(nsPlaceholder, dtoClassNamespace)
                    .Replace(clsPlaceholder, dtoClassName)
                    .Replace(propertiesPlaceholder, propertyDeclarations);
                var sourceText = SourceText.From(generatedCode, Encoding.UTF8);
                var fileName = $"{dtoClassName}.g.cs";
                context.AddSource(fileName, sourceText);

            }
        }
    }
}
