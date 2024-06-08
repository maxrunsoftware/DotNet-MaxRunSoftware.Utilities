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

using System.Xml;
using System.Xml.Xsl;

namespace MaxRunSoftware.Utilities.Common;

public class XmlElement(string name, string? value = null)
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

    #region Read

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

    #endregion Read
}

public static class XmlElementExtensions
{
    public static IEnumerable<XmlElement> GetChildrenAll(this XmlElement element)
    {
        var returnedItems = new HashSet<XmlElement>();
        var stack = new Stack<XmlElement>();
        stack.Push(element);
        while (stack.Count > 0)
        {
            var elementCurrent = stack.Pop();
            if (!returnedItems.Add(elementCurrent)) continue;
            if (elementCurrent != element) yield return elementCurrent;
            foreach (var elementCurrentChild in elementCurrent.Children.Reverse())
            {
                if (returnedItems.Contains(elementCurrentChild)) continue;
                stack.Push(elementCurrentChild);
            }
        }
    }

    public static IEnumerable<XmlElement> FindChildren(this XmlElement element, params string?[] names) => FindChildren(element, null, names);

    public static IEnumerable<XmlElement> FindChildren(this XmlElement element, IEqualityComparer<string>? comparer, params string?[] names)
    {
        var items = new List<XmlElement> { element };
        foreach (var name in names)
        {
            var itemsSub = new List<XmlElement>();
            foreach (var item in items)
            {
                itemsSub.AddRange(name == null ? item.Children : item.GetChildren(comparer, name));
            }

            items = itemsSub;
        }

        return items;
    }

    public static IEnumerable<XmlElement> GetChildren(this XmlElement element, string name) => GetChildren(element, null, name);

    public static IEnumerable<XmlElement> GetChildren(this XmlElement element, IEqualityComparer<string>? comparer, string name)
    {
        foreach (var child in element.Children)
        {
            if (comparer == null)
            {
                if (child.Name.Equals(name)) yield return child;
            }
            else
            {
                if (comparer.Equals(child.Name, name)) yield return child;
            }
        }
    }

    #region ApplyXslt

    public static string ApplyXslt(string xml, string xslt, XmlReaderSettings? readerSettings = null, XmlWriterSettings? writerSettings = null)
    {
        using var xmlReaderBuffer = new StringReader(xml);
        using var xmlReader = readerSettings == null
            ? XmlReader.Create(xmlReaderBuffer)
            : XmlReader.Create(xmlReaderBuffer, readerSettings);

        using var xsltReaderBuffer = new StringReader(xslt);
        using var xsltReader = readerSettings == null
            ? XmlReader.Create(xsltReaderBuffer)
            : XmlReader.Create(xsltReaderBuffer, readerSettings);

        var s = ApplyXslt(xmlReader, xsltReader, writerSettings);

        return s;
    }

    public static string ApplyXslt(XmlReader xml, XmlReader xslt, XmlWriterSettings? writerSettings = null)
    {
        var writerBuffer = new StringBuilder();
        using var writer = writerSettings == null
            ? XmlWriter.Create(writerBuffer)
            : XmlWriter.Create(writerBuffer, writerSettings);

        ApplyXslt(xml, xslt, writer);

        writer.Flush();

        return writerBuffer.ToString();
    }

    public static void ApplyXslt(XmlReader xml, XmlReader xslt, XmlWriter writer)
    {
        var transform = new XslCompiledTransform();
        transform.Load(xml);
        transform.Transform(xslt, writer);
        writer.Flush();
    }

    #endregion ApplyXslt

    #region ToStringXml

    private static void ToStringXml(XmlElement element, XmlWriter xmlWriter, HashSet<XmlElement> itemsSeen)
    {
        if (!itemsSeen.Add(element)) throw new ArgumentException($"Found multiple references to the same element {element}", nameof(element));

        xmlWriter.WriteStartElement(element.Name);

        foreach (var kvp in element.Attributes) xmlWriter.WriteAttributeString(kvp.Key, kvp.Value);

        if (element.Value != null) xmlWriter.WriteString(element.Value);

        foreach (var child in element.Children)
        {
            ToStringXml(child, xmlWriter, itemsSeen);
        }

        xmlWriter.WriteEndElement();
    }
    
    /// <summary>
    /// https://stackoverflow.com/a/9459212
    /// </summary>
    /// <param name="encoding"></param>
    private sealed class StringWriterWithEncoding(Encoding encoding) : StringWriter
    {
        public override Encoding Encoding => encoding;
    }
    
    public static string ToStringXml(this XmlElement element, XmlWriterSettings? settings = null)
    {
        using var stringWriter = new StringWriterWithEncoding((settings ?? XmlElement.DefaultWriterSettings).Encoding);
        ToStringXml(element, stringWriter, settings);
        stringWriter.Flush();
        return stringWriter.ToString();
    }

    public static void ToStringXml(this XmlElement element, TextWriter textWriter, XmlWriterSettings? settings = null)
    {
        using var xmlWriter = XmlWriter.Create(textWriter, settings ?? XmlElement.DefaultWriterSettings);
        var itemsSeen = new HashSet<XmlElement>();
        ToStringXml(element, xmlWriter, itemsSeen);
        xmlWriter.Flush();
        textWriter.Flush();
    }

    public static void ToStringXml(this XmlElement element, Stream stream, XmlWriterSettings? settings = null, Encoding? encoding = null, int bufferSize = -1)
    {
        encoding ??= settings?.Encoding ?? XmlElement.DefaultWriterSettings.Encoding;
        using var streamWriter = new StreamWriter(stream, encoding, bufferSize, true);
        ToStringXml(element, streamWriter, settings);
        streamWriter.Flush();
        stream.Flush();
    }

    #endregion ToStringXml
}
