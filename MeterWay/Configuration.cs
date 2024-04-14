using Dalamud.Configuration;
using MeterWay.Overlay;
using System;
using System.Collections.Generic;

namespace MeterWay;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    //overlay
    public List<OverlayWindowSpecs> Overlays { get; set; } = [];
    public bool OverlayRealtimeUpdate { get; set; } = false;
    public TimeSpan OverlayIntervalUpdate { get; set; } = TimeSpan.FromSeconds(1);

    public void Save() => Dalamud.PluginInterface.SavePluginConfig(this);

    public static Configuration Load()
    {
        if (Dalamud.PluginInterface.GetPluginConfig() is Configuration config) return config;
        config = new Configuration();
        config.Save();
        return config;
    }
}