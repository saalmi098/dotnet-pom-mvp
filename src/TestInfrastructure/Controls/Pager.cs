using Microsoft.Playwright;

namespace TestInfrastructure.Controls;

/// <summary>Table pager showing e.g. "Rows per page: 10  1-10 of 25".</summary>
public sealed class Pager(ILocator locator) : Control(locator);
