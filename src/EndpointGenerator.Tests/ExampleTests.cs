using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit.Abstractions;

namespace EndpointGenerator.Tests;

[UsesVerify]
public class ExampleTests
{
    private readonly VerifySettings _codeVerifySettings;

    public ExampleTests()
    {
        _codeVerifySettings = new();
        _codeVerifySettings.ScrubLinesContaining("Version:", "SHA:", "GeneratedCodeAttribute");
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var compilation = await Compile(theoryData.Code);
        var generator = new EndpoointBuilderSourceGenerator();
        var driver = CreateDriver(compilation, generator).RunGenerators(compilation);

        await Verify(driver, _codeVerifySettings)
            .UseDirectory(Path.Combine("Examples", "Verified"))
            .UseTypeName(theoryData.Name);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        string[] ignoredWarnings = [];

        var compilation = await Compile(theoryData.Code);
        var generator = new EndpoointBuilderSourceGenerator();
        CreateDriver(compilation, generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        outputCompilation.GetDiagnostics()
            .Where(d => !ignoredWarnings.Contains(d.Id))
            .Should().BeEmpty();
    }

    private static GeneratorDriver CreateDriver(Compilation c, params IIncrementalGenerator[] generators)
        => CSharpGeneratorDriver.Create(generators).WithUpdatedParseOptions(c.SyntaxTrees.FirstOrDefault().Options as CSharpParseOptions);

    private static async Task<CSharpCompilation> Compile(params string[] code)
    {
        var references = await new ReferenceAssemblies(
            "net8.0",
            new PackageIdentity(
                "Microsoft.NETCore.App.Ref",
                "8.0.0"),
            Path.Combine("ref", "net8.0"))
            .AddPackages([new("Microsoft.AspNetCore.App.Ref", "8.0.0")])
            .ResolveAsync(null, CancellationToken.None);
        var attributeReference = MetadataReference.CreateFromFile(Path.Combine(Environment.CurrentDirectory, "EndpointGenerator.Attributes.dll"));

        return CSharpCompilation.Create(
            "EndpointGeneratorTest",
            code.Select(c => CSharpSyntaxTree.ParseText(c, CSharpParseOptions.Default)),
            [attributeReference, .. references],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    public static IEnumerable<object[]> GetExamples()
    {
        var baseDir = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;
        if (baseDir == null) yield break;

        var examples = Directory.GetFiles(Path.Combine(baseDir.FullName, "Examples"), "*.cs");
        foreach (var example in examples)
        {
            if (example.Contains(".g.")) continue;

            yield return new object[] {
                new CodeFileTheoryData {
                    Code = File.ReadAllText(example),
                    Name = Path.GetFileNameWithoutExtension(example)
                }
            };
        }
    }

    public class CodeFileTheoryData : IXunitSerializable
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>("Name");
            Code = info.GetValue<string>("Code");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("Name", Name);
            info.AddValue("Code", Code);
        }

        public override string ToString() => Name + ".cs";
    }
}
