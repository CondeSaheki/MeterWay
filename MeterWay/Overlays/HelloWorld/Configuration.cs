using MeterWay.Overlay;
using System;

namespace HelloWorld;

[Serializable]
public class Configuration : MeterWayOverlayConfiguration
{
    public bool Enabled { get; set; } = false;
}