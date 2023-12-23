namespace EndpointGenerator;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class EndpointBuilderAttribute : Attribute
{
}
