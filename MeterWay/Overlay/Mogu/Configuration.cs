
using MeterWay.Overlay;
using System;
using System.Numerics;

namespace Mogu;

[Serializable]
public class Configuration : IConfiguration
{
    public int Version { get; set; } = 0;

    public bool FrameCalc { get; set; } = false;
    public bool ClickThrough { get; set; } = false;

    public bool Background { get; set; } = false;
    public Vector4 BackgroundColor { get; set; } = new Vector4(0f, 0f, 0f, 0.25f);
}