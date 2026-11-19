using Microsoft.Playwright;

namespace TestInfrastructure.Controls;

/// <summary>A tab header. The located element may be the tab itself or a child of it.</summary>
public sealed class Tab(ILocator locator) : Control(locator)
{
    private ILocator TabElement => Locator.Locator("xpath=ancestor-or-self::*[@role='tab'][1]");

    public Task SelectAsync() => Locator.ClickAsync();

    public async Task<bool> IsSelectedAsync() =>
        await TabElement.GetAttributeAsync("aria-selected") == "true";
}
