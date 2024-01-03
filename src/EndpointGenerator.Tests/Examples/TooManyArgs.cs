internal class TooManyArgs
{
    [EndpointGenerator.EndpointBuilder]
    public static void Map(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder, string s)
    {
    }

    [EndpointGenerator.EndpointGroupBuilder("/toomanyargs")]
    public static void GroupMap(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder, string s)
    {
    }
}
