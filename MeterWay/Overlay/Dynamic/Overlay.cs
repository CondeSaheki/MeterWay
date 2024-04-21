using ImGuiNET;

using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;

namespace Dynamic;

public partial class Overlay : IOverlay, IOverlayConfig
{
    public static string Name => "Dynamic"; // required
    public static string Autor => "MeterWay";
    public static string Description => "(THIS CURRENTLY IS EXPERIMENTAL)\nWith the Dynamic Overlay, you're not limited by pre-set templates or fixed designs. Instead, you have the flexibility to design and adapt your overlay to suit your specific needs and preferences.\nUsing Lua Scripting whether you're a seasoned developer looking to fine-tune every aspect or a casual user seeking a personalized touch offering you endless possibilities enhancing your gaming experience and empowering you like never before.";

    private Configuration Config { get; init; }
    public LuaScript? Script { get; private set; }

    private Encounter Data = new();
    private OverlayWindow Window { get; init; }

    private static readonly LuaScript.Function[] Registers =
    [
        new ("Text", null, typeof(Overlay).GetMethod("Text"))
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

    public void Draw()
    {
        if (Script == null) return;
        Script.ExecuteDraw();
        if (!Script.Status) Window.IsOpen = false;
    }

    public void Dispose()
    {
        Script?.Dispose();
    }
}