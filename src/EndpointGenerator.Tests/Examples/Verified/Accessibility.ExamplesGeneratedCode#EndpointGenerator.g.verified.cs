﻿//HintName: EndpointGenerator.g.cs
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by https://github.com/distantcam/EndpointGenerator
//     Version: 0.0.0.0
//     SHA: abc
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
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("EndpointGenerator", "0.0.0.0")]
	internal static class EndpointExtensions
	{
		public static IEndpointRouteBuilder MapEndpointGeneratorTestEndpoints(this IEndpointRouteBuilder builder)
		{
			global::Accessibility.PublicMap(builder);
			global::Accessibility.InternalMap(builder);
			global::Accessibility.ProtectedInternalMap(builder);
			var group0 = builder.MapGroup("").WithName("Accessibility");
			global::Accessibility.PublicGroupMap(group0);
			global::Accessibility.InternalGroupMap(group0);
			global::Accessibility.ProtectedInternalGroupMap(group0);
			return builder;
		}
	}
}
