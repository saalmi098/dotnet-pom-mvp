using Microsoft.Playwright;

namespace TestInfrastructure.Attributes;

/// <summary>Base type: stores locator instructions, resolves them against a scope.</summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class LocatorAttribute : Attribute
{
    public abstract ILocator Resolve(ILocator root);
}
