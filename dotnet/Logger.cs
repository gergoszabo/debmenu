using Serilog;

namespace debmenu;

internal class Logger
{
    private ILogger _log;
    private Logger()
    {
        _log = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }

    public static Logger Instance => field ??= new Logger();

    public static void Information(string template, params object[] args)
    {
        Instance._log.Information(template, args);
    }
    
    public static void Warning(string template, params object[] args)
    {
        Instance._log.Warning(template, args);
    }

    public static void Error(string template, params object[] args)
    {
        Instance._log.Error(template, args);
    }
}