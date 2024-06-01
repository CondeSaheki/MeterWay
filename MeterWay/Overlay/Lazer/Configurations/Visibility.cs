using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Overlay;

namespace Lazer;

[Serializable]
public class Visibility()
{
    public bool Always { get; set; } = false;
    public bool Enabled { get; set; } = true;
    public bool Combat { get; set; } = true;
    public bool Delay { get; set; } = true;
    public TimeSpan DelayDuration { get; set; } = TimeSpan.FromSeconds(30);
}

public partial class Overlay : BasicOverlay
{
    private void DrawVisibilityTab()
    {
        using var tab = ImRaii.TabItem("Visibility");
        if (!tab) return;

        bool isDisabled = false;

        ImGui.Spacing();
        ImGui.Text("Adjust when the overlay is visible");
        ImGui.Spacing();
        
        _ImguiCheckboxWithTooltip("Always Show", "Always show this overlay.", Config.Visibility.Always, (value) =>
        {
            Config.Visibility.Always = value;
            Save(Window.WindowName, Config);
        });

        ImGui.Indent();
        if (Config.Visibility.Always) _ImGuiBeginDisabled(ref isDisabled);

        _ImguiCheckboxWithTooltip("When enabled", "Imediatly show this overlay when it gets enabled", Config.Visibility.Enabled, (value) =>
        {
            Config.Visibility.Enabled = value;
            Save(Window.WindowName, Config);
        });

        _ImguiCheckboxWithTooltip("When In Combat", "Show this overlay during combat.", Config.Visibility.Combat, (value) =>
        {
            Config.Visibility.Combat = value;
            Save(Window.WindowName, Config);
        });

        ImGui.Indent();
        if (!Config.Visibility.Combat) _ImGuiBeginDisabled(ref isDisabled);

        _ImguiCheckboxWithTooltip("Hide Delay", "Automatically hide this overlay after a certain duration of combat ends.", Config.Visibility.Delay, (value) =>
        {
            Config.Visibility.Delay = value;
            Save(Window.WindowName, Config);
        });
        ImGui.SameLine();
        ImGui.PushItemWidth(70);
        float delayDurationValue = (float)Config.Visibility.DelayDuration.TotalSeconds;
        if (ImGui.DragFloat("##DelaySecond", ref delayDurationValue, 0.1f, 0.1f, 3600f))
        {
            Config.Visibility.DelayDuration = TimeSpan.FromSeconds(delayDurationValue);
            Save(Window.WindowName, Config);
        }
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.Text("Seconds");

        _ImGuiEndDisabled(ref isDisabled);
        ImGui.Unindent();
        ImGui.Unindent();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button(Window.IsOpen ? "Hide" : "Show", new Vector2(240, 24))) Window.IsOpen = !Window.IsOpen;
    }
}