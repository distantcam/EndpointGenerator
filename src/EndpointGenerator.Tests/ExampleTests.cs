using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace EndpointGenerator.Tests;

public class ExampleTests
{
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<EndpointBuilderAttribute>(theoryData.Codes,
            preprocessorSymbols: PreprocessorSymbols,
            assemblyName: "EndpointGeneratorTest",
            extraReferences: await ExtraReferences());
        var generator = new EndpointBuilderSourceGenerator().AsSourceGenerator();
        var driver = Helpers.CreateDriver(theoryData.Options, generator)
            .RunGenerators(compilation);

        await Verify(driver)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<EndpointBuilderAttribute>(theoryData.Codes,
            preprocessorSymbols: PreprocessorSymbols,
            assemblyName: "EndpointGeneratorTest",
            extraReferences: await ExtraReferences());
        var generator = new EndpointBuilderSourceGenerator().AsSourceGenerator();
        Helpers.CreateDriver(theoryData.Options, generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        outputCompilation.GetDiagnostics()
            .Where(d => !theoryData.IgnoredCompileDiagnostics.Contains(d.Id))
            .Should().BeEmpty();
    }

#if ROSLYN_4
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task EnsureRunsAreCachedCorrectly(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<EndpointBuilderAttribute>(theoryData.Codes,
            preprocessorSymbols: PreprocessorSymbols,
            assemblyName: "EndpointGeneratorTest",
            extraReferences: await ExtraReferences());
        var generator = new EndpointBuilderSourceGenerator().AsSourceGenerator();

        var driver = Helpers.CreateDriver(theoryData.Options, generator);
        driver = driver.RunGenerators(compilation);
        var firstResult = driver.GetRunResult();
        compilation = compilation.AddSyntaxTrees(
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("// dummy"));
        driver = driver.RunGenerators(compilation);
        var secondResult = driver.GetRunResult();

        Helpers.AssertRunsEqual(firstResult, secondResult,
            EndpointBuilderSourceGenerator.TrackingNames.AllTrackers);
    }
#endif

    // ----------------------------------------------------------------------------------------

    private static IEnumerable<string> PreprocessorSymbols =
#if ROSLYN_3
        ["ROSLYN_3"];
#elif ROSLYN_4
        ["ROSLYN_4"];
#endif

    private static async Task<IEnumerable<MetadataReference>> ExtraReferences()
    {
        var aspnetRef = await new ReferenceAssemblies(
            "net8.0",
            new("Microsoft.AspNetCore.App.Ref", "8.0.6"),
            Path.Combine("ref", "net8.0"))
            .ResolveAsync(null, CancellationToken.None);

        return [.. aspnetRef];
    }

    private static DirectoryInfo? BaseDir { get; } = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;

    private static IEnumerable<string> GetExamplesFiles(string path) => Directory.GetFiles(Path.Combine(BaseDir?.FullName ?? "", path), "*.cs").Where(e => !e.Contains(".g."));

    public static TheoryData<CodeFileTheoryData> GetExamples()
    {
        if (BaseDir == null)
            throw new Exception("BaseDir is null");

        var data = new TheoryData<CodeFileTheoryData>();

        foreach (var example in GetExamplesFiles("Examples"))
        {
            data.Add(new CodeFileTheoryData(example));
        }

        return data;
    }
}
