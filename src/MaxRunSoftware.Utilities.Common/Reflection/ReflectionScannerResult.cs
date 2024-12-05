namespace MaxRunSoftware.Utilities.Common;

public class ReflectionScannerResult(Assembly assembly, HashSet<Type> typesInThisAssembly, HashSet<Type> typesInOtherAssemblies)
{
    public ReflectionScannerResult(Assembly assembly) : this(assembly, [], []) { }
    
    public Assembly Assembly { get; } = assembly;
    public HashSet<Type> TypesInThisAssembly { get; } = typesInThisAssembly;
    public HashSet<Type> TypesInOtherAssemblies { get; } = typesInOtherAssemblies;

    
}
