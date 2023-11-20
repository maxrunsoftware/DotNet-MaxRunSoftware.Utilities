// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mail;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable PropertyCanBeMadeInitOnly.Local
// ReSharper disable InconsistentNaming

namespace MaxRunSoftware.Utilities.Common.Tests.Utils;

#nullable enable

[SuppressMessage("Assertions", "xUnit2002:Do not use null check on value type")]
public class UtilChangeTypeTests : TestBase
{
    public UtilChangeTypeTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    private void TestItem<T>(object actual, T expected, Action<T, T>? equal = null)
    {
        var actualChangedType = Util.ChangeType<T>(actual);
        Assert.NotNull(actualChangedType!);
        Assert.IsType<T>(actualChangedType);
        if (equal == null)
        {
            Assert.Equal(expected, actualChangedType);
        }
        else
        {
            equal(expected, actualChangedType);
        }
    }

    #region string

    [Theory]
    [InlineData("42", 42)]
    public void String_Int(string actual, int expected) => TestItem(actual, expected);

    [Theory]
    [InlineData("12.345", 12.345f)]
    public void String_Float(string actual, float expected) => TestItem(actual, expected);

    [Theory]
    [InlineData("333.321", 333.321f)]
    public void String_Decimal(string actual, decimal expected) => TestItem(actual, expected);

    [SkippableFact]
    public void String_Guid() => TestItem("93907b87-5572-4bac-aa30-14cfc6fef5e3", Guid.Parse("93907b87-5572-4bac-aa30-14cfc6fef5e3"));

    [SkippableFact]
    public void String_MailAddress() => TestItem("abc@aol.com", new MailAddress("abc@aol.com"), (e, a) => Assert.Equal(e.Address, a.Address));

    [Theory]
    [InlineData("1")]
    [InlineData("t", "T", "  T   ")]
    [InlineData("TRUE", "true", "  TrUe", "tRue")]
    [InlineData("y", "Y ")]
    [InlineData("YES", "YeS", "yEs")]
    public void String_Bool_True(params string[] strs)
    {
        foreach (var str in strs)
        {
            TestItem(str, true);
        }
    }

    [Theory]
    [InlineData("0")]
    [InlineData("f", "F", "  F   ")]
    [InlineData("FALSE", "false", "  FalSe", "fAlSe")]
    [InlineData("n", "N ")]
    [InlineData("NO", "No", " nO  ")]
    public void String_Bool_False(params string[] strs)
    {
        foreach (var str in strs)
        {
            TestItem(str, false);
        }
    }

    [Theory]
    [InlineData("Static", BindingFlags.Static)]
    [InlineData("  staTic ", BindingFlags.Static)]
    [InlineData(" puBlic  ", BindingFlags.Public)]
    public void String_Enum(string enumItemString, object expected)
    {
        var enumType = expected.GetType();
        var actual = Util.ChangeType(enumItemString, enumType);
        Assert.NotNull(actual!);
        Assert.IsType(enumType, actual);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("public", BindingFlags.GetField)]
    [InlineData("static", BindingFlags.ExactBinding)]
    public void String_Enum_NotEqual(string enumItemString, object expected)
    {
        var enumType = expected.GetType();
        var actual = Util.ChangeType(enumItemString, enumType);
        Assert.NotNull(actual!);
        Assert.IsType(enumType, actual);
        Assert.NotEqual(expected, actual);
    }

    #endregion string

    #region int

    [Theory]
    [InlineData(42, "42")]
    public void Int_String(int expected, string actual) => TestItem(expected, actual);


    [Theory]
    [InlineData(13, 13.0f)]
    public void Int_Float(int expected, float actual) => TestItem(expected, actual);

    [Theory]
    [InlineData(13, 13)]
    public void Int_Byte(int expected, byte actual) => TestItem(expected, actual);

    [Theory]
    [InlineData(-5, -5)]
    public void Int_SByte(int expected, sbyte actual) => TestItem(expected, actual);

    #endregion int

    #region CastImplicit

    private class CastImplicit_String
    {
        public CastImplicit_String(string value)
        {
            Value = value;
        }
        public string Value { get; set; }

        public static implicit operator CastExplicit_Int(CastImplicit_String obj) => new(int.Parse(obj.Value));
        public static implicit operator CastImplicit_String(CastExplicit_Int obj) => new(obj.Value.ToString());

        public static implicit operator CastImplicit_String(int obj) => new(obj + "a");
        public static implicit operator int(CastImplicit_String obj) => obj.Value.ToInt();

        public static implicit operator CastImplicit_String(string obj) => new(obj + "b");
        public static implicit operator string(CastImplicit_String obj) => obj.Value + "c";

        public override string ToString() => Value + "t";
    }

    private class CastExplicit_Int
    {
        public CastExplicit_Int(int value)
        {
            Value = value;
        }
        public int Value { get; set; }

        public static explicit operator CastExplicit_Int(CastImplicit_String obj) => new(int.Parse(obj.Value));
        public static explicit operator CastImplicit_String(CastExplicit_Int obj) => new(obj.Value.ToString());

        public static explicit operator CastExplicit_Int(IPAddress obj) => new(obj.GetAddressBytes().Select(o => (int)o).Sum());
        public static explicit operator IPAddress(CastExplicit_Int obj) => new(new []{(byte)(obj.Value), (byte)(obj.Value-1), (byte)(obj.Value-2), (byte)(obj.Value-3)});

    }

    [SkippableFact]
    public void CastImplicit()
    {
        //var tc = new TypeConverter();

        foreach (var m in typeof(CastImplicit_String).GetMethodSlims(BindingFlags.Public | BindingFlags.Static)) WriteLine(m.ToString());
        foreach (var m in typeof(CastExplicit_Int).GetMethodSlims(BindingFlags.Public | BindingFlags.Static)) WriteLine(m.ToString());

        var i = new CastImplicit_String("V");
        //var o = Convert.ChangeType(i, typeof(string));
        string o = i;
        Assert.Equal("Vc", o);
        //var s = (string)o;
        //Assert.Equal("Vc", s);

        //var ms = i.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static);
        //foreach (var m in ms) output.WriteLine(m.Name);
        //var s = Util.ChangeType<string>(i);
        //Assert.Equal("Vc", s);
    }

    #endregion Cast Implicit
}
