using MySourceGenerator2;

namespace ConsoleApp;

partial class Program
{
    static void Main(string[] args)
    {
        HelloFrom("Generated Code");
        var dto = new BookGenerateDto();
    }

    static partial void HelloFrom(string name);
}