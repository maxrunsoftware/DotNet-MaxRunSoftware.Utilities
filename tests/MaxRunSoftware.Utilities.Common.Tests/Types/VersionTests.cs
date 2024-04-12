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

namespace MaxRunSoftware.Utilities.Common.Tests.Types;

#nullable enable

public class VersionTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    private const string LARGE_INT = "9999999999999999999999999999999999";
    [SkippableTheory]
    [InlineData("1,2,3,4,5", "1.2.3.4.5")]
    [InlineData("1,2,3,4,null", "1.2.3.4." + LARGE_INT)]
    public void ValueInt(string expected, string version) => Assert.Equal(
        expected.Split(",").Select(o => o == "null" ? null : o).Select(o => o.ToIntNullable()).ToArray(),
        new Version(version).Select(o => o.ValueInt).ToArray()
    );

    [SkippableTheory]
    [InlineData("1,2,3,4,5", "1.2.3.4.5")]
    [InlineData("1,2,3,4,null", "1.2.3.4." + LARGE_INT)]
    public void ValueLong(string expected, string version) => Assert.Equal(
        expected.Split(",").Select(o => o == "null" ? null : o).Select(o => o.ToLongNullable()).ToArray(),
        new Version(version).Select(o => o.ValueLong).ToArray()
    );

    [SkippableTheory]
    [InlineData("1,2,3,4,5", "1.2.3.4.5")]
    [InlineData(LARGE_INT, LARGE_INT)]
    [InlineData("1,2,3,4," + LARGE_INT, "1.2.3.4." + LARGE_INT)]
    public void ValueBigInteger(string expected, string version) => Assert.Equal(
        expected.Split(",").Select(o => o == "null" ? null : o).Select(o => o.ToBigIntegerNullable()).ToArray(),
        new Version(version).Select(o => o.ValueBigInteger).ToArray()
    );

    [SkippableFact]
    public void Value()
    {
        var v = new Version("1.2.3.4.5");
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }.Select(o => o.ToString()).ToArray(), v.Select(o => o.Value).ToArray());
    }

    [SkippableFact]
    public void ToSystemVersion_Missing1()
    {
        var v = new Version("1.2.3");
        var vv = v.ToSystemVersion();
        Assert.Equal(1, vv.Major);
        Assert.Equal(2, vv.Minor);
        Assert.Equal(3, vv.Build);
        Assert.Equal(0, vv.Revision);
    }

    [SkippableFact]
    public void ToSystemVersion_Missing2()
    {
        var v = new Version("3.2");
        var vv = v.ToSystemVersion();
        Assert.Equal(3, vv.Major);
        Assert.Equal(2, vv.Minor);
        Assert.Equal(0, vv.Build);
        Assert.Equal(0, vv.Revision);
    }

    [SkippableTheory]
    [InlineData("1", "2", "3", "4", "5")]
    [InlineData("1.0", "2.8", "3.4", "4.9", "5.3")]
    [InlineData("1.6", "2.beta", "2", "2.1", "3.2")]
    [InlineData("1.6", "2.beta.3", "2.beta.4", "2.pre.1", "2")]
    [InlineData("1.6", "2.beta.3", "2.beta.4", "2.pre.1", "2.0.0")]
    public void CompareTo(string a, string b, string c, string d, string e)
    {
        var list = new List<Version>();
        var versions = new[] { a, b, c, d, e }.Select(o => new Version(o)).ToArray();

        foreach (var array in versions.Permutate())
        {
            list.Clear();
            list.AddRange(array);
            list.Sort();
            Assert.Equal(versions.Length, list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                Assert.Equal(versions[i], list[i]);
            }
        }
    }

    [SkippableTheory]
    [InlineData("Beta")]
    [InlineData("BETA")]
    [InlineData("17.5.0-preview-20221221-03")]
    public void IsPreRelease(string a) => Assert.True(new Version(a).IsPreRelease);

    [SkippableTheory]
    [InlineData("1", "1")]
    [InlineData("1", "1.0")]
    [InlineData("1.0", "1.0.0.0.0")]
    [InlineData("Beta", "BETA")]
    [InlineData("Beta", "BETA.0.0.0")]
    [InlineData("1.2.Beta", "1.2.BETA.0.0.0")]
    [InlineData("1.2.Beta.1.2.0", "1.2.BETA.1.2.0")]
    [InlineData("17.5.0-preview-20221221-03", "17.5.0-PREVIEW.--20221221-03")]
    public void Equal(string a, string b)
    {
        var v1 = new Version(a);
        var v2 = new Version(b);
        Assert.True(Version.Equals(v1, v2));
        Assert.True(Version.Equals(v2, v1));
    }

    [SkippableTheory]
    [InlineData("1", "2")]
    [InlineData("1.1", "1.2")]
    [InlineData("1.2", "2.1")]
    [InlineData("Beta", "Alpha")]
    [InlineData("1.2.Beta", "1.2.Alpha")]
    [InlineData("1.2.Beta.1.2", "1.2.Beta.1.3")]
    [InlineData("17.5.0-preview-20221221-03", "17.5.1-preview-20221221-03")]
    [InlineData("17.5.0-preview-20221221-03", "17.5.0-preview-20221221-04")]
    public void NotEqual(string a, string b)
    {
        var v1 = new Version(a);
        var v2 = new Version(b);
        Assert.False(Version.Equals(v1, v2));
        Assert.False(Version.Equals(v2, v1));
    }
}

public class VersionPartTests : TestBaseBase
{
    public VersionPartTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [SkippableTheory]
    [InlineData("1", "1")]
    [InlineData("2", "2")]
    [InlineData("Beta", "BETA")]
    [InlineData(null!, null!)]
    [InlineData(null!, "0")]
    [InlineData("0", null!)]
    public void Equal(string? a, string? b)
    {
        var v1 = a != null ? new VersionPart(a) : null;
        var v2 = b != null ? new VersionPart(b) : null;
        Assert.True(VersionPart.Equals(v1, v2));
        Assert.True(VersionPart.Equals(v2, v1));
    }

    [SkippableTheory]
    [InlineData("20200102")]
    [InlineData("20200102030405")]
    public void ValueDateTime(string a)
    {
        var v = new VersionPart(a);
        Assert.NotNull(v.ValueDateTime);
        Assert.Equal(a.ToDateTime(), v.ValueDateTime);
    }

    [SkippableTheory]
    [InlineData("20200102", "20200102")]
    [InlineData("20200102030405", "20200102")]
    public void ValueDate(string versionPart, string expected)
    {
        var v = new VersionPart(versionPart);
        Assert.NotNull(v.ValueDate);
        Assert.Equal(expected.ToDateOnly(), v.ValueDate);
    }
}
