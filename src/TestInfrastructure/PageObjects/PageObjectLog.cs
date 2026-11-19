namespace TestInfrastructure.PageObjects;

/// <summary>
/// Timestamped debug output for page-object creation. The sink is per async flow (per test),
/// so parallel tests do not mix output. Falls back to the console when no sink is set.
/// </summary>
public static class PageObjectLog
{
    private static readonly AsyncLocal<Action<string>?> Sink = new();

    public static void SetSink(Action<string>? sink) => Sink.Value = sink;

    internal static void Write(int depth, string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] {new string(' ', depth * 2)}{message}";
        (Sink.Value ?? Console.WriteLine)(line);
    }
}
