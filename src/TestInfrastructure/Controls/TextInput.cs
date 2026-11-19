using Microsoft.Playwright;

namespace TestInfrastructure.Controls;

public sealed class TextInput(ILocator locator) : Control(locator)
{
    public Task FillAsync(string value) => Locator.FillAsync(value);

    public Task<string> ValueAsync() => Locator.InputValueAsync();
}
