using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

#if ROSLYN_4
using Microsoft.CodeAnalysis.CSharp;
#endif

namespace EndpointGenerator.Tests;

public class ExampleTests
{
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<EndpointBuilderAttribute>(theoryData.Codes,
            langPreview: theoryData.LangPreview,
            preprocessorSymbols: s_preprocessorSymbols,
            assemblyName: "EndpointGeneratorTest",
            extraReferences: await ExtraReferences());
        var generator = new EndpointBuilderSourceGenerator().AsSourceGenerator();
        var driver = Helpers.CreateDriver(theoryData.Options, theoryData.LangPreview, generator)
            .RunGenerators(compilation, TestContext.Current.CancellationToken);

        await Verify(driver)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name).IgnoreParametersForVerified(theoryData);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<EndpointBuilderAttribute>(theoryData.Codes,
            langPreview: theoryData.LangPreview,
            preprocessorSymbols: s_preprocessorSymbols,
            assemblyName: "EndpointGeneratorTest",
            extraReferences: await ExtraReferences());
        var generator = new EndpointBuilderSourceGenerator().AsSourceGenerator();
        Helpers.CreateDriver(theoryData.Options, theoryData.LangPreview, generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _, TestContext.Current.CancellationToken);

        Assert.Empty(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken).Where(d => !theoryData.IgnoredCompileDiagnostics.Contains(d.Id)));
    }

#if ROSLYN_4
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task EnsureRunsAreCachedCorrectly(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<EndpointBuilderAttribute>(theoryData.Codes,
            langPreview: theoryData.LangPreview,
            preprocessorSymbols: s_preprocessorSymbols,
            assemblyName: "EndpointGeneratorTest",
            extraReferences: await ExtraReferences());
        var generator = new EndpointBuilderSourceGenerator().AsSourceGenerator();

        var driver = Helpers.CreateDriver(theoryData.Options, theoryData.LangPreview, generator);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var firstResult = driver.GetRunResult();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("// dummy",
            CSharpParseOptions.Default.WithLanguageVersion(theoryData.LangPreview
                ? LanguageVersion.Preview
                : LanguageVersion.Latest),
            cancellationToken: TestContext.Current.CancellationToken));
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var secondResult = driver.GetRunResult();

        Helpers.AssertRunsEqual(firstResult, secondResult,
            EndpointBuilderSourceGenerator.TrackingNames.AllTrackers);
    }
#endif

    // ----------------------------------------------------------------------------------------

    private static readonly IEnumerable<string> s_preprocessorSymbols =
#if ROSLYN_3
        ["ROSLYN_3"];
#elif ROSLYN_4
        ["ROSLYN_4"];
#endif

    private static async Task<IEnumerable<MetadataReference>> ExtraReferences()
    {
        var aspnetRef = await new ReferenceAssemblies(
            "net9.0",
            new("Microsoft.AspNetCore.App.Ref", "9.0.0"),
            Path.Combine("ref", "net9.0"))
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
