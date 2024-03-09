using MeterWay.Overlay;
using System;
using System.Numerics;

namespace Lazer;

[Serializable]
public class Configuration : IConfiguration
{
    public int Version { get; set; } = 0;

    public bool ClickThrough { get; set; } = false;

    public Vector4 BackgroundColor { get; set; } = new Vector4(0f, 0f, 0f, 0.25f);

    // public string FontPath { get; set; } = "Inter-Bold.ttf";
    // public float FontSize { get; set; } = 15f;
    public float FontScale { get; set; } = 1f;
}