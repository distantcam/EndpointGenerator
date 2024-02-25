internal class DisableAntiforgery
{
    [EndpointGenerator.EndpointGroupBuilder(disableAntiforgery: true)]
    public static void MapWithoutAntiforgery(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
    {
    }
}
