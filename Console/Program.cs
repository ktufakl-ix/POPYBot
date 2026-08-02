using POPYBot;

try
{
    PrintBanner();

    // Step 1: Load configuration
    Status("Loading config.json...");
    var config = ConfigManager.Load();

    if (string.IsNullOrEmpty(config.AppId) || config.AppId == "YOUR_APP_ID")
    {
        Error("AppId is not configured. Edit config.json with your bot credentials.");
        Error("Obtain AppId and Secret from https://q.qq.com/");
        return 1;
    }
    if (string.IsNullOrEmpty(config.Secret) || config.Secret == "YOUR_SECRET")
    {
        Error("Secret is not configured. Edit config.json with your bot credentials.");
        return 1;
    }

    var intents = config.Intents ?? Intents.PublicGuildMessages;
    Status($"Config loaded. Sandbox: {config.IsSandbox}, Timeout: {config.Timeout}s");

    // Step 2: Show plugin directory
    var pluginsPath = config.PluginsPath ?? Path.Combine(AppContext.BaseDirectory, "plugins");
    if (!Directory.Exists(pluginsPath))
    {
        Directory.CreateDirectory(pluginsPath);
        Status($"Created plugins directory: {pluginsPath}");
    }
    var pluginDlls = Directory.GetFiles(pluginsPath, "*.dll", SearchOption.AllDirectories);
    Status($"Plugins directory: {pluginsPath} ({pluginDlls.Length} DLL(s) found)");

    // Step 3: Start the bot
    Info($"Starting bot - AppId: {config.AppId}, Intents: {intents}");

    var client = new Client(intents, config.Timeout, config.IsSandbox);

    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        Info("Ctrl+C pressed — shutting down...");
    };

    // Run the bot
    client.Run(config.AppId, config.Secret);

    Info("Bot stopped.");
    return 0;
}
catch (Exception ex)
{
    Error($"Fatal: {ex.Message}");
    if (ex.InnerException != null)
        Error($"  Inner: {ex.InnerException.Message}");
    return 1;
}

// ─── helpers ──────────────────────────────────────────────

static void PrintBanner()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔══════════════════════════════════════╗");
    Console.WriteLine("║                             POPYBot - QQ Bot SDK                           ║");
    Console.WriteLine("║                         Plugin-powered bot framework                       ║");
    Console.WriteLine("║                             .NET 10   |   C# 13                            ║");
    Console.WriteLine("╚══════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

static void Status(string msg) => WriteColored(ConsoleColor.DarkGray, "  → ", msg);
static void Info(string msg) => WriteColored(ConsoleColor.White, "  ℹ ", msg);
static void Error(string msg) => WriteColored(ConsoleColor.Red, "  ✖ ", msg);

static void WriteColored(ConsoleColor color, string prefix, string msg)
{
    var saved = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.Write(prefix);
    Console.ForegroundColor = saved;
    Console.WriteLine(msg);
}
