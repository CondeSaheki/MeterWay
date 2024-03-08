using MeterWay.Overlays;
using System;

namespace HelloWorld;

[Serializable]
public class Configuration : MeterWayOverlayConfiguration
{
    public bool Enabled { get; set; }

    public Configuration()
    {
        Enabled = false;
    }
}