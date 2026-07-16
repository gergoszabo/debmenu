using System.Diagnostics;
using Serilog;

namespace debmenu.Logging;

public class TimedOperation(string messageTemplate, object[] args,  ILogger logger) : IDisposable
{
    public string MessageTemplate { get; } = messageTemplate;
    public object[] Args { get; } = args;
    public ILogger Logger { get; } = logger;
    public long StartedAt { get; } = Stopwatch.GetTimestamp();

    public void Dispose()
    {
        Logger.Information(MessageTemplate, Args);
    }
}