using System;
using System.Numerics;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

using MeterWay.Overlay;

namespace Lazer;

[Serializable]
public class Appearance()
{
    public float Spacing { get; set; } = 2f;

    public uint BackgroundColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.25f));
    public uint HeaderBackgroundColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(26f / 255f, 26f / 255f, 26f / 255f, 192f / 255f));
    public uint YourBarColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(128f / 255f, 170f / 255f, 128f / 255f, 255f / 255f));
    public uint BarColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0.5f, 0.5f, 170f / 255f, 1f));
    public uint BarBorderColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(26f / 255f, 26f / 255f, 26f / 255f, 222f / 255f));
    public uint JobIconBorderColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(30f / 255f, 30f / 255f, 30f / 255f, 190f / 255f));
    public uint JobNameTextColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(144f / 255f, 144f / 255f, 144f / 255f, 1f));
    public uint TotalDamageTextColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(210f / 255f, 210f / 255f, 210f / 255f, 1f));

    // color lost during refactor, sory
    // public uint Color8 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(80f / 255f, 80f / 255f, 80f / 255f, 194f / 255f));
}

public partial class Overlay : BasicOverlay
{
    private void DrawAppearanceTab()
    {
        using var tab = ImRaii.TabItem("Appearance");
        if (!tab) return;

        using var bar = ImRaii.TabBar("Overlay Appearance Tabs");
        if (!bar) return;

        DrawAppearanceGeralTab();
        DrawFontsTab();
    }

    private void DrawAppearanceGeralTab()
    {
        using var tab = ImRaii.TabItem("General");
        if (!tab) return;

        ImGui.Spacing();
        ImGui.Text("Customize how the Lazer looks");
        ImGui.Spacing();

        ImGui.PushItemWidth(70);
        float SpacingValue = Config.Appearance.Spacing;
        if (ImGui.DragFloat("Spacing", ref SpacingValue, 0.1f, 0.1f, 3600f))
        {
            Config.Appearance.Spacing = SpacingValue;
            Save(Window.WindowName, Config);
        }
        ImGui.PopItemWidth();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        _ImGuiColorPick("Background", Config.Appearance.BackgroundColor, (value) =>
        {
            Config.Appearance.BackgroundColor = value;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Header Background", Config.Appearance.HeaderBackgroundColor, (value) =>
        {
            Config.Appearance.HeaderBackgroundColor = value;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Your Bar", Config.Appearance.YourBarColor, (value) =>
        {
            Config.Appearance.YourBarColor = value;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Bar", Config.Appearance.BarColor, (value) =>
        {
            Config.Appearance.BarColor = value;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Bar Border", Config.Appearance.BarBorderColor, (value) =>
        {
            Config.Appearance.BarBorderColor = value;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Job Name Text", Config.Appearance.JobNameTextColor, (value) =>
        {
            Config.Appearance.JobNameTextColor = value;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Job Icon Border", Config.Appearance.JobIconBorderColor, (value) =>
        {
            Config.Appearance.JobIconBorderColor = value;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Total Damage Text", Config.Appearance.TotalDamageTextColor, (value) =>
        {
            Config.Appearance.TotalDamageTextColor = value;
            Save(Window.WindowName, Config);
        });
    }
}