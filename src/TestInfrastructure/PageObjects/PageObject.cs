using System.Diagnostics;
using System.Reflection;
using Microsoft.Playwright;
using TestInfrastructure.Attributes;

namespace TestInfrastructure.PageObjects;

public abstract class PageObject
{
    protected PageObject(IPage page)
    {
        Page = page;
        Root = page.Locator("html");
    }

    protected PageObject(ILocator root)
    {
        Root = root;
        Page = root.Page;
    }

    public IPage Page { get; }

    public ILocator Root { get; }

    /// <summary>Creates the page object, populates attributed properties, waits until all are visible.</summary>
    public static async Task<T> CreateAsync<T>(Func<T> factory) where T : PageObject
    {
        var pageObject = factory();
        await pageObject.InitializeAsync();
        return pageObject;
    }

    /// <summary>
    /// Creates the page object via reflection (bypasses C# 'required' checks, since properties are
    /// populated by <see cref="InitializeAsync"/>), then initializes it.
    /// </summary>
    public static async Task<T> CreateAsync<T>(IPage page) where T : PageObject
    {
        var pageObject = (T)Activator.CreateInstance(typeof(T), page)!;
        await pageObject.InitializeAsync();
        return pageObject;
    }

    /// <summary>
    /// Same as <see cref="CreateAsync{T}(IPage)"/>, scoped to a container locator
    /// (e.g. a tab panel that only exists after the tab was activated).
    /// </summary>
    public static async Task<T> CreateAsync<T>(ILocator root) where T : PageObject
    {
        var pageObject = (T)Activator.CreateInstance(typeof(T), root)!;
        await pageObject.InitializeAsync();
        return pageObject;
    }

    /// <summary>Populates attributed properties and waits for each (recursively for nested page objects).</summary>
    public Task InitializeAsync() => InitializeAsync(0);

    private async Task InitializeAsync(int depth)
    {
        var name = GetType().Name;
        var total = Stopwatch.StartNew();
        PageObjectLog.Write(depth, $"creating page object {name}");

        var props = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(p => p.GetCustomAttribute<LocatorAttribute>() != null);

        foreach (var prop in props)
        {
            var locator = prop.GetCustomAttribute<LocatorAttribute>()!.Resolve(Root);
            var value = Activator.CreateInstance(prop.PropertyType, locator)
                ?? throw new InvalidOperationException($"Cannot create {prop.PropertyType} for {name}.{prop.Name}.");
            prop.SetValue(this, value);

            PageObjectLog.Write(depth + 1, $"waiting for {name}.{prop.Name} to be visible");
            var wait = Stopwatch.StartNew();
            await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            PageObjectLog.Write(depth + 1, $"{name}.{prop.Name} visible after {wait.ElapsedMilliseconds} ms");

            if (value is PageObject nested)
                await nested.InitializeAsync(depth + 1);
        }

        PageObjectLog.Write(depth, $"page object {name} ready after {total.ElapsedMilliseconds} ms");
    }
}
