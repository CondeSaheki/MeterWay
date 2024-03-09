using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Windows;
using MeterWay.Overlay;

namespace Lazer;

public partial class Overlay : IOverlay, IOverlayTab
{
    public  void DrawTab()
    {
        using var bar = ImRaii.TabBar("Overlay Settings Tabs");
        if (!bar) return;

        DrawGeneralTab();
        DrawAppearanceTab();
        DrawFontsTab();
    }

    public  void DrawGeneralTab()
    {
        using var tab = ImRaii.TabItem("General");
        if (!tab) return;

        ImGui.Spacing();

        var clickThroughValue = Config.ClickThrough;
        if (ImGui.Checkbox("Click Through", ref clickThroughValue))
        {
            Config.ClickThrough = clickThroughValue;
            File.Save(Name, Config);
            if (clickThroughValue) Window.Flags = OverlayWindow.defaultflags | ImGuiWindowFlags.NoInputs;
            else Window.Flags = OverlayWindow.defaultflags;
        }
    }

    public  void DrawAppearanceTab()
    {
        using var tab = ImRaii.TabItem("Appearance");
        if (!tab) return;

        ImGui.Spacing();

        var OverlayBackgroundColorValue = Config.BackgroundColor;
        if (ImGui.ColorEdit4("Background Color", ref OverlayBackgroundColorValue,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
        {
            Config.BackgroundColor = OverlayBackgroundColorValue;
            File.Save(Name, Config);
        }
    }

    public  void DrawFontsTab()
    {
        using var tab = ImRaii.TabItem("Fonts");
        if (!tab) return;

        ImGui.Spacing();

        ImGui.Text("WIP");
        // ImGui.PushItemWidth(50);
        // var fontScaleValue = Config.FontScale;
        // if (ImGui.DragFloat("Font Scale", ref fontScaleValue, 0.01f, 1f, 5f))
        // {
        //     Config.FontScale = fontScaleValue;
        //     File.Save(Name, Config);
        // }
        // ImGui.PopItemWidth();
    }
}