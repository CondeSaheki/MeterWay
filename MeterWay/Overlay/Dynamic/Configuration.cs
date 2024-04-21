using MeterWay.Overlay;
using System;

namespace Dynamic;

[Serializable]
public class Configuration : IConfiguration
{
    public int Version { get; set; } = 0;
    
    public string? ScriptFile { get; set; } = null;
    public bool Startup { get; set; } = true;
}