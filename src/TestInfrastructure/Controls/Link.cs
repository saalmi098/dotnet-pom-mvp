using Microsoft.Playwright;

namespace TestInfrastructure.Controls;

public sealed class Link(ILocator locator) : Control(locator);
