using Microsoft.Playwright;

namespace TestInfrastructure.Controls;

public sealed class Button(ILocator locator) : Control(locator)
{
    public Task ClickAsync() => Locator.ClickAsync();
}
