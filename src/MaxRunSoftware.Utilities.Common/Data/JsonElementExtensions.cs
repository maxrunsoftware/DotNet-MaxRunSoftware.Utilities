using System.Text.Json;

namespace MaxRunSoftware.Utilities.Common;

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
