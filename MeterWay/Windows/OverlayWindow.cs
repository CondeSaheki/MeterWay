using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;

using Meterway.Overlays;

namespace Meterway.Windows;

public class OverlayWindow : Window, IDisposable
{
    private Plugin plugin;
    private ImGuiWindowFlags DefaultFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground;

    private List<IMeterwayOverlay> overlays;
    public ImFontPtr font;

    public OverlayWindow(Plugin plugin) : base("OverlayWindow")
    {
        this.plugin = plugin;

        // window configs
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(80, 45),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.IsOpen = true;
        this.RespectCloseHotkey = false;
        this.Flags = Gerateflags();

        this.font = ImGui.GetIO().Fonts.AddFontFromFileTTF(this.plugin.Configuration.OverlayFontPath, this.plugin.Configuration.OverlayFontSize);

        this.overlays = [new LazerOverlay()];

    }

    // draw

    public override void Draw()
    {
        // background
        if (this.plugin.Configuration.OverlayBackground)
        {
            Background(this.plugin.Configuration.OverlayBackgroundColor);
        }

        // font
        ImGui.SetWindowFontScale(this.plugin.Configuration.OverlayFontScale);

        // Custom Overlay
        overlays[this.plugin.Configuration.OverlayType].Draw();
    }

    public void Dispose()
    {
        overlays[this.plugin.Configuration.OverlayType].Dispose();
    }

    // helpers

    private void Background(Vector4 color)
    {
        Vector2 vMin = ImGui.GetWindowContentRegionMin();
        Vector2 vMax = ImGui.GetWindowContentRegionMax();

        vMin.X += ImGui.GetWindowPos().X;
        vMin.Y += ImGui.GetWindowPos().Y;
        vMax.X += ImGui.GetWindowPos().X;
        vMax.Y += ImGui.GetWindowPos().Y;

        ImGui.GetWindowDrawList().AddRectFilled(vMin, vMax, Color(color));
    }

    public ImGuiWindowFlags Gerateflags()
    {
        var flags = DefaultFlags;
        flags |= this.plugin.Configuration.OverlayClickThrough ? ImGuiWindowFlags.NoInputs : ImGuiWindowFlags.None;
        //flags |= !this.plugin.Configuration.OverlayBackground ? ImGuiWindowFlags.NoBackground : ImGuiWindowFlags.None;
        return flags;
    }

    public void updatefont()
    {
        var conf = this.plugin.Configuration;
        //this.plugin.PluginLog.Info("trying to load path: " + conf.OverlayFontPath);

        //this.plugin.OverlayWindow.font = ImGui.GetIO().Fonts.AddFontFromFileTTF(conf.OverlayFontPath, conf.OverlayFontSize);

        //this.plugin.PluginLog.Info("font loaded ?");
    }

    public static uint Color(byte r, byte g, byte b, byte a)
    {
        uint ret = a;
        ret <<= 8;
        ret += b;
        ret <<= 8;
        ret += g;
        ret <<= 8;
        ret += r;
        return ret;
    }

    public static uint Color(Vector4 color)
    {
        return Color(Convert.ToByte(Math.Min(Math.Round(color.X * 255), 255)), Convert.ToByte(Math.Min(Math.Round(color.Y * 255), 255)),
            Convert.ToByte(Math.Min(Math.Round(color.Z * 255), 255)), Convert.ToByte(Math.Round(color.W * 255)));
    }
}
