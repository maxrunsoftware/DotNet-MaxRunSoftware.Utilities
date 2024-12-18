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

/// <summary>
///     TODO: Perhaps use...<br />
///     https://laurentkempe.com/2019/02/18/dynamically-compile-and-run-code-using-dotNET-Core-3.0/ <br />
///     https://stackoverflow.com/questions/71474900/dynamic-compilation-in-net-core-6 <br />
/// </summary>
[PublicAPI]
public abstract class YamlReader
{
    /*
    public static List<YamlReaderItem> Read(string yaml)
    {
        // https://laurentkempe.com/2019/02/18/dynamically-compile-and-run-code-using-dotNET-Core-3.0/
        // https://stackoverflow.com/questions/71474900/dynamic-compilation-in-net-core-6

        var db = new DeserializerBuilder();
        var ds = db.Build();
        var tr = new StringReader(yaml);
        var items = new List<object?>();
        // https://github.com/aaubry/YamlDotNet/issues/12
        var reader = new Parser(tr);
        reader.Consume<StreamStart>();
        while (reader.TryConsume<DocumentStart>(out _))
        {
            var item = ds.Deserialize<object?>(reader);
            items.Add(item);
            reader.TryConsume<DocumentEnd>(out _);
        }
        return items.Select(o => new YamlReaderItem(o)).ToList();
    }
    */

    /// <summary>
    /// Use this method to read (potentially) multiple documents from a YAML file.<br />
    /// Implement the code in the example code block.<br />
    /// https://github.com/aaubry/YamlDotNet/issues/12
    /// <see href="https://github.com/aaubry/YamlDotNet/issues/12"/>
    /// </summary>
    /// <param name="yaml">The raw YAML string</param>
    /// <returns>a list of YamlReaderItems</returns>
    /// <example>
    /// <code>
    ///   var db = new DeserializerBuilder();
    ///   var ds = db.Build();
    ///   var tr = new StringReader(yaml);
    ///   var items = new List&lt;object?&gt;();
    ///   var reader = new Parser(tr);
    ///   reader.Consume&lt;StreamStart&gt;();
    ///   while (reader.TryConsume&lt;DocumentStart&gt;(out _))
    ///   {
    ///       var item = ds.Deserialize&lt;object?&gt;(reader);
    ///       items.Add(item);
    ///       reader.TryConsume&lt;DocumentEnd&gt;(out _);
    ///   }
    ///   return items.Select(o => new YamlReaderItem(o)).ToList();
    /// </code>
    /// </example>
    public abstract List<YamlReaderItem> Read(string yaml);
}

[PublicAPI]
public enum YamlReaderItemType { Dictionary, List, Value }

[PublicAPI]
public class YamlReaderItem
{
    public object? Value { get; }

    public YamlReaderItemType Type { get; }
    public IReadOnlyDictionary<string, YamlReaderItem>? Dictionary { get; }
    public IReadOnlyList<YamlReaderItem>? List { get; }
    public string? String { get; }

    public YamlReaderItem(object? obj)
    {
        Value = obj;

        static bool IsList(object obj)
        {
            // https://stackoverflow.com/a/17190236
            var isList = obj is IList && obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
            return isList;
        }

        static bool IsDictionary(object obj)
        {
            // https://stackoverflow.com/a/17190236
            var isDictionary = obj is IDictionary && obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
            return isDictionary;
        }

        if (obj == null)
        {
            Type = YamlReaderItemType.Value;
        }
        else if (IsDictionary(obj))
        {
            Type = YamlReaderItemType.Dictionary;
            var dic = new Dictionary<string, YamlReaderItem>();
            if (obj is IDictionary<object?, object?> dictionaryGeneric)
            {
                foreach (var kvp in dictionaryGeneric)
                {
                    var k = kvp.Key?.ToString();
                    if (k == null) continue;
                    var v = kvp.Value;
                    var vyi = new YamlReaderItem(v);
                    dic[k] = vyi;
                }
            }
            else if (obj is IDictionary dictionaryNotGeneric)
            {
                foreach (var key in dictionaryNotGeneric.Keys)
                {
                    if (key == null) continue;
                    var v = dictionaryNotGeneric[key];
                    var k = key.ToString();
                    if (k == null) continue;
                    var vyi = new YamlReaderItem(v);
                    dic[k] = vyi;
                }
            }
            else
            {
                throw new NotImplementedException($"Type {obj.GetType().FullNameFormatted()} is not a dictionary");
            }

            Dictionary = dic.AsReadOnly();
        }
        else if (IsList(obj))
        {
            Type = YamlReaderItemType.List;
            var list = new List<YamlReaderItem>();
            foreach (var item in (IEnumerable)obj)
            {
                var vyi = new YamlReaderItem(item);
                list.Add(vyi);
            }

            List = list;
        }
        else
        {
            Type = YamlReaderItemType.Value;
            String = obj.ToString();
        }
    }


    private static readonly ImmutableHashSet<char> DICTIONARY_KEY_NOT_ESCAPE_CHARSET = ("-_" + Constant.Chars_Alphanumeric_String).ToCharArray().ToImmutableHashSet();
    public override string? ToString()
    {
        string FormatValue(string? s, bool dictKey)
        {
            if (s == null) return "null";
            if (s.EqualsOrdinalIgnoreCase("true") || s.EqualsOrdinalIgnoreCase("false")) return s;
            if (s.ToIntTry(out _)) return s;
            if (s.ToFloatTry(out _)) return s;
            if (s.ToDoubleTry(out _)) return s;
            if (s.ToDecimalTry(out _)) return s;
            if (dictKey && s.All(c => DICTIONARY_KEY_NOT_ESCAPE_CHARSET.Contains(c))) return s;
            return "\"" + s + "\"";
        }

        string FormatKvp(KeyValuePair<string, YamlReaderItem> kvp) => $"{FormatValue(kvp.Key, true)}: {kvp.Value}";

        if (Type == YamlReaderItemType.Value) return FormatValue(String, false);
        if (Type == YamlReaderItemType.List) return "[" + List!.Select(o => o.ToString()).ToStringDelimited(", ") + "]";
        if (Type == YamlReaderItemType.Dictionary) return "{" + Dictionary!.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase).Select(FormatKvp).ToStringDelimited(", ") + "}";
        return base.ToString();
    }
}
