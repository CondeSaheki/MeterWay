using MeterWay.Overlay;
using System;
using System.Collections.Generic;

namespace Dynamic;

[Serializable]
public class Configuration : IConfiguration
{
    public int Version { get; set; } = 0;
    
    public Dictionary<string, string> Scripts { get; set; } = [];
    public string? ScriptName { get; set; } = null;
    
    public bool LoadInit { get; set; } = false;
}