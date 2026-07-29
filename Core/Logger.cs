namespace POPYBot;

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

public static class Logger
{
    private static ILoggerFactory? _loggerFactory;
    private static readonly ConcurrentDictionary<string, ILogger> _loggers = new();
    private static LogLevel _minLevel = LogLevel.Information;
    private static bool _consoleEnabled = true;

    public const string DefaultLoggerName = "botpy";

    public static void Configure(Action<ILoggingBuilder>? configure = null)
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(_minLevel);
            if (_consoleEnabled)
            {
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
            }
            configure?.Invoke(builder);
        });
    }

    public static void SetLevel(LogLevel level)
    {
        _minLevel = level;
        _loggerFactory?.Dispose();
        _loggerFactory = null;
        Configure();
    }

    public static void EnableConsole(bool enable)
    {
        _consoleEnabled = enable;
        _loggerFactory?.Dispose();
        _loggerFactory = null;
        Configure();
    }

    public static ILogger GetLogger(string name = DefaultLoggerName)
    {
        if (_loggerFactory == null)
            Configure();

        return _loggers.GetOrAdd(name, n => _loggerFactory!.CreateLogger(n));
    }

    public static void LogDebug(string message)
    {
        if (_minLevel <= LogLevel.Debug)
            GetLogger().LogDebug(message);
    }

    public static void LogInfo(string message)
    {
        GetLogger().LogInformation(message);
    }

    public static void LogWarning(string message)
    {
        GetLogger().LogWarning(message);
    }

    public static void LogError(string message)
    {
        GetLogger().LogError(message);
    }
}
