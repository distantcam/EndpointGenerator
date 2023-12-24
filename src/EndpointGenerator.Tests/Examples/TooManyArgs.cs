internal class TooManyArgs
{
    [EndpointGenerator.EndpointBuilder]
    public static void Map(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder, string s)
    {
    }

    [EndpointGenerator.EndpointGroupBuilder]
    public static void GroupMap(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder, string s)
    {
    }
}
