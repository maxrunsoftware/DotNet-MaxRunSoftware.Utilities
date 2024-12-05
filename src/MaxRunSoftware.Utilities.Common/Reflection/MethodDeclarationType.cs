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

namespace MaxRunSoftware.Utilities.Common;

public enum MethodDeclarationType
{
    None,
    Override,
    ShadowSignature,
    ShadowName,
    Virtual,
}

public static class MethodDeclarationTypeExtensions
{
    public static MethodDeclarationType GetDeclarationType(this MethodBase? info)
    {
        // https://stackoverflow.com/a/288928
        if (info == null) return MethodDeclarationType.None;
        if (info.DeclaringType != info.ReflectedType)
        {
            //if (info.IsHideBySig) return MethodDeclarationType.Override;
            return MethodDeclarationType.None;
        }

        var attrs = info.Attributes;

        if ((attrs & MethodAttributes.Virtual) != 0 && (attrs & MethodAttributes.NewSlot) == 0) return MethodDeclarationType.Override;

        var baseType = info.DeclaringType?.BaseType;
        if (baseType == null) return MethodDeclarationType.None;

        if (info.IsHideBySig)
        {
            var flagsSig = info.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            flagsSig |= info.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
            flagsSig |= BindingFlags.ExactBinding; //https://stackoverflow.com/questions/288357/how-does-reflection-tell-me-when-a-property-is-hiding-an-inherited-member-with-t#comment75338322_288928
            var paramTypes = info.GetParameters().Select(p => p.ParameterType).ToArray();
            var baseMethod = baseType.GetMethod(info.Name, flagsSig, null, paramTypes, null);
            if (baseMethod != null) return MethodDeclarationType.ShadowSignature;

            // return MethodDeclarationType.None;
        }


        var flagsName = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        if (baseType.GetMethods(flagsName).Any(m => m.Name == info.Name)) return MethodDeclarationType.ShadowName;

        if ((attrs & MethodAttributes.Virtual) != 0) return MethodDeclarationType.Virtual;

        return MethodDeclarationType.None;
    }
}
