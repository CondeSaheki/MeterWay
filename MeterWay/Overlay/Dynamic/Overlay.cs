using ImGuiNET;

using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;

namespace Dynamic;

public partial class Overlay : IOverlay, IOverlayConfig
{
    public static string Name => "Dynamic"; // required

    private Configuration Config { get; init; }
    public LuaScript? Script { get; private set; }
    
    private Encounter Data = new();
    private OverlayWindow Window { get; init; }

    private static readonly LuaScript.Function[] Registers =
    [
        new ("HelloWorld", null, typeof(Overlay).GetMethod("HelloWorld"))
    ];

    public Overlay(OverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        Config = File.Load<Configuration>($"{Window.Name}{Window.Id}");
        Window.Flags = ImGuiWindowFlags.None;
        Window.IsOpen = false;

        if (Config.Startup) LoadScript();
    }

    public void DataUpdate()
    {
        Data = EncounterManager.Inst.CurrentEncounter();
    }

    public void Remove()
    {
        File.Delete($"{Window.Name}{Window.Id}");
    }

    public void Draw() => Script?.ExecuteDraw();

    public void Dispose()
    {
        Script?.Dispose();
    }
}