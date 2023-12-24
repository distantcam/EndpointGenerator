internal class NonStatic
{
    [EndpointGenerator.EndpointBuilder]
    public void Map(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder)
    {
    }

    [EndpointGenerator.EndpointGroupBuilder]
    public void GroupMap(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
    {
    }
}
