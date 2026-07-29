using POPYBot;
using POPYBot.Models;
using POPYBot.Plugins;

public class GreetingPlugin : IBotPlugin
{
    public string Name => "GreetingPlugin";
    public string Version => "1.0.0";

    private IPluginContext? _context;

    public Task InitializeAsync(IPluginContext context)
    {
        _context = context;

        // Register hook for @ message events - data is a Message object
        context.Hooks.On(HookEvents.AtMessageCreate, async (Message message) =>
        {
            Logger.LogInfo($"[{Name}] Received message from {message.Author?.Username}: {message.Content}");

            if (message.Content?.Contains("hello", StringComparison.OrdinalIgnoreCase) == true)
            {
                await message.Reply(content: $"Hello! I'm '{Name}' v{Version}.");
            }
        });

        context.Hooks.On(HookEvents.Ready, () =>
        {
            Logger.LogInfo($"[{Name}] Bot is ready!");
            return Task.CompletedTask;
        });

        Logger.LogInfo($"[{Name}] Plugin initialized");
        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        Logger.LogInfo($"[{Name}] Plugin shutting down");
        return Task.CompletedTask;
    }
}
