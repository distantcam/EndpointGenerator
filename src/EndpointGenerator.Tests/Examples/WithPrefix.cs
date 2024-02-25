internal class WithPrefix
{
    [EndpointGenerator.EndpointGroupBuilder("ThePrefix")]
    public static void PrefixMap1(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
    {
    }

    [EndpointGenerator.EndpointGroupBuilder("ThePrefix")]
    public static void PrefixMap2(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
    {
    }
}
