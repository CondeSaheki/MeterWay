using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using System.Reflection;

using MeterWay.Windows;
using MeterWay.Managers;
using MeterWay.Overlay;
using MeterWay.Connection;

namespace MeterWay;

public sealed class Plugin : IDalamudPlugin
{
    public static string Name => "MeterWay";
    public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;

    public ConfigWindow ConfigWindow { get; private init; }
    public MainWindow MainWindow { get; private init; }
    public OverlayManager OverlayManager { get; private init; }
    public ConnectionManager ConnectionManager { get; private init; }

    #if DEBUG
        public DebugWindow DebugWindow { get; } = new();
    #endif

    private Commands Commands { get; init; }
    private readonly WindowSystem WindowSystem = new(Name);
    private readonly EncounterManager encounterManager;
    private readonly ConfigurationManager configurationManager;

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        try
        {
            Dalamud.Initialize(pluginInterface);
            Dalamud.Log.Info($"{Name} V{Version} Loading.");

            configurationManager = new();
            encounterManager = new();
            ConnectionManager = new() { Subscriptions = [ConnectionManager.SubscriptionType.LogLine] };
            ConnectionManager.OnDataReceived += EncounterManager.Receiver;

            // register your overlay here
            OverlayManager = new(
                [
                    #if DEBUG
                        typeof(HelloWorld.Overlay),
                    #endif
                    typeof(Mogu.Overlay),
                    typeof(Vision.Overlay),
                    typeof(Lazer.Overlay),
                    typeof(Dynamic.Overlay)
                ]
            );

            ConfigWindow = new(this);
            MainWindow = new(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            #if DEBUG
                WindowSystem.AddWindow(DebugWindow);
            #endif

            Commands = new(this);

            Dalamud.PluginInterface.UiBuilder.OpenMainUi += MainWindow.Toggle;
            Dalamud.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
            Dalamud.PluginInterface.UiBuilder.OpenConfigUi += ConfigWindow.Toggle;

            Dalamud.Log.Info($"{Name} V{Version} loaded successfully.");
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (ConfigWindow != null) Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= ConfigWindow.Toggle;
        if (WindowSystem != null) Dalamud.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        if (MainWindow != null) Dalamud.PluginInterface.UiBuilder.OpenMainUi -= MainWindow.Toggle;

        Commands?.Dispose();

        WindowSystem?.RemoveAllWindows();
        ConfigWindow?.Dispose();
        MainWindow?.Dispose();
        OverlayManager?.Dispose();

        #if DEBUG
            DebugWindow?.Dispose();
        #endif

        if (encounterManager != null && ConnectionManager != null) ConnectionManager.OnDataReceived -= EncounterManager.Receiver;
        ConnectionManager?.Dispose();
        encounterManager?.Dispose();
    }
}