using Microsoft.Playwright;

namespace TestInfrastructure.Controls;

public sealed class Label(ILocator locator) : Control(locator);