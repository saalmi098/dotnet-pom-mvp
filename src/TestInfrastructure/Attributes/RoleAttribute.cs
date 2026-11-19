using Microsoft.Playwright;

namespace TestInfrastructure.Attributes;

/// <summary>Locate by ARIA role and optional accessible name (substring match).</summary>
public sealed class RoleAttribute(AriaRole role) : LocatorAttribute
{
    public AriaRole Role { get; } = role;

    public string? Name { get; set; }

    public override ILocator Resolve(ILocator root) =>
        root.GetByRole(Role, new LocatorGetByRoleOptions { Name = Name });
}
