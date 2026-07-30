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

        // Register hook for @ message events
        context.Hooks.On(HookEvents.AtMessageCreate, async (Message message) =>
        {
            Logger.LogInfo($"[{Name}] Received @ message from {message.Author?.Username}: {message.Content}");

            if (message.Content?.Contains("hello", StringComparison.OrdinalIgnoreCase) == true)
            {
                await message.Reply(content: $"Hello! I'm '{Name}' v{Version}.");
            }
        });

        // Register hook for group message events (needs GROUP_MESSAGE_CREATE intent)
        context.Hooks.On(HookEvents.GroupMessageCreate, async (GroupMessage msg) =>
        {
            Logger.LogInfo($"[{Name}] Received group message from {msg.Author?.MemberOpenid}: {msg.Content}");

            if (msg.Content?.Contains("ping", StringComparison.OrdinalIgnoreCase) == true)
            {
                await msg.Reply(content: "pong!");
            }
        });

        // Register hook for group member join events
        context.Hooks.On(HookEvents.GroupMemberAdd, async (GroupManageEvent ev) =>
        {
            Logger.LogInfo($"[{Name}] New member joined group {ev.GroupOpenid}");
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
