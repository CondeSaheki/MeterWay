using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using MeterWay.Overlay;
using MeterWay.Windows;

namespace Mogu;

static class ConfigurationTab
{
    public static void Draw(Overlay obj)
    {
        using var bar = ImRaii.TabBar("Overlay Settings Tabs");
        if (!bar) return;

        DrawGeneralTab(obj);
        DrawAppearanceTab(obj);
        DrawFontsTab(obj);
    }

    public static void DrawGeneralTab(Overlay obj)
    {
        using var tab = ImRaii.TabItem("General");
        if (!tab) return;

        ImGui.Spacing();

        var clickThroughValue = obj.Config.ClickThrough;
        if (ImGui.Checkbox("Click Through", ref clickThroughValue))
        {
            obj.Config.ClickThrough = clickThroughValue;
            File.Save(obj._Name(), obj.Config);
            if (clickThroughValue) obj.Window.Flags = OverlayWindow.defaultflags | ImGuiWindowFlags.NoInputs;
            else obj.Window.Flags = OverlayWindow.defaultflags;
        }

        var frameCalcValue = obj.Config.FrameCalc;
        if (ImGui.Checkbox("Calculate per frame", ref frameCalcValue))
        {
            obj.Config.FrameCalc = frameCalcValue;
            File.Save(obj._Name(), obj.Config);
        }
    }

    public static void DrawAppearanceTab(Overlay obj)
    {
        using var tab = ImRaii.TabItem("Appearance");
        if (!tab) return;

        ImGui.Spacing();

        var BackgroundValue = obj.Config.Background;
        if (ImGui.Checkbox("Background", ref BackgroundValue))
        {
            obj.Config.Background = BackgroundValue;
            File.Save(obj._Name(), obj.Config);
        }

        if (obj.Config.Background)
        {
            var OverlayBackgroundColorValue = obj.Config.BackgroundColor;
            if (ImGui.ColorEdit4("Background Color", ref OverlayBackgroundColorValue,
                ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
            {
                obj.Config.BackgroundColor = OverlayBackgroundColorValue;
                File.Save(obj._Name(), obj.Config);
            }
        }
    }

    public static void DrawFontsTab(Overlay obj)
    {
        using var tab = ImRaii.TabItem("Fonts");
        if (!tab) return;

        ImGui.Spacing();
        ImGui.Text("WIP");

        // ImGui.PushItemWidth(50);
        // ImGui.PushItemWidth(50);
        // var fontScaleValue = obj.Config.FontScale;
        // if (ImGui.DragFloat("Font Scale", ref fontScaleValue, 0.01f, 1f, 5f))
        // {
        //     obj.Config.FontScale = fontScaleValue;
        //     File.Save<Configuration>(obj.Name,obj.Config);
        // }
        // ImGui.PopItemWidth();
    }
}