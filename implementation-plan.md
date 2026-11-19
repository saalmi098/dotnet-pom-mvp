# Implementation plan for Claude Code

Start in the empty folder and create a minimal C# 10 solution that demonstrates an attribute-based Page Object Model for the running application at `http://localhost:5178/`.

## 1. Create the solution structure

Separate reusable test infrastructure from application-specific tests:

```text
/
  PageObjectModel.sln
  src/
    TestInfrastructure/
      TestInfrastructure.csproj
      Attributes/
      Controls/
      PageObjects/
  tests/
    App.E2E.Tests/
      App.E2E.Tests.csproj
      Pages/
      Tests/
```

- `TestInfrastructure` is a class library containing attributes, typed controls, page-object initialization, and ready/visibility waits.
- `App.E2E.Tests` is an xUnit v3 test project referencing `TestInfrastructure`, Playwright for .NET, and the xUnit v3 runner.

## 2. Build the reusable infrastructure

### Selector attributes

Implement property attributes such as:

- `[TestId("...")]` to locate an element by test ID.
- Optionally, `[Role(...)]` if the demo flow needs role-based selection.

Attributes should store locator instructions. Use reflection in the page-object initializer to read them.

### Typed controls

Create small control classes that wrap an `ILocator`, for example:

- `Button` with `ClickAsync()`.
- `TextInput` with `FillAsync(string)`.
- Add a date input only if the inspected demo flow needs it.

Control constructors should accept an `ILocator`. Keep each control focused on reusable interaction behavior.

### Page objects and nested objects

- A page object receives an `IPage` or scoped `ILocator` root.
- Resolve each attributed locator relative to that root.
- For properties typed as controls, construct the control with the resolved locator.
- For properties typed as nested page objects, construct the nested object with the resolved locator as its root. This supports sections or tabs within a larger page.
- Avoid requiring each property to call `GetLocator(nameof(...))`; initialize attributed properties centrally.

### Async readiness

Constructors cannot await browser operations. Provide an async creation/initialization path that:

1. Creates and populates attributed properties.
2. Waits for each required locator to become visible.
3. Recursively initializes nested page objects where appropriate.
4. Returns the page object only after initialization succeeds.

Use this path for page objects in tests. Hidden or inactive tabs should be initialized and waited for when activated, rather than requiring every tab to be visible at initial page load.

## 3. Configure Playwright and xUnit

In the test project:

- Use an xUnit v3 test class lifecycle to create Playwright, launch a browser, and create a page.
- Navigate to `http://localhost:5178/` in the test flow.
- Close the browser and dispose Playwright after the test.
- Keep browser setup and reusable helpers in test infrastructure where practical; keep application-specific flow and assertions in the test project.

## 4. Explore the running application and implement one demo flow

Use the browser to inspect the application at `http://localhost:5178/`:

1. Identify a simple end-to-end flow with visible controls and an observable result.
2. Prefer accessible roles, labels, or stable test IDs for selectors.
3. Create application-specific page objects and typed controls needed for that flow.
4. Write one xUnit test that navigates to the application, creates the page object through its async ready path, performs the flow through page-object methods/controls, and asserts the result.
5. If the UI has tabs or nested sections relevant to the flow, model them as nested page objects scoped to their respective containers.

Keep the MVP small: implement only the infrastructure needed by the chosen flow, plus the flow and its assertions.


---

# Code Samples (for reference)

```csharp
public sealed class LoginPage(IPage page) : PageObject(page)
{
    [TestId("email")]
    public TextInput Email { get; private set; } = null!;

    [TestId("login")]
    public Button Login { get; private set; } = null!;
}

// In the test:
var login = await PageObject.CreateAsync(() => new LoginPage(page));
await login.Email.FillAsync("user@example.com");
await login.Login.ClickAsync();
```

## Attribute and typed control

```csharp
[AttributeUsage(AttributeTargets.Property)]
public sealed class TestIdAttribute(string id) : Attribute
{
    public string Id { get; } = id;
}

public sealed class Button(ILocator locator)
{
    public Task ClickAsync() => locator.ClickAsync();
}```

## Page object with a nested section

```csharp
public sealed class DetailsPage(IPage page) : PageObject(page)
{
    [TestId("details-tab-panel")]
    public DetailsTab Details { get; private set; } = null!;
}

public sealed class DetailsTab(ILocator root) : PageObject(root)
{
    [TestId("save-button")]
    public Button Save { get; private set; } = null!;
}
```

## Async creation and readiness

```csharp
var pageObject = await PageObject.CreateAsync(
    () => new DetailsPage(page));
```