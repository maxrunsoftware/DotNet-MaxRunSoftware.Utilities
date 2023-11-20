// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

using System.Text.Json;

namespace MaxRunSoftware.Utilities.Common;

public enum JsonElementType { Value, Array, Object }

public abstract class JsonElement
{
    public static JsonWriterOptions DefaultWriterOptions => new()
    {
        Indented = true,
    };

    public abstract JsonElementType Type { get; }

}

public class JsonValue : JsonElement
{
    public override JsonElementType Type => JsonElementType.Value;

    public string? Value { get; set; }

    public JsonValue() : this(null) {}
    public JsonValue(string? value) => Value = value;
}

public class JsonArray : JsonElement
{
    public override JsonElementType Type => JsonElementType.Array;

    public IList<JsonElement> Items { get; set; } = new List<JsonElement>();

    public JsonArray(params string?[]? values)
    {
        foreach (var value in values.OrEmpty())
        {
            Items.Add(new JsonValue(value));
        }
    }
}

public class JsonObject : JsonElement
{
    public override JsonElementType Type => JsonElementType.Object;

    public IDictionary<string, JsonElement> Properties { get; set; } = new Dictionary<string, JsonElement>();
}

public static class JsonElementExtensions
{
    public static string ToJson(this JsonArray array, JsonWriterOptions? options = null)
    {
        options ??= JsonElement.DefaultWriterOptions;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, options.Value);
        array.ToJson(writer);
        writer.Flush();
        stream.Flush();
        var str = Encoding.UTF8.GetString(stream.ToArray());
        return str;
    }

    public static void ToJson(this JsonArray array, Utf8JsonWriter writer) => array.ToJson(writer, null);

    private static void ToJson(this JsonArray array, Utf8JsonWriter writer, string? propertyName)
    {
        if (propertyName == null)
        {
            writer.WriteStartArray();
        }
        else
        {
            writer.WriteStartArray(propertyName);
        }

        foreach (var arrayItem in array.Items)
        {
            if (arrayItem is JsonArray subArray)
            {
                subArray.ToJson(writer);
            }
            else if (arrayItem is JsonObject subObject)
            {
                subObject.ToJson(writer);
            }
            else if (arrayItem is JsonValue subValue)
            {
                if (subValue.Value == null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    writer.WriteStringValue(subValue.Value);
                }
            }
        }

        writer.WriteEndArray();
    }

    public static string ToJson(this JsonObject obj, JsonWriterOptions? options = null)
    {
        options ??= JsonElement.DefaultWriterOptions;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, options.Value);
        obj.ToJson(writer);
        writer.Flush();
        stream.Flush();
        var str = Encoding.UTF8.GetString(stream.ToArray());
        return str;
    }

    public static void ToJson(this JsonObject obj, Utf8JsonWriter writer) => obj.ToJson(writer, null);

    private static void ToJson(this JsonObject obj, Utf8JsonWriter writer, string? propertyName)
    {
        if (propertyName == null)
        {
            writer.WriteStartObject();
        }
        else
        {
            writer.WriteStartObject(propertyName);
        }

        foreach (var kvp in obj.Properties)
        {
            if (kvp.Value is JsonArray subArray)
            {
                subArray.ToJson(writer, kvp.Key);
            }
            else if (kvp.Value is JsonObject subObject)
            {
                subObject.ToJson(writer, kvp.Key);
            }
            else if (kvp.Value is JsonValue subValue)
            {
                if (subValue.Value == null)
                {
                    writer.WriteNull(kvp.Key);
                }
                else
                {
                    writer.WriteString(kvp.Key, subValue.Value);
                }
            }
        }

        writer.WriteEndObject();
    }


}
