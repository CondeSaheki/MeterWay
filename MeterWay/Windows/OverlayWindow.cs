using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;

using MeterWay.Overlays;
using MeterWay.managers;

namespace MeterWay.Windows;

public class OverlayWindow : Window, IDisposable
{
    
    private ImGuiWindowFlags defaultflags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground;

    private List<IMeterwayOverlay> overlays;
    public ImFontPtr font;

    private readonly Encounter data;

    public OverlayWindow(Encounter data) : base("OverlayWindow")
    {

        // window configs
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(80, 45),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.IsOpen = true;
        this.RespectCloseHotkey = false;
        this.Flags = Gerateflags();
        this.data = data;
        this.font = ImGui.GetIO().Fonts.AddFontFromFileTTF(ConfigurationManager.Instance.Configuration.OverlayFontPath, ConfigurationManager.Instance.Configuration.OverlayFontSize);

        // precisamos de achar uma forma dele adicionar isso sem ser manualmente

        this.overlays = [new LazerOverlay(), new MoguOverlay()];
        

    }

    // draw

    public override void Draw()
    {
        // font
        ImGui.SetWindowFontScale(ConfigurationManager.Instance.Configuration.OverlayFontScale);

        // adicionar isso aqui ou no handler ?? 
        overlays[ConfigurationManager.Instance.Configuration.OverlayType].DataProcess(data);

        // Custom Overlay
        overlays[ConfigurationManager.Instance.Configuration.OverlayType].Draw();
    }

    public void Dispose()
    {
        overlays[ConfigurationManager.Instance.Configuration.OverlayType].Dispose();
    }

    public ImGuiWindowFlags Gerateflags()
    {
        var flags = defaultflags;
        flags |= ConfigurationManager.Instance.Configuration.OverlayClickThrough ? ImGuiWindowFlags.NoInputs : ImGuiWindowFlags.None;
        //flags |= !PluginManager.Instance.Configuration.OverlayBackground ? ImGuiWindowFlags.NoBackground : ImGuiWindowFlags.None;
        return flags;
    }

    public void updatefont()
    {
        var conf = ConfigurationManager.Instance.Configuration;
        //PluginManager.Instance.PluginLog.Info("trying to load path: " + conf.OverlayFontPath);

        //PluginManager.Instance.OverlayWindow.font = ImGui.GetIO().Fonts.AddFontFromFileTTF(conf.OverlayFontPath, conf.OverlayFontSize);

        //PluginManager.Instance.PluginLog.Info("font loaded ?");
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
