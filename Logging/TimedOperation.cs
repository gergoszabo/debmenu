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

/// <summary>
/// Factory helpers for creating <see cref="TimedOperation"/> instances with a consistent
/// <c>[Class] Method</c> template, so call sites do not hand-roll the class name.
/// </summary>
public static class TimedOperationExtensions
{
    /// <summary>
    /// Creates a <see cref="TimedOperation"/> whose template is <c>[{Class}] {methodName}</c>,
    /// bound to the calling type's name and any extra arguments.
    /// </summary>
    /// <param name="logger">The logger used to emit start and duration messages.</param>
    /// <param name="callerType">The type whose simple name is reported as the class.</param>
    /// <param name="methodName">The method or operation being timed.</param>
    /// <param name="args">Optional extra structured arguments appended after the method name.</param>
    public static TimedOperation CreateTimedOperation(this ILogger logger, Type callerType, string methodName, params object[] args)
    {
        var template = $"[{{Class}}] {methodName}";
        var allArgs = new object[args.Length + 2];
        allArgs[0] = callerType.Name;
        allArgs[1] = methodName;
        Array.Copy(args, 0, allArgs, 2, args.Length);
        return new TimedOperation(template, allArgs, logger);
    }
}