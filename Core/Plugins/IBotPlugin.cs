namespace POPYBot.Plugins;

public interface IBotPlugin
{
    string Name { get; }
    string Version { get; }
    Task InitializeAsync(IPluginContext context);
    Task ShutdownAsync();
}

public interface IPluginContext
{
    BotAPI Api { get; }
    IPluginHookManager Hooks { get; }
}

public interface IPluginHookManager
{
    void On(string eventName, Delegate handler);
    void Off(string eventName, Delegate handler);
    Task InvokeAsync(string eventName, params object?[] args);
}
