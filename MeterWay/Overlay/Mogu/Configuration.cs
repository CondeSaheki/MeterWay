
using MeterWay.Overlay;
using System;
using System.Numerics;

namespace Mogu;

[Serializable]
public class Configuration : MeterWayOverlayConfiguration
{
    public bool FrameCalc { get; set; } = false;
    public bool ClickThrough { get; set; } = false;

    public bool Background { get; set; } = false;
    public Vector4 BackgroundColor { get; set; } = new Vector4(0f, 0f, 0f, 0.25f);
}