namespace AgriculturalMonitorSystem.Src.Shared.Attributes;

/// <summary>
/// Marks a controller or action as requiring specific roles.
/// Checked by RoleMiddleware (like Express.js authorize middleware).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRoleAttribute : Attribute
{
    public string[] Roles { get; }
    public AuthorizeRoleAttribute(params string[] roles) => Roles = roles;
}
