﻿namespace NS
{
    public class WithNamespace
    {
        [EndpointGenerator.EndpointBuilder]
        public static void Map(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder)
        {
        }

        [EndpointGenerator.EndpointGroupBuilder]
        public static void GroupMap(Microsoft.AspNetCore.Routing.RouteGroupBuilder builder)
        {
        }
    }
}
