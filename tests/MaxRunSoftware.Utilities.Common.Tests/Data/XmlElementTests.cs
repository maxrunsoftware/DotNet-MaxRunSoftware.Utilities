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

using System.Diagnostics.CodeAnalysis;

namespace MaxRunSoftware.Utilities.Common.Tests.Data;

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleNullReferenceException
[SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.")]
public class XmlElementTests : TestBase
{
    public XmlElementTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [SkippableFact]
    public void FindChildren()
    {
        var xml = @"
<root>
  <people>
    <students>
      <gradeSchool>
        <person>A</person>
        <dog>DogA</dog>
        <person>B</person>
      </gradeSchool>
      <highSchool>
        <dog>DogB</dog>
        <person>C</person>
        <dog>DogC</dog>
        <person>D</person>
        <dog>DogD</dog>
      </highSchool>
    </students>
    <parents>
      <gradeSchool>
        <person>AA</person>
        <dog>DogAA</dog>
        <person>BB</person>
      </gradeSchool>
      <highSchool>
        <dog>DogBB</dog>
        <person>CC</person>
        <dog>DogCC</dog>
        <person>DD</person>
        <dog>DogDD</dog>
      </highSchool>
    </parents>
  </people>
</root>
";

        var element = XmlElement.FromXml(xml);
        Assert.Equal("root", element.Name);

        var children = element.FindChildren("people").ToList();
        Assert.Equal(1, children.Count);
        Assert.Equal("people", children[0].Name);

        children = element.FindChildren("people", "students").ToList();
        Assert.Equal(1, children.Count);
        Assert.Equal("students", children[0].Name);

        children = element.FindChildren("people", "parents").ToList();
        Assert.Equal(1, children.Count);
        Assert.Equal("parents", children[0].Name);

        children = element.FindChildren("people", "students", "gradeSchool").ToList();
        Assert.Equal(1, children.Count);
        Assert.Equal("gradeSchool", children[0].Name);

        children = element.FindChildren("people", "students", "gradeSchool", "person").ToList();
        Assert.Equal(2, children.Count);
        Assert.Equal("person", children[0].Name);
        Assert.Equal("A", children[0].Value);
        Assert.Equal("B", children[1].Value);

        children = element.FindChildren("people", "students", "gradeSchool", "dog").ToList();
        Assert.Equal(1, children.Count);
        Assert.Equal("dog", children[0].Name);
        Assert.Equal("DogA", children[0].Value);

        children = element.FindChildren("people", null, "gradeSchool", "dog").ToList();
        Assert.Equal(2, children.Count);
        Assert.Equal("dog", children.Select(o => o.Name).Distinct().Single());
        Assert.Equal("dog", children[0].Name);
        Assert.Equal("DogA", children[0].Value);
        Assert.Equal("dog", children[1].Name);
        Assert.Equal("DogAA", children[1].Value);

        children = element.FindChildren("people", "students", "highSchool", null).ToList();
        Assert.Equal(5, children.Count);
        Assert.Equal(2, children.Select(o => o.Name).Distinct().ToArray().Length);
        Assert.Equal(new[] { "DogB", "C", "DogC", "D", "DogD" }, children.Select(o => o.Value));
    }

    [SkippableFact]
    public void ToXml_Text_Null()
    {
        var settings = XmlElement.DefaultWriterSettings.Clone();
        settings.Indent = false;
        var element = new XmlElement("root")
        {
            Children = new List<XmlElement>
            {
                new("a") { Children = new List<XmlElement> { new("aa"), new("ab"), new("ac") } },
                new("b") { Children = new List<XmlElement> { new("ba"), new("bb"), new("bc") } },
                new("c") { Children = new List<XmlElement> { new("ca"), new("cb"), new("cc") } },
            },
        };
        var xml = element.ToXml(settings);
        Assert.Equal("<root><a><aa /><ab /><ac /></a><b><ba /><bb /><bc /></b><c><ca /><cb /><cc /></c></root>", xml);
    }

    [SkippableFact]
    public void ToXml_Text_Empty()
    {
        var settings = XmlElement.DefaultWriterSettings.Clone();
        settings.Indent = false;
        var element = new XmlElement("root")
        {
            Children = new List<XmlElement>
            {
                new("a") { Children = new List<XmlElement> { new("aa", ""), new("ab", ""), new("ac", "") } },
                new("b") { Children = new List<XmlElement> { new("ba", ""), new("bb", ""), new("bc", "") } },
                new("c") { Children = new List<XmlElement> { new("ca", ""), new("cb", ""), new("cc", "") } },
            },
        };
        var xml = element.ToXml(settings);
        Assert.Equal("<root><a><aa></aa><ab></ab><ac></ac></a><b><ba></ba><bb></bb><bc></bc></b><c><ca></ca><cb></cb><cc></cc></c></root>", xml);
    }

    [SkippableFact]
    public void Read()
    {
        var xml = @"
<root>
  <a>
    <aa>
      <aaa>1</aaa>
      <aab>2</aab>
      <aaa>3</aaa>
    </aa>
    <ab>
      <abb>4</abb>
      <aba>5</aba>
      <aba>6</aba>
    </ab>
    <aa>
      <aab>7</aab>
      <aaa>8</aaa>
    </aa>
  </a>
  <b>
    <ba>
      <baa>9</baa>
      <bab>10</bab>
      <baa>3</baa>
    </ba>
    <ba>
      <baa>11</baa>
      <bab>12</bab>
      <baa>13</baa>
    </ba>
    <bb>
      <bba>14</bba>
      <bbb>15</bbb>
      <bba>16</bba>
    </bb>
  </b>
</root>
";

        var element = XmlElement.FromXml(xml);
        Assert.Equal("root", element.Name);
        Assert.Equal(2, element.Children.Count);
        Assert.Equal("a", element[0].Name);
        Assert.Equal("b", element[1].Name);

        Assert.Equal(3, element[0].Children.Count);
        Assert.Equal("aa", element[0][0].Name);
        Assert.Equal("ab", element[0][1].Name);
        Assert.Equal("aa", element[0][2].Name);

        Assert.Equal(3, element[0].Children.Count);
        Assert.Equal("aaa", element[0][0][0].Name);
        Assert.Equal("aab", element[0][0][1].Name);
        Assert.Equal("aaa", element[0][0][2].Name);
    }

    [SkippableFact]
    public void GetAllChildren()
    {
        var xml = @"
            <root>
              <a>
                <a1><a11>a11</a11><a12>a12</a12><a13>a13</a13></a1>
                <a2><a21>a21</a21><a22>a22</a22><a23>a23</a23></a2>
                <a3><a31>a31</a31><a32>a32</a32><a33>a33</a33></a3>
              </a>
              <b>
                <b1><b11>b11</b11><b12>b12</b12><b13>b13</b13></b1>
                <b2><b21>b21</b21><b22>b22</b22><b23>b23</b23></b2>
                <b3><b31>b31</b31><b32>b32</b32><b33>b33</b33></b3>
              </b>
              <c>
                <c1><c11>c11</c11><c12>c12</c12><c13>c13</c13></c1>
                <c2><c21>c21</c21><c22>c22</c22><c23>c23</c23></c2>
                <c3><c31>c31</c31><c32>c32</c32><c33>c33</c33></c3>
              </c>
            </root>
        ";

        var element = XmlElement.FromXml(xml);
        Assert.Equal("root", element.Name);
        Assert.Equal(3, element.Children.Count);
        var childrenAll = element.GetChildrenAll().ToList();
        Assert.Equal(3 + 3 * 3 + 3 * 3 * 3, childrenAll.Count);

        var names = "x,x1,x11,x12,x13,x2,x21,x22,x23,x3,x31,x32,x33";
        names = names.Replace('x', 'a') + "," + names.Replace('x', 'b') + "," + names.Replace('x', 'c');
        var namesPart = names.Split(',');
        Assert.Equal(namesPart, childrenAll.Select(o => o.Name));
    }
}
