using Dalamud.Configuration;
using System;

namespace MeterWay;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    //overlay
    public bool OverlayEnabled { get; set; } = false;
    public string OverlayName { get; set; } = string.Empty;
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