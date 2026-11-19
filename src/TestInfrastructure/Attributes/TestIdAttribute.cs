using Microsoft.Playwright;

namespace TestInfrastructure.Attributes;

/// <summary>Locate by test ID (data-testid).</summary>
public sealed class TestIdAttribute(string id) : LocatorAttribute
{
    public string Id { get; } = id;

    public override ILocator Resolve(ILocator root) => root.GetByTestId(Id);
}
