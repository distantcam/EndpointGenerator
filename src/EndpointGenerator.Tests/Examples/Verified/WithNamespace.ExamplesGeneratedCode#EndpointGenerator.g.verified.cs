﻿//HintName: EndpointGenerator.g.cs
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by https://github.com/distantcam/EndpointGenerator
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#pragma warning disable CS8019
using global::Microsoft.AspNetCore.Builder;
using global::Microsoft.AspNetCore.Http;
using global::Microsoft.AspNetCore.Routing;
#pragma warning restore CS8019

namespace Microsoft.Extensions.DependencyInjection
{
	[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
	internal static class EndpointExtensions
	{
		public static IEndpointRouteBuilder MapEndpointGeneratorTestEndpoints(this IEndpointRouteBuilder builder)
		{
			global::NS.WithNamespace.Map(builder);
			global::NS.WithNamespace.GroupMap(builder.MapGroup("").WithName("WithNamespace").WithTags("NS"));
			return builder;
		}
	}
}
