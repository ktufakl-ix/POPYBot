namespace POPYBot.Plugins;

using System.Reflection;
using System.Runtime.Loader;

public class PluginLoader
{
    private readonly string _pluginsPath;
    private readonly List<LoadedPlugin> _loadedPlugins = new();

    public IReadOnlyList<LoadedPlugin> LoadedPlugins => _loadedPlugins;

    public PluginLoader(string? pluginsPath = null)
    {
        _pluginsPath = pluginsPath ?? Path.Combine(AppContext.BaseDirectory, "plugins");
    }

    public IEnumerable<string> GetAvailablePlugins()
    {
        if (!Directory.Exists(_pluginsPath))
            yield break;

        foreach (var dll in Directory.GetFiles(_pluginsPath, "*.dll", SearchOption.AllDirectories))
            yield return dll;
    }

    public async Task<List<LoadedPlugin>> LoadPluginsAsync(IPluginContext context)
    {
        _loadedPlugins.Clear();

        if (!Directory.Exists(_pluginsPath))
        {
            Directory.CreateDirectory(_pluginsPath);
            Logger.LogInfo($"[POPYBot] Created plugins directory: {_pluginsPath}");
            return new List<LoadedPlugin>(_loadedPlugins);
        }

        var dllFiles = Directory.GetFiles(_pluginsPath, "*.dll", SearchOption.AllDirectories);
        Logger.LogInfo($"[botpy] Found {dllFiles.Length} plugin DLL(s) in {_pluginsPath}");

        foreach (var dllPath in dllFiles)
        {
            try
            {
                var plugin = await LoadPluginFromAssemblyAsync(dllPath, context);
                if (plugin != null)
                {
                    _loadedPlugins.Add(plugin);
                    Logger.LogInfo($"[botpy] Loaded plugin: {plugin.Name} v{plugin.Version}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[botpy] Failed to load plugin from {dllPath}: {ex.Message}");
            }
        }

        return new List<LoadedPlugin>(_loadedPlugins);
    }

    private async Task<LoadedPlugin?> LoadPluginFromAssemblyAsync(string assemblyPath, IPluginContext context)
    {
        var loadContext = new PluginLoadContext(assemblyPath);
        var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);

        foreach (var type in assembly.GetExportedTypes())
        {
            if (typeof(IBotPlugin).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
            {
                if (Activator.CreateInstance(type) is IBotPlugin pluginInstance)
                {
                    await pluginInstance.InitializeAsync(context);
                    return new LoadedPlugin(pluginInstance, loadContext, assemblyPath);
                }
            }
        }

        loadContext.Unload();
        return null;
    }

    public async Task ShutdownAllAsync()
    {
        foreach (var plugin in _loadedPlugins)
        {
            try
            {
                await plugin.Instance.ShutdownAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError($"[botpy] Error shutting down plugin {plugin.Name}: {ex.Message}");
            }
        }

        foreach (var plugin in _loadedPlugins)
        {
            try { plugin.LoadContext.Unload(); } catch { }
        }

        _loadedPlugins.Clear();
    }
}

public class LoadedPlugin
{
    public IBotPlugin Instance { get; }
    public AssemblyLoadContext LoadContext { get; }
    public string AssemblyPath { get; }
    public string Name => Instance.Name;
    public string Version => Instance.Version;

    public LoadedPlugin(IBotPlugin instance, AssemblyLoadContext loadContext, string assemblyPath)
    {
        Instance = instance;
        LoadContext = loadContext;
        AssemblyPath = assemblyPath;
    }
}

internal class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
            return LoadFromAssemblyPath(assemblyPath);
        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
            return LoadUnmanagedDllFromPath(libraryPath);
        return IntPtr.Zero;
    }
}
