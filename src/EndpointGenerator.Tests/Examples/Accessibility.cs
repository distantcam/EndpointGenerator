﻿internal class Accessibility
{
    [EndpointGenerator.EndpointBuilder]
    public static void PublicMap(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder)
    {
    }

    [EndpointGenerator.EndpointGroupBuilder("/accessibility")]
    public static void PublicGroupMap(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
    {
    }

    [EndpointGenerator.EndpointBuilder]
    internal static void InternalMap(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder)
    {
    }

    [EndpointGenerator.EndpointGroupBuilder("/accessibility")]
    internal static void InternalGroupMap(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
    {
    }

    [EndpointGenerator.EndpointBuilder]
    protected static void ProtectedMap(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder)
    {
    }

    [EndpointGenerator.EndpointGroupBuilder("/accessibility")]
    protected static void ProtectedGroupMap(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
    {
    }

    [EndpointGenerator.EndpointBuilder]
    protected internal static void ProtectedInternalMap(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder)
    {
    }

    [EndpointGenerator.EndpointGroupBuilder("/accessibility")]
    protected internal static void ProtectedInternalGroupMap(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
    {
    }

    [EndpointGenerator.EndpointBuilder]
    private static void PrivateMap(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder)
    {
    }

    [EndpointGenerator.EndpointGroupBuilder("/accessibility")]
    private static void PrivateGroupMap(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
    {
    }
}
