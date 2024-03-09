using MeterWay.Overlay;
using System;

namespace HelloWorld;

[Serializable]
public class Configuration : IConfiguration
{
    public int Version { get; set; } = 0;

    public bool Enabled { get; set; } = false;
}