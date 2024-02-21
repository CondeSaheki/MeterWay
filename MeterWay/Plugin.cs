using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

using MeterWay.Windows;
using MeterWay.IINACT;
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

    // meterway stuff
    public IpcClient IpcClient { get; init; }
    public EncounterManager EncounterManager { get; init; }
    private Commands Commands { get; init; }

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

        EncounterManager = new EncounterManager();
        IpcClient = new IpcClient();
        IpcClient.receivers.Add(EncounterManager.Receiver);

        MainWindow = new MainWindow();
        OverlayWindow = new OverlayWindow();
        ConfigWindow = new ConfigWindow(this.IpcClient, OverlayWindow);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        if (ConfigurationManager.Instance.Configuration.Overlay)
        {
            WindowSystem.AddWindow(OverlayWindow);
        }

        // register any overlay here
        OverlayWindow.Overlays = [new LazerOverlay(), new MoguOverlay()];

        // TODO make only Active Overlay subscribed
        foreach (var overlay in OverlayWindow.Overlays)
        {
            EncounterManager.Inst.Clients.Add(overlay.DataProcess);
        }

        Commands = new Commands(this);

        InterfaceManager.Inst.PluginInterface.UiBuilder.Draw += DrawUI;
        InterfaceManager.Inst.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    public void Dispose()
    {
        EncounterManager.Dispose();
        this.WindowSystem.RemoveAllWindows();
        IpcClient.Dispose();
        ConfigWindow.Dispose();
        MainWindow.Dispose();
        Commands.Dispose();
    }

    private void DrawUI()
    {
        this.WindowSystem.Draw();
    }

    public void DrawConfigUI()
    {
        ConfigWindow.IsOpen = true;
    }
}