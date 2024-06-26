<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /readme.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->

# EndpointGenerator

[![Build Status](https://img.shields.io/github/actions/workflow/status/distantcam/EndpointGenerator/build.yml)](https://github.com/distantcam/EndpointGenerator/actions/workflows/build.yml)
[![NuGet Status](https://img.shields.io/nuget/v/EndpointGenerator.svg)](https://www.nuget.org/packages/EndpointGenerator/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/EndpointGenerator.svg)](https://www.nuget.org/packages/EndpointGenerator/)


EndpointGenerator is a Roslyn Source Generator to add methods to call marked minimal api methods.

<!-- toc -->
## Contents

  * [NuGet packages](#nuget-packages)
    * [Your code](#your-code)
    * [What gets generated](#what-gets-generated)
    * [How to use](#how-to-use)<!-- endToc -->

## NuGet packages

https://nuget.org/packages/EndpointGenerator/

### Your code

<!-- snippet: Basic -->
<a id='snippet-Basic'></a>
```cs
public static class Basic
{
    [EndpointGenerator.EndpointBuilder]
    public static void Simple(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder) =>
        builder.MapGet("/simple", () => TypedResults.Ok("Hello World"));

    [EndpointGenerator.EndpointBuilder]
    public static void Grouped(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder) =>
        builder.MapGet("/grouped", () => TypedResults.Ok("Hello World"));

    [EndpointGenerator.EndpointGroupBuilder("prefix")]
    public static void GroupedWithPrefix(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder) =>
        builder.MapGet("/grouped", () => TypedResults.Ok("Hello World"));
}
```
<sup><a href='/src/EndpointGenerator.Example/BasicExamples.cs#L5-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-Basic' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### What gets generated

<!-- snippet: BasicGeneratedCode -->
<a id='snippet-BasicGeneratedCode'></a>
```cs
// The name will be "Map[AssemblyName]Endpoints"
public static IEndpointRouteBuilder MapEndpointGeneratorExampleEndpoints(this IEndpointRouteBuilder builder)
{
    Basic.Simple(builder);
    var group = builder.MapGroup("").WithName("Basic");
    Basic.Grouped(group);
    group = builder.MapGroup("prefix").WithName("Basic");
    Basic.GroupedWithPrefix(group);
    return builder;
}
```
<sup><a href='/src/EndpointGenerator.Example/BasicExamples.cs#L24-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-BasicGeneratedCode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### How to use

<!-- snippet: Usage -->
<a id='snippet-Usage'></a>
```cs
var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.MapEndpointGeneratorExampleEndpoints();

app.Run();
```
<sup><a href='/src/EndpointGenerator.Example/BasicExamples.cs#L41-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-Usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
