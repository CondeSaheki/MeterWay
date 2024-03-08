using ImGuiNET;

using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlays;
using MeterWay.Windows;

namespace HelloWorld;

public class Overlay : MeterWayOverlay
{
    public new static string Name => "HelloWorld";
    private OverlayWindow Window {get; init; }

    private Configuration Config { get; }

    private Encounter data = new();

    public Overlay(OverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        //Window.Flags = OverlayWindow.GetFlags() | ImGuiWindowFlags.NoMove;
        Config = Load<Configuration>();
    }

    public override void DataProcess()
    {
        data = EncounterManager.Inst.CurrentEncounter();
    }

    public override void Draw()
    {
        ImGui.Text("Hello world this is a MeterWay overlay!");
        ImGui.Text($"This is the encounter name: {data.Name}");
        ImGui.Text($"This is the Config \'Enabled\' value: {(Config.Enabled ? "true" : "false")}");
    }
    public override void DrawConfiguration()
    {
        ImGui.Text("Hello world this is a MeterWay overlay configuration!");

        var enabledValue = Config.Enabled;
        if(ImGui.Checkbox("This toggles \'Enabled\'", ref enabledValue))
        {
            Config.Enabled = enabledValue;
            Save(Config);
        }
    }

    public override void Dispose() { }
}