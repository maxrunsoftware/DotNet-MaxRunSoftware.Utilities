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

using System.Xml;

namespace MaxRunSoftware.Utilities.Common;

public partial class XmlElement(string name, string? value = null)
{
    public static string ValueDelimiter { get; set; } = "\n";

    public static XmlWriterSettings DefaultWriterSettings { get; } = new()
    {
        Encoding = Constant.Encoding_UTF8_Without_BOM,
        Indent = true,
        NewLineOnAttributes = false,
        OmitXmlDeclaration = true,
    };
    
    private string name = name.CheckNotNullTrimmed();
    public string Name { get => name; set => name = value.CheckNotNullTrimmed(); }

    public string? Value { get; set; } = value;
    
    private IDictionary<string, string>? attributes;
    public IDictionary<string, string> Attributes { get => attributes ??= new Dictionary<string, string>(); set => attributes = value; }

    private IList<XmlElement>? children;
    public IList<XmlElement> Children { get => children ??= new List<XmlElement>(); set => children = value; }

    public XmlElement? Parent { get; set; }

    public XmlElement this[int childIndex] => Children[childIndex];

    public string? this[string attributeName] => Attributes.GetValueCaseInsensitive(attributeName);
}

public partial class XmlElement
{
        public static XmlElement FromXml(string xml)
    {
        var document = new XmlDocument();
        document.LoadXml(xml);
        return FromXml(document);
    }

    public static XmlElement FromXml(XmlDocument document)
    {
        var elementRoot = document.DocumentElement.CheckNotNull();
        return FromXmlElement(elementRoot);
    }

    private readonly record struct FromXmlItem(XmlElement Element, List<System.Xml.XmlElement>? Children);

    private static FromXmlItem FromXmlCreateItem(System.Xml.XmlElement element)
    {
        var newElement = new XmlElement(element.Name);
        var attrs = element.Attributes;
        foreach (XmlAttribute attr in attrs) newElement.Attributes[attr.Name] = attr.Value;

        List<System.Xml.XmlElement>? newElementChildren = null;
        foreach (XmlNode child in element.ChildNodes)
        {
            if (child.NodeType is XmlNodeType.Element)
            {
                newElementChildren ??= new();
                newElementChildren.Add((System.Xml.XmlElement)child);
            }
            else if (child.NodeType is XmlNodeType.Text or XmlNodeType.CDATA)
            {
                var v = child.Value;
                if (v == null) continue;
                if (child.NodeType is XmlNodeType.Text && string.IsNullOrWhiteSpace(v)) continue;

                if (newElement.Value == null)
                {
                    newElement.Value = v;
                }
                else
                {
                    newElement.Value = newElement.Value + ValueDelimiter + v;
                }
            }
        }

        return new(newElement, newElementChildren);
    }

    private static XmlElement FromXmlElement(System.Xml.XmlElement element)
    {
        var stack = new Stack<FromXmlItem>();
        var root = FromXmlCreateItem(element);
        stack.Push(root);
        while (stack.TryPop(out var item))
        {
            var children = item.Children;
            if (children == null) continue;
            //children.Reverse();
            foreach (var child in children)
            {
                var childElement = FromXmlCreateItem(child);
                childElement.Element.Parent = item.Element;
                item.Element.Children.Add(childElement.Element);
                stack.Push(childElement);
            }
        }

        return root.Element;
    }

}
