using Microsoft.Playwright;

namespace TestInfrastructure.PageObjects;

/// <summary>Owns Playwright, browser and page for one test.</summary>
public sealed class BrowserSession : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IPage Page { get; private set; } = null!;

    public static async Task<BrowserSession> StartAsync(bool headless = true)
    {
        var session = new BrowserSession
        {
            _playwright = await Playwright.CreateAsync(),
        };

        session._browser = await session._playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless
        });

        session.Page = await session._browser.NewPageAsync();
        return session;
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }
}
