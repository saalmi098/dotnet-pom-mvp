using Microsoft.Playwright;

namespace TestInfrastructure.Controls;

public abstract class Control(ILocator locator)
{
    public ILocator Locator { get; } = locator;

    public Task<string> TextAsync() => Locator.InnerTextAsync();
}
