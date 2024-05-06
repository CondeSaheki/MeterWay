using Dalamud.Plugin;
using Dalamud.Interface.Windowing;

using MeterWay.Windows;
using MeterWay.Ipc;
using MeterWay.Managers;
using MeterWay.Overlay;

namespace MeterWay;

public sealed class Plugin : IDalamudPlugin
{
    public static string Name => "MeterWay";
    
    public ConfigWindow ConfigWindow { get; private init; }
    public MainWindow MainWindow { get; private init; }
    public OverlayManager OverlayManager { get; private init; }
    
    #if DEBUG
        public DebugWindow DebugWindow { get; } = new();
    #endif

    public IINACTClient IinactIpcClient { get; private init; }

    private Commands Commands { get; init; }
    private readonly WindowSystem WindowSystem = new(Name);
    private readonly EncounterManager encounterManager;
    private readonly ConfigurationManager configurationManager;
    
    public Plugin(DalamudPluginInterface pluginInterface)
    {
        try
        {
            Dalamud.Initialize(pluginInterface);

            configurationManager = new();
            encounterManager = new();

            IinactIpcClient = new IINACTClient();
            IinactIpcClient.Connect();
            IinactIpcClient.Subscribe([IINACTClient.SubscriptionType.LogLine]);
            IinactIpcClient.OnDataReceived += EncounterManager.Receiver;

            // register your overlays here
            OverlayManager = new(
                [
                    #if DEBUG
                        typeof(HelloWorld.Overlay),
                    #endif
                    typeof(Mogu.Overlay),
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
        
        if (encounterManager != null) IinactIpcClient.OnDataReceived -= EncounterManager.Receiver;
        IinactIpcClient?.Dispose();
        encounterManager?.Dispose();
    }
}