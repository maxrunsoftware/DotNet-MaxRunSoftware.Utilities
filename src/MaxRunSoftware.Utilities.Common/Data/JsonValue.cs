namespace MaxRunSoftware.Utilities.Common;

public class JsonValue(string? value) : JsonElement
{
    public override JsonElementType Type => JsonElementType.Value;

    public string? Value { get; set; } = value;

    public JsonValue() : this(null) { }
}
