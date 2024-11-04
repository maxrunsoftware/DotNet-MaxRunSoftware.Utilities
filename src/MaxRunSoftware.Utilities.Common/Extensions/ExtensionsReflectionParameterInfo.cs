// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
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

using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.RegularExpressions;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsReflectionParameterInfo
{
    #region ParameterInfo

    /// <summary>
    /// https://stackoverflow.com/a/38110036
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsOut(this ParameterInfo info) => info.ParameterType.IsByRef && !info.IsOut;

    /// <summary>
    /// https://stackoverflow.com/a/38110036
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsIn(this ParameterInfo info) => info.ParameterType.IsByRef && info.IsIn;

    /// <summary>
    /// https://stackoverflow.com/a/38110036
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsRef(this ParameterInfo info) => info.ParameterType.IsByRef && !info.IsOut;

    #endregion ParameterInfo
    
    public static int? MetadataToken_Safe(this ParameterInfo info)
    {
        try
        {
            return info.MetadataToken;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (NotImplementedException)
        {
            return null;
        }
    }
}
