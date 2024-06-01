using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

#region Basic
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
#endregion

public static class Generated
{
    #region BasicGeneratedCode

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

    #endregion

    public static void Main()
    {
        #region Usage

        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapEndpointGeneratorExampleEndpoints();

        app.Run();

        #endregion
    }
}
