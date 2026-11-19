using System.Diagnostics;
using Serilog;

namespace debmenu.Logging;

/// <summary>
/// Logs the start and elapsed duration of a scoped operation when disposed.
/// </summary>
public class TimedOperation : IDisposable
{
    /// <summary>The structured log message template for the operation.</summary>
    public string MessageTemplate { get; }

    /// <summary>Structured logging arguments bound to the message template.</summary>
    public object[] Args { get; }

    /// <summary>The logger used to emit start and duration messages.</summary>
    public ILogger Logger { get; }

    /// <summary>High-resolution stopwatch timestamp captured when the operation started.</summary>
    public long StartedAt { get; } = Stopwatch.GetTimestamp();

    /// <summary>
    /// Initializes a new instance and logs that the operation has started.
    /// </summary>
    /// <param name="messageTemplate">The structured log message template (Serilog format).</param>
    /// <param name="args">Structured logging arguments for the template.</param>
    /// <param name="logger">The logger used to emit start and duration messages.</param>
    public TimedOperation(string messageTemplate, object[] args, ILogger logger)
    {
        MessageTemplate = messageTemplate;
        Args = args;
        Logger = logger;
        Logger.Information($"{MessageTemplate} started", Args);
    }

    /// <summary>
    /// Logs the elapsed duration of the operation.
    /// </summary>
    public void Dispose()
    {
        Logger.Information($"{MessageTemplate} took {Stopwatch.GetElapsedTime(StartedAt)}", Args);
    }
}