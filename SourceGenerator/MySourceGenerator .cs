﻿using Microsoft.CodeAnalysis;
using System;

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
            Console.WriteLine("Dang chay SG");
            // Xử lý logic để tạo mã nguồn
            // Có thể sử dụng context để truy cập vào thông tin về mã nguồn hiện tại và tạo mã mới
            // Không có gì để khởi tạo
            // Find the main method
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

            // Build up the source code
            string source = $@"// <auto-generated/>
using System;

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
    public static partial class {mainMethod.ContainingType.Name}
    {{
        static partial void HelloFrom(string name) =>
            Console.WriteLine($""Generator says: Hi from '{{name}}'"");
    }}
}}
";
            var typeName = mainMethod.ContainingType.Name;

            // Add the source code to the compilation
            context.AddSource($"{typeName}.g.cs", source);
        }
    }
}
