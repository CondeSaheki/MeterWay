using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;

using MeterWay.Overlays;
using Meterway.Managers;
using MeterWay.Data;

namespace MeterWay.Windows;

public class OverlayWindow : Window, IDisposable
{

    private ImGuiWindowFlags defaultflags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground;

    public List<IMeterwayOverlay> Overlays;
    public ImFontPtr font;

    private readonly Func<Encounter> GetEncounter;

    public OverlayWindow(Func<Encounter> GetEncounter) : base("OverlayWindow")
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
        this.GetEncounter = GetEncounter;

        this.Overlays = new List<IMeterwayOverlay>();
    }

    // draw

    public override void Draw()
    {
        // font
        ImGui.SetWindowFontScale(ConfigurationManager.Instance.Configuration.OverlayFontScale);

        // adicionar isso aqui ou no handler ?? 
        Overlays[ConfigurationManager.Instance.Configuration.OverlayType].DataProcess(GetEncounter());

        // Custom Overlay
        Overlays[ConfigurationManager.Instance.Configuration.OverlayType].Draw();
    }

    public void Dispose()
    {
        Overlays[ConfigurationManager.Instance.Configuration.OverlayType].Dispose();
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

}
