namespace EndpointGenerator;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class EndpointGroupBuilderAttribute : Attribute
{
    public EndpointGroupBuilderAttribute(string prefix = "")
    {
    }
}
