using System.Text;
using NLog;

namespace Nananet.Core.Utils;

public enum LogLevel
{
    Verbose = 1,
    Debug = 2,
    Info = 3,
    Warn = 4,
    Error = 5,
    Disabled = 10,
}

public class Logger
{
    protected static Logger s_instance = new Logger();
    
    public static Logger L
    {
        get => s_instance;
        set => s_instance = value;
    }

    private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

    // protected LogLevel _logLevel = LogLevel.Error;
    // public LogLevel LogLevel
    // {
    //     get => _logLevel;
    //     set => _logLevel = value;
    // }

    protected StringBuilder Stamp(LogLevel logLevel = LogLevel.Disabled)
    {
        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("[yy-MM-dd HH:mm:ss]"));
        sb.Append($"[{logLevel.ToString()}]");
        return sb;
    }
    
    public void Log(object? obj, LogLevel level = LogLevel.Debug)
    {
        _logger.Log(ToNLogLevel(level), obj);
    }

    private static NLog.LogLevel ToNLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Verbose => NLog.LogLevel.Trace,
            LogLevel.Debug => NLog.LogLevel.Debug,
            LogLevel.Info => NLog.LogLevel.Info,
            LogLevel.Warn => NLog.LogLevel.Warn,
            LogLevel.Error => NLog.LogLevel.Error,
            LogLevel.Disabled => NLog.LogLevel.Off,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }

    public void Verbose(object? obj) => Log(obj, LogLevel.Verbose);
    public void Debug(object? obj) => Log(obj);
    public void Info(object? obj) => Log(obj, LogLevel.Info);
    public void Warn(object? obj) => Log(obj, LogLevel.Warn);
    public void Error(object? obj) => Log(obj, LogLevel.Error);
    
}