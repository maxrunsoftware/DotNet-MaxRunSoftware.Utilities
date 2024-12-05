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

using System.Net;
using System.Numerics;

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    public static readonly Guid Guid_Min = new("00000000-0000-0000-0000-000000000000");
    public static readonly Guid Guid_Max = new("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
}

/// <summary>
/// https://stackoverflow.com/questions/75365299/how-to-generate-constant-values-with-generic-math
/// </summary>
/// <typeparam name="T"></typeparam>
public static class ConstantNumber<T> where T : INumberBase<T>
{
    private static T Calc(int count)
    {
        var v = T.Zero;
        for (var i = 0; i < count; i++)
        {
            v = v + T.One;
        }
        return v;
    }
    
    // @formatter:off
    
    public static readonly T Zero  = T.Zero;
    public static readonly T One  = Calc(1);
    public static readonly T Two  = Calc(2);
    public static readonly T Three  = Calc(3);
    public static readonly T Four  = Calc(4);
    public static readonly T Five  = Calc(5);
    public static readonly T Six  = Calc(6);
    public static readonly T Seven  = Calc(7);
    public static readonly T Eight  = Calc(8);
    public static readonly T Nine  = Calc(9);
    public static readonly T Ten = Calc(10);
    
    // @formatter:on
}

public static class ConstantNumberNegative<T> where T : ISignedNumber<T>
{
    private static T Calc(int count)
    {
        var v = T.Zero;
        for (var i = 0; i < count; i++)
        {
            v += T.NegativeOne;
        }
        return v;
    }
    
    // @formatter:off
    
    public static readonly T NegativeOne  = Calc(1);
    public static readonly T NegativeTwo  = Calc(2);
    public static readonly T NegativeThree  = Calc(3);
    public static readonly T NegativeFour  = Calc(4);
    public static readonly T NegativeFive  = Calc(5);
    public static readonly T NegativeSix  = Calc(6);
    public static readonly T NegativeSeven  = Calc(7);
    public static readonly T NegativeEight  = Calc(8);
    public static readonly T NegativeNine  = Calc(9);
    public static readonly T NegativeTen = Calc(10);
    
    // @formatter:on
}
