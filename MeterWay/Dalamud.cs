using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace MeterWay;

public class Dalamud
{
    [PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static ICommandManager Commands { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static IDataManager GameData { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static IClientState ClientState { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static IChatGui Chat { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static ICondition Conditions { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static ITextureProvider Textures { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static IPluginLog Log { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static IPartyList PartyList { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static IDutyState Duty { get; private set; } = null!;
    //[PluginService][RequiredVersion("1.0")] public static ISigScanner SigScanner { get; private set; } = null!;
    //[PluginService][RequiredVersion("1.0")] public static IFramework Framework { get; private set; } = null!;
    //[PluginService][RequiredVersion("1.0")] public static IKeyState Keys { get; private set; } = null!;
    //[PluginService][RequiredVersion("1.0")] public static IGameGui GameGui { get; private set; } = null!;
    //[PluginService][RequiredVersion("1.0")] public static ITargetManager Targets { get; private set; } = null!;
    //[PluginService][RequiredVersion("1.0")] public static IGameInteropProvider Interop { get; private set; } = null!;

    public static void Initialize(DalamudPluginInterface pluginInterface) => pluginInterface.Create<Dalamud>();
}
