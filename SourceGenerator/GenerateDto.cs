using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;

namespace GenerateDTO
{
    [Generator]
    public class GenerateDto : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // Lấy ra tất cả các class trong syntax trees của compilation
            var classes = context.Compilation.SyntaxTrees
                .SelectMany(st => st.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());

            // Duyệt qua từng class để kiểm tra attribute và sinh ra mã cho class tương ứng
            foreach (var @class in classes)
            {
                var model = context.Compilation.GetSemanticModel(@class.SyntaxTree);
                var attributeData = model.GetDeclaredSymbol(@class)?.GetAttributes()
                    .FirstOrDefault(ad => ad.AttributeClass?.Name == "EntityToDtoAttribute");

                // Kiểm tra xem class có được đánh dấu bằng attribute "EntityToDtoAttribute" hay không
                if (attributeData != null)
                {
                    var className = @class.Identifier.ValueText;
                    var dtoClassName = $"{className}Dto";

                    // Tạo mã cho class DTO
                    var dtoClassSource = GenerateDtoClassSource(@class, dtoClassName);

                    // Tạo syntax tree từ mã DTO class
                    var syntaxTree = CSharpSyntaxTree.ParseText(dtoClassSource);

                    // Thêm syntax tree vào compilation
                    context.AddSource($"{dtoClassName}.g.cs", syntaxTree.GetText());
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        private string GenerateDtoClassSource(ClassDeclarationSyntax @class, string dtoClassName)
        {
            var properties = @class.Members
           .OfType<PropertyDeclarationSyntax>()
           .Where(p => p.Modifiers.Any(mod => mod.Kind() == SyntaxKind.PublicKeyword))
           .Select(p => p.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))))
           .ToList();

            var dtoClass = SyntaxFactory.ClassDeclaration(dtoClassName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(properties.ToArray());

            var namespaceSyntax = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("YourNamespace"))
                .AddMembers(dtoClass);

            var code = new StringBuilder();
            var writer = new System.IO.StringWriter(code);

            var workspace = new AdhocWorkspace();
            var options = workspace.Options
            .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, 4)
            .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, false);
            var formattedNamespace = Formatter.Format(namespaceSyntax, workspace, options);
            formattedNamespace.WriteTo(writer);

            return code.ToString();
        }
    }

}

