using ImGuiNET;

using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;
using MeterWay.Windows;

namespace HelloWorld;

public class Overlay : IOverlay, IOverlayTab
{
    public static string Name => "HelloWorld"; // required

    private Configuration Config { get; init; }

    private Encounter Data = new();

    public Overlay(OverlayWindow overlayWindow)
    {
        Config = File.Load<Configuration>(Name);
        overlayWindow.Flags = ImGuiWindowFlags.None; // you can change the window properties if you want
    }

    // this function is called wenever you need to update data
    public void DataUpdate()
    {
        Data = EncounterManager.Inst.CurrentEncounter();
    }

    public void Draw()
    {
        ImGui.Text("Hello world this is a MeterWay overlay!");
        ImGui.Spacing();
        ImGui.Text($"Encounter Name: {Data.Name}");
        ImGui.Text($"\'Enabled\' Configuration: {(Config.Enabled ? "true" : "false")}");
    }

    public void DrawTab()
    {
        ImGui.Text("I'm a configuration tab!\n");

        var enabledValue = Config.Enabled;
        if (ImGui.Checkbox("\'Enabled\' Configuration", ref enabledValue))
        {
            Config.Enabled = enabledValue;
            File.Save(Name, Config);
        }
    }

    public void Dispose() { }
}