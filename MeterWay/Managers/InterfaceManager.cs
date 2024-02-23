using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace MeterWay.Managers;

public class InterfaceManager
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


    public static InterfaceManager Inst { get; private set; } = null!;

    public InterfaceManager(WindowSystem windowsystem,
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
        WindowSystem = windowsystem;
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        PluginLog = pluginLog;
        ChatGui = chatGui;
        ClientState = clientState;
        Condition = condition;
        PartyList = partyList;
        DataManager = dataManager;
        TextureProvider = textureProvider;
        DutyState = dutyState;

        Inst = this;
    }
}