namespace AgriculturalMonitorSystem.Src.Shared.Attributes;

/// <summary>
/// Marks an action as requiring farm ownership verification.
/// Checked by FarmOwnershipMiddleware. Admins bypass this check.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequireFarmOwnershipAttribute : Attribute
{
    /// <summary>Where to find the farm ID: "route", "query"</summary>
    public string FarmIdSource { get; }

    /// <summary>Parameter name containing the farm ID (e.g. "id", "farmId")</summary>
    public string FarmIdPath { get; }

    public RequireFarmOwnershipAttribute(string source = "route", string path = "id")
    {
        FarmIdSource = source;
        FarmIdPath = path;
    }
}
