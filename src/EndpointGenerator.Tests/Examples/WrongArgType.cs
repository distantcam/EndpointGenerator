internal class WrongArgType
{
    [EndpointGenerator.EndpointBuilder]
    public static void StringMap(string builder)
    {
    }
    [EndpointGenerator.EndpointBuilder]
    public static void GroupMap(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
    {
    }

    [EndpointGenerator.EndpointGroupBuilder]
    public static void Map(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder)
    {
    }
    [EndpointGenerator.EndpointGroupBuilder]
    public static void StringGroupMap(string builder)
    {
    }
}
