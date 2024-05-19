namespace MaxRunSoftware.Utilities.Web.Server;

/// <summary>
/// Service Options
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class WebControllerAttribute(string? path = null) : Attribute
{
    public string? Path { get; set; } = path;
}
