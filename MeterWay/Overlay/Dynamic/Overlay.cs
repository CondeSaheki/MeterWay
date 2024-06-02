using ImGuiNET;

using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;

namespace Dynamic;

public partial class Overlay : BasicOverlay
{
    public static BasicOverlayInfo Info => new()
    {
        Name = "Dynamic",
        Author = "MeterWay",
        Description = "(THIS CURRENTLY IS EXPERIMENTAL)\nWith the Dynamic Overlay, you're not limited by pre-set templates or fixed designs. Instead, you have the flexibility to design and adapt your overlay to suit your specific needs and preferences.\nUsing Lua Scripting whether you're a seasoned developer looking to fine-tune every aspect or a casual user seeking a personalized touch offering you endless possibilities enhancing your gaming experience and empowering you like never before.",
    };

    private Configuration Config { get; init; }
    public LuaScript? Script { get; private set; }

    private Encounter? Data;
    private IOverlayWindow Window { get; init; }

    private static readonly LuaScript.Function[] Registers =
    [
        new ("Text", null, typeof(Overlay).GetMethod("Text"))
    ];

    public Overlay(IOverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        Config = Load<Configuration>(Window.WindowName);
        Window.IsOpen = false;

        if (Config.Startup) LoadScript();
    }

    public override void OnEncounterUpdate()
    {
        Data = EncounterManager.Inst.CurrentEncounter();
    }

    public static void Remove(IOverlayWindow window)
    {
        Delete(window.WindowName);
    }

    public override void Draw()
    {
        if (Script == null) return;
        Script.ExecuteDraw();
        if (!Script.Status) Window.IsOpen = false;
    }

    public override void Dispose()
    {
        Script?.Dispose();
    }
}