using System;
using ImGuiNET;

using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;
using MeterWay.Windows;

namespace HelloWorld;

public class Overlay : MeterWayOverlay, IDisposable
{
    public new static string Name => "HelloWorld";
    private OverlayWindow Window { get; init; }
    public override bool HasConfigurationTab => true;

    private Configuration Config { get; }

    private Encounter data = new();

    public Overlay(OverlayWindow overlayWindow)
    {
        Config = Load<Configuration>();
        Window = overlayWindow;

        Window.Flags = ImGuiWindowFlags.None; // you can change the window properties if you want
    }

    public override void DataProcess()
    {
        data = EncounterManager.Inst.CurrentEncounter();
    }

    public override void Draw()
    {
        ImGui.Text("Hello world this is a MeterWay overlay!");
        ImGui.Spacing();
        ImGui.Text($"Encounter Name: {data.Name}");
        ImGui.Text($"Configuration \'Enabled\': {(Config.Enabled ? "true" : "false")}");
    }

    public override void DrawConfigurationTab()
    {
        ImGui.Text("Im a configuration tab!\n");

        var enabledValue = Config.Enabled;
        if (ImGui.Checkbox("\'Enabled\' Configuration", ref enabledValue))
        {
            Config.Enabled = enabledValue;
            Save(Config);
        }
    }

    public override void Dispose() { }
}