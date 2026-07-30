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

        // Subscribe via += syntax (strongly typed, IDE auto-complete)
        context.Hooks.OnAtMessageCreate += async (Message message) =>
        {
            Logger.LogInfo($"[{Name}] Received @ message from {message.Author?.Username}: {message.Content}");

            if (message.Content?.Contains("hello", StringComparison.OrdinalIgnoreCase) == true)
            {
                await message.Reply(content: $"Hello! I'm '{Name}' v{Version}.");
            }
        };

        context.Hooks.OnGroupMessageCreate += async (GroupMessage msg) =>
        {
            Logger.LogInfo($"[{Name}] Received group message from {msg.Author?.MemberOpenid}: {msg.Content}");

            if (msg.Content?.Contains("ping", StringComparison.OrdinalIgnoreCase) == true)
            {
                await msg.Reply(content: "pong!");
            }
        };

        context.Hooks.OnGroupMemberAdd += async (GroupManageEvent ev) =>
        {
            Logger.LogInfo($"[{Name}] New member joined group {ev.GroupOpenid}");
        };

        context.Hooks.OnReady += () =>
        {
            Logger.LogInfo($"[{Name}] Bot is ready!");
            return Task.CompletedTask;
        };

        Logger.LogInfo($"[{Name}] Plugin initialized");
        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        Logger.LogInfo($"[{Name}] Plugin shutting down");
        return Task.CompletedTask;
    }
}
