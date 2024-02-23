using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;

using MeterWay.Overlays;
using MeterWay.Managers;

namespace MeterWay.Windows;

public class OverlayWindow : Window, IDisposable
{
    private static readonly ImGuiWindowFlags defaultflags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground;

    public List<IMeterwayOverlay> Overlays;

    public OverlayWindow() : base("OverlayWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(80, 45),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        IsOpen = true;
        RespectCloseHotkey = false;
        Flags = GetFlags();

        Overlays = [];
    }

    public override void Draw()
    {
        ImGui.SetWindowFontScale(ConfigurationManager.Inst.Configuration.OverlayFontScale);

        Overlays[ConfigurationManager.Inst.Configuration.OverlayType].Draw();
    }

    public void Dispose()
    {
        foreach(var overlay in Overlays) overlay.Dispose();
    }

    public static ImGuiWindowFlags GetFlags()
    {
        return defaultflags | (ConfigurationManager.Inst.Configuration.OverlayClickThrough ? ImGuiWindowFlags.NoInputs : ImGuiWindowFlags.None);
    }
}
