namespace MaxRunSoftware.Utilities.Common;

public class JsonObject : JsonElement
{
    public override JsonElementType Type => JsonElementType.Object;

    public IDictionary<string, JsonElement> Properties { get; set; } = new Dictionary<string, JsonElement>();
}
