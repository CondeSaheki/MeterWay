using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Meterway.Managers;

public class PluginManager
{
    public readonly WindowSystem WindowSystem;
    public readonly DalamudPluginInterface PluginInterface;
    public readonly ICommandManager CommandManager;
    public readonly IPluginLog PluginLog;
    public readonly IChatGui ChatGui;
    public readonly IClientState ClientState;
    public readonly ICondition Condition;
    public readonly IPartyList PartyList;
    public readonly IDataManager DataManager;
    public readonly ITextureProvider TextureProvider;
    public readonly IDutyState DutyState;


    public static PluginManager Instance { get; private set; } = null!;

    public PluginManager(WindowSystem windowsystem,
        DalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IPluginLog pluginLog,
        IChatGui chatGui,
        IClientState clientState,
        ICondition condition,
        IPartyList partyList,
        IDataManager dataManager,
        ITextureProvider textureProvider,
        IDutyState dutyState
    )
    {
        this.WindowSystem = windowsystem;
        this.PluginInterface = pluginInterface;
        this.CommandManager = commandManager;
        this.PluginLog = pluginLog;
        this.ChatGui = chatGui;
        this.ClientState = clientState;
        this.Condition = condition;
        this.PartyList = partyList;
        this.DataManager = dataManager;
        this.TextureProvider = textureProvider;
        this.DutyState = dutyState;

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