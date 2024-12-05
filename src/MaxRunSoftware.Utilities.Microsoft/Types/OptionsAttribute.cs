namespace MaxRunSoftware.Utilities.Common;

/// <summary>
/// Service Options
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class OptionsAttribute(string configSectionPath) : Attribute
{
    public string ConfigSectionPath { get; set; } = configSectionPath;
}
