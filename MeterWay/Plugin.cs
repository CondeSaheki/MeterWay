using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

using MeterWay.Windows;
using MeterWay.Ipc;
using MeterWay.Managers;
using MeterWay.Overlays;

namespace MeterWay;

public sealed class Plugin : IDalamudPlugin
{
    public static string Name => "MeterWay";
    private readonly WindowSystem WindowSystem = new(Name);

    // windows
    public ConfigWindow ConfigWindow { get; init; }
    public MainWindow MainWindow { get; init; }
    public OverlayWindow OverlayWindow { get; init; }

    #if DEBUG
        public DebugWindow DebugWindow { get; init; }
    #endif

    // meterway stuff
    public IINACTClient IpcClient { get; init; }
    
    private Commands Commands { get; init; }
    private readonly EncounterManager encounterManager;
    private readonly InterfaceManager pluginManager;
    private readonly ConfigurationManager configurationManager;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager,
        [RequiredVersion("1.0")] IPluginLog pluginLog,
        [RequiredVersion("1.0")] IChatGui chatGui,
        [RequiredVersion("1.0")] IClientState clientState,
        [RequiredVersion("1.0")] ICondition condition,
        [RequiredVersion("1.0")] IPartyList partyList,
        [RequiredVersion("1.0")] IDataManager datamanager,
        [RequiredVersion("1.0")] IDutyState dutyState,
        [RequiredVersion("1.0")] ITextureProvider textureProvider
        )
    {
        pluginManager = new InterfaceManager(WindowSystem, pluginInterface, commandManager, pluginLog, chatGui, clientState, condition,
            partyList, datamanager, textureProvider, dutyState);

        configurationManager = new ConfigurationManager();

        encounterManager = new EncounterManager();
        IpcClient = new IINACTClient();
        IpcClient.Connect();
        IpcClient.Subscribe([IINACTClient.SubscriptionType.LogLine]);
        IpcClient.Receivers.Add(EncounterManager.Receiver);

        MainWindow = new MainWindow();
        
        // register your overlays here
        OverlayWindow = new OverlayWindow(
            [
                typeof(LazerOverlay), 
                typeof(MoguOverlay)
            ]
        );

        ConfigWindow = new ConfigWindow(IpcClient, OverlayWindow);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        if (ConfigurationManager.Inst.Configuration.OverlayEnabled) WindowSystem.AddWindow(OverlayWindow);

        #if DEBUG
            DebugWindow = new DebugWindow();
            WindowSystem.AddWindow(DebugWindow);
        #endif

        Commands = new Commands(this);

        InterfaceManager.Inst.PluginInterface.UiBuilder.Draw += DrawUI;
        InterfaceManager.Inst.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    public void Dispose()
    {
        Commands.Dispose();
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        OverlayWindow.Dispose();
        MainWindow.Dispose();
        
        #if DEBUG
            DebugWindow.Dispose();
        #endif
        
        encounterManager.Dispose();
        IpcClient.Dispose();
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    private void DrawConfigUI()
    {
        ConfigWindow.IsOpen = true;
    }
}