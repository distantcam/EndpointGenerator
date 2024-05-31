namespace EndpointGenerator;

public partial class EndpointBuilderSourceGenerator
{
    public static class TrackingNames
    {
        public static string AssemblyName => nameof(AssemblyName);
        public static string BuilderModels => nameof(BuilderModels);
        public static string GroupBuilderModels => nameof(GroupBuilderModels);

        public static IReadOnlyCollection<string> AllTrackers { get; } = [
            AssemblyName,
            BuilderModels,
            GroupBuilderModels,
        ];
    }
}
