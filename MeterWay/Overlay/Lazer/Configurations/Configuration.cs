using System;
using Dalamud.Interface.Utility.Raii;

using MeterWay.Overlay;

namespace Lazer;

[Serializable]
public class Configuration : IConfiguration
{
    public int Version { get; set; } = 0;

    public General General { get; set; } = new();
    public Font Font { get; set; } = new();
    public Visibility Visibility { get; set; } = new();
    public Appearance Appearance { get; set; } = new();
}

public partial class Overlay : IOverlay, IOverlayConfig
{
    public void DrawConfig()
    {
        using var bar = ImRaii.TabBar("Overlay Settings Tabs");
        if (!bar) return;

        DrawGeneralTab();
        DrawVisibilityTab();
        DrawAppearanceTab();
        DrawAboutTab();
    }
}