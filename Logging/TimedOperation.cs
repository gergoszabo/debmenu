using System.Diagnostics;
using Serilog;

namespace debmenu.Logging;

internal sealed class TimedOperation : IDisposable
{
    public string MessageTemplate { get; }
    public object[] Args { get; }
    public ILogger Logger { get; }
    public long StartedAt { get; } = Stopwatch.GetTimestamp();

    public TimedOperation(string messageTemplate, object[] args, ILogger logger)
    {
        MessageTemplate = messageTemplate;
        Args = args;
        Logger = logger;
        Logger.Information($"{MessageTemplate} started", Args);
    }

    public void Dispose()
    {
        Logger.Information($"{MessageTemplate} took {Stopwatch.GetElapsedTime(StartedAt).TotalMilliseconds} ms", Args);
    }
}