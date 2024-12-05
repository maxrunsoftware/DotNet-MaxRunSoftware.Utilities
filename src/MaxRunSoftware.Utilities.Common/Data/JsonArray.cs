namespace MaxRunSoftware.Utilities.Common;

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
