// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
