using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGenerator;
using System;
using System.Linq;
using System.Text;

namespace ConsoleApp
{
    [Generator]
    public class MySourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Method intentionally left empty.
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Xử lý logic để tạo mã nguồn
            // Có thể sử dụng context để truy cập vào thông tin về mã nguồn hiện tại và tạo mã mới
            // Không có gì để khởi tạo
            // Find the main method
            var entityTypeAttribute = typeof(EntityToDtoAttribute);
            var compilation = context.Compilation;

            var entityClasses = compilation.SyntaxTrees
           .SelectMany(st => st.GetRoot().DescendantNodes())
           .OfType<ClassDeclarationSyntax>()
           .Where(cls => compilation.GetSemanticModel(cls.SyntaxTree).GetDeclaredSymbol(cls)
               .GetAttributes()
               .Any(attr => attr.AttributeClass.Name == entityTypeAttribute.Name));

            foreach (var entityClass in entityClasses)
            {
                var semanticModel = compilation.GetSemanticModel(entityClass.SyntaxTree);
                var entitySymbol = semanticModel.GetDeclaredSymbol(entityClass);

                var typeNodeSymbol = context.Compilation
                    .GetSemanticModel(entityClass.SyntaxTree)
                    .GetDeclaredSymbol(entityClass);

                // get the namespace of the entity class
                var entityClassNamespace = typeNodeSymbol.ContainingNamespace?.ToDisplayString() ?? "NoNamespace";

                // give each DTO a name, just suffix the entity class name with "Dto"
                var generatedDtoClassName = typeNodeSymbol.Name;

                var entityType = entitySymbol.GetAttributes()
                .First(attr => attr.AttributeClass.Name == entityTypeAttribute.Name)
                .ConstructorArguments[0].Value as INamedTypeSymbol;
                var dtoClassName = entityType.Name + "Dto";

                var properties = entityType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(prop => prop.DeclaredAccessibility == Accessibility.Public)
                .ToList();

                var sb = new StringBuilder();
                sb.AppendLine($"namespace  {entityClassNamespace} {{");
                sb.AppendLine($"public partial class {generatedDtoClassName}");
                sb.AppendLine("{");
                foreach (var property in properties)
                {
                    sb.AppendLine($"    public {property.Type} {property.Name} {{ get; set; }}");
                }
                sb.AppendLine("}");
                sb.AppendLine("}");
                var generatedCode = sb.ToString();
                var sourceText = SourceText.From(generatedCode, Encoding.UTF8);
                var fileName = $"{generatedDtoClassName}.g.cs";
                context.AddSource(fileName, sourceText);
            }
        }
    }
}
