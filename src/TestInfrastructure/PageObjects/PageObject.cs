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

    /// <summary>Populates attributed properties and waits for each (recursively for nested page objects).</summary>
    public async Task InitializeAsync()
    {
        var props = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(p => p.GetCustomAttribute<LocatorAttribute>() != null);

        foreach (var prop in props)
        {
            var locator = prop.GetCustomAttribute<LocatorAttribute>()!.Resolve(Root);
            var value = Activator.CreateInstance(prop.PropertyType, locator)
                ?? throw new InvalidOperationException($"Cannot create {prop.PropertyType} for {GetType().Name}.{prop.Name}.");
            prop.SetValue(this, value);

            await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            if (value is PageObject nested)
                await nested.InitializeAsync();
        }
    }
}
