; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category          | Severity | Notes
--------|-------------------|----------|--------------------
ENDP001 | EndpointGenerator |  Warning | BuilderMethodMustBeStatic
ENDP002 | EndpointGenerator |  Warning | BuilderMethodMustBeAccessible
ENDP003 | EndpointGenerator |  Warning | BuilderMethodMustHaveOnlyOneArgument
ENDP004 | EndpointGenerator |  Warning | BuilderMethodMustHaveCorrectArg
ENDP005 | EndpointGenerator |  Warning | BuilderGroupMethodMustHaveCorrectArg
