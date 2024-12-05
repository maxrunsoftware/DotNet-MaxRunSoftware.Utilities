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

// ReSharper disable ConvertToCompoundAssignment
// ReSharper disable RedundantCast

namespace MaxRunSoftware.Utilities.Common.Tests.Types;

public class PercentTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    [SkippableFact]
    public void Default_Value()
    {
        var p = new Percent();
        Assert.Equal(0, (int)p);
    }

    [SkippableFact]
    public void Cast_To_Int_Rounds()
    {
        Assert.Equal(2, (int)(Percent)1.75f);
        Assert.Equal(2, (int)(Percent)1.51f);
        Assert.Equal(1, (int)(Percent)1.25f);
        Assert.Equal(1, (int)(Percent)1.49f);
    }

    [SkippableFact]
    public void Cast_To_Int_Rounds_Every_Other()
    {
        Assert.Equal(2, (int)(Percent)1.5f); // up
        Assert.Equal(2, (int)(Percent)2.5f); // down
        Assert.Equal(4, (int)(Percent)3.5f); // up
        Assert.Equal(4, (int)(Percent)4.5f); // down
    }

    [SkippableFact]
    public void Min()
    {
        Assert.Equal(0, (int)(Percent)(-5.0f));
        Assert.Equal(0, (int)(Percent)(-1000.0f));
    }

    [SkippableFact]
    public void Max()
    {
        Assert.Equal(100, (int)(Percent)101.0f);
        Assert.Equal(100, (int)(Percent)1000.0f);
    }

    [SkippableFact]
    public void Plus_Int()
    {
        var p = (Percent)4;
        Assert.Equal(4, (int)p);
        p = p + (Percent)1;
        Assert.Equal(5, (int)p);
        p = p + 1;
        Assert.Equal(6, (int)p);
    }

    [SkippableFact]
    public void Plus_Float()
    {
        var p = (Percent)4.25f;
        Assert.Equal(4.25f, (float)p);
        p = p + (Percent)1;
        Assert.Equal(5.25f, (float)p);
        p = p + (Percent)0.50f;
        Assert.Equal(5.75f, (float)p);
        p = p + 0.25f;
        Assert.Equal(6, (float)p);
    }
    [SkippableFact]
    public void PlusPlus()
    {
        var p = (Percent)4;
        Assert.Equal(4, (int)p);
        p++;
        Assert.Equal(5, (int)p);
    }

    [SkippableFact]
    public void PlusPlus_Max()
    {
        var p = Percent.MaxValue;
        Assert.Equal(100, (int)p);
        p++;
        Assert.Equal(0, (int)p);
    }

    [SkippableFact]
    public void MinusMinus()
    {
        var p = (Percent)4;
        Assert.Equal(4, (int)p);
        p--;
        Assert.Equal(3, (int)p);
    }

    [SkippableFact]
    public void MinusMinus_Min()
    {
        var p = Percent.MinValue;
        Assert.Equal(0, (int)p);
        p--;
        Assert.Equal(100, (int)p);
    }

    [SkippableFact]
    public void For_Loop()
    {
        var broken = 0;
        for (var percent = Percent.MinValue; percent < Percent.MaxValue; percent++)
        {
            if (broken++ > 150) Assert.Fail(nameof(For_Loop) + " test failed because of too many loops");
            WriteLine(percent.ToString());
        }
    }

    [SkippableFact]
    public void ValuesInt()
    {
        Assert.Equal(101, Percent.ValuesInt.Length);
        for (var i = 0; i <= 100; i++)
        {
            Assert.Equal(i, (int)Percent.ValuesInt[i]);
        }
    }
}
