using System.Security;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsReflectionAssemblyName
{
    /// <summary>
    /// TODO: avoid Assembly.Load and perhaps switch to something else that doesn't full load, perhaps https://learn.microsoft.com/en-us/dotnet/standard/assembly/inspect-contents-using-metadataloadcontext
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to load</param>
    /// <returns>The assembly</returns>
    public static Assembly GetAssembly(this AssemblyName assemblyName) => Assembly.Load(assemblyName);
    
    [ContractAnnotation("exception:null => notnull; exception:notnull=>null")]
    public static Assembly? GetAssembly(this AssemblyName assemblyName, out Exception? exception)
    {
        try
        {
            var a = Assembly.Load(assemblyName);
            exception = null;
            return a;
        }
        catch (Exception e)
        {
            exception = e;
            return null;
        }
    }
    
    public static byte[]? GetPublicKey_Safe(this AssemblyName assemblyName)
    {
        try
        {
            return assemblyName.GetPublicKey();
        }
        catch (SecurityException)
        {
            return null;
        }
    }
}
