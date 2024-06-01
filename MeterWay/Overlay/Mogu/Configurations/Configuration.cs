using System;
using Dalamud.Interface.Utility.Raii;

using MeterWay.Overlay;

namespace Mogu;

[Serializable]
public class Configuration : IBasicOverlayConfiguration
{
    public int Version { get; set; } = 0;

    public General General { get; set; } = new();
    public Visibility Visibility { get; set; } = new();
    public Font Font { get; set; } = new();
    public Appearance Appearance { get; set; } = new();
}

public partial class Overlay : BasicOverlay
{
    public override void DrawConfiguration()
    {
        using var bar = ImRaii.TabBar("Overlay Settings Tabs");
        if (!bar) return;

        DrawGeneralTab();
        DrawVisibilityTab();
        DrawAppearanceTab();
        DrawAboutTab();
    }
}