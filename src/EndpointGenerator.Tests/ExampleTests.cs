﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace EndpointGenerator.Tests;

public class ExampleTests
{
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile(theoryData.Codes);
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
        var compilation = await Helpers.Compile(theoryData.Codes);
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
        var compilation = await Helpers.Compile(theoryData.Codes);
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

    public record CodeFileTheoryData : IXunitSerializable
    {
        public required string Name { get; set; }
        public required string[] Codes { get; set; }
        public required string VerifiedDirectory { get; set; }

        public (string, string)[] Options { get; set; } = [];
        public string[] IgnoredCompileDiagnostics { get; set; } = [];

        [SetsRequiredMembers]
        public CodeFileTheoryData(string file, params string[] codes)
        {
            Name = Path.GetFileNameWithoutExtension(file);
            Codes = [File.ReadAllText(file), .. codes];
            VerifiedDirectory = Path.Combine(Path.GetDirectoryName(file) ?? "", "Verified");
        }

        public CodeFileTheoryData() { }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
            Codes = info.GetValue<string[]>(nameof(Codes));
            VerifiedDirectory = info.GetValue<string>(nameof(VerifiedDirectory));
            Options = info.GetValue<string[]>(nameof(Options))
                .Select(o => o.Split('|'))
                .Select(o => (o[0], o[1]))
                .ToArray();
            IgnoredCompileDiagnostics = info.GetValue<string[]>(nameof(IgnoredCompileDiagnostics));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Codes), Codes);
            info.AddValue(nameof(VerifiedDirectory), VerifiedDirectory);
            info.AddValue(nameof(Options), Options.Select(o => $"{o.Item1}|{o.Item2}").ToArray());
            info.AddValue(nameof(IgnoredCompileDiagnostics), IgnoredCompileDiagnostics);
        }

        public override string ToString() => Name + ".cs";
    }
}
