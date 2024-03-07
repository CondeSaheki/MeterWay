using Dalamud.Plugin;
using Dalamud.Interface.Windowing;

using MeterWay.Windows;
using MeterWay.Ipc;
using MeterWay.Managers;
using MeterWay.Overlays;

namespace MeterWay;

public sealed class Plugin : IDalamudPlugin
{
    public static string Name => "MeterWay";
    
    public OverlayWindow OverlayWindow { get; private init; }
    public ConfigWindow ConfigWindow { get; private init; }
    public MainWindow MainWindow { get; } = new();
    
    #if DEBUG
        public DebugWindow DebugWindow { get; } = new();
    #endif

    public IINACTClient IinactIpcClient { get; private init; }

    private Commands Commands { get; init; }
    private readonly WindowSystem WindowSystem = new(Name);
    private readonly EncounterManager encounterManager = new();
    private readonly ConfigurationManager configurationManager = new();
    
    public Plugin(DalamudPluginInterface pluginInterface)
    {
        try
        {
            Dalamud.Initialize(pluginInterface);

            IinactIpcClient = new IINACTClient();
            IinactIpcClient.Connect();
            IinactIpcClient.Subscribe([IINACTClient.SubscriptionType.LogLine]);
            IinactIpcClient.Receivers.Add(EncounterManager.Receiver);

            // register your overlays here
            OverlayWindow = new OverlayWindow(
                [
                    typeof(LazerOverlay),
                    typeof(MoguOverlay)
                ]
            );
            ConfigWindow = new(this);
            
            WindowSystem.AddWindow(OverlayWindow);
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            #if DEBUG
                WindowSystem.AddWindow(DebugWindow);
            #endif

            Commands = new Commands(this);

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
        
        Commands?.Dispose();

        WindowSystem?.RemoveAllWindows();
        ConfigWindow?.Dispose();
        MainWindow?.Dispose();
        OverlayWindow?.Dispose();
        
        #if DEBUG
            DebugWindow.Dispose();
        #endif
   
        IinactIpcClient?.Dispose();
        encounterManager?.Dispose(); 
    }
}