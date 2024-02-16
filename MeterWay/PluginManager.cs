using Dalamud.Game.Command;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Numerics;

using MeterWay;

namespace MeterWay.managers;

public class PluginManager
{

    //private readonly ConfigWindow _configRoot;


    // dalamud interfaces
    public readonly DalamudPluginInterface PluginInterface;
    public readonly ICommandManager CommandManager;
    public readonly IPluginLog PluginLog;
    public readonly IChatGui ChatGui;
    public readonly IClientState ClientState;
    public readonly ICondition Condition;
    public readonly IPartyList PartyList;
    public readonly ITextureProvider TextureProvider;

    // // sus
    // public Configuration Configuration { get; init; }
    // public WindowSystem WindowSystem = new("MeterWay");

    // // window
    // private ConfigWindow ConfigWindow { get; init; }
    // private MainWindow MainWindow { get; init; }
    // public OverlayWindow OverlayWindow { get; init; }

    // // meterway stuff
    // public IINACTIpcClient IpcClient { get; init; }
    // public DataManager dataManager { get; init; }

    // private const string CommandName = "/meterway";
    // private List<MeterWayCommand> commands { get; init; }

    public static PluginManager Instance { get; private set; } = null!;

    public PluginManager(
        DalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IPluginLog pluginLog,
        IChatGui chatGui,
        IClientState clientState,
        ICondition condition,
        IPartyList partyList,
        ITextureProvider textureProvider
    )
    {
        this.PluginInterface = pluginInterface;
        this.CommandManager = commandManager;
        this.PluginLog = pluginLog;
        this.ChatGui = chatGui;
        this.ClientState = clientState;
        this.Condition = condition;
        this.PartyList = partyList;
        this.TextureProvider = textureProvider;

        PluginManager.Instance = this;
    }
}

public class ConfigurationManager
{
    public readonly Configuration Configuration;

    public static ConfigurationManager Instance { get; private set; } = null!;

    public ConfigurationManager()
    {
        this.Configuration = PluginManager.Instance.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.Configuration.Initialize(PluginManager.Instance.PluginInterface);
        
        ConfigurationManager.Instance = this;
    }
}

public class WindowManager : IDisposable
{

    private readonly WindowSystem windowsystem;

    public static WindowManager Instance { get; private set; } = null!;

    public WindowManager(WindowSystem windowsystem)
    {
        this.windowsystem = windowsystem;
        

        WindowManager.Instance = this;
    }

    public void Dispose()
    {
        windowsystem.RemoveAllWindows();
    }
    
}