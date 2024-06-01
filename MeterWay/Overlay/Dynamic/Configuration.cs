using MeterWay.Overlay;
using System;

namespace Dynamic;

[Serializable]
public class Configuration : IBasicOverlayConfiguration
{
    public int Version { get; set; } = 0;
    
    public string? ScriptFile { get; set; } = null;
    public bool Startup { get; set; } = true;
}