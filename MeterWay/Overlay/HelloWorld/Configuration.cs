using MeterWay.Overlay;
using System;

namespace HelloWorld;

[Serializable]
public class Configuration : IConfiguration
{
    // keep track of the overlay version
    public int Version { get; set; } = 0;
    
    // Example configuration
    public bool IsEnabled { get; set; } = false;
}