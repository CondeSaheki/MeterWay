using Dalamud.Configuration;
using System;
using System.Collections.Generic;

using MeterWay.Connection;
using MeterWay.Overlay;

namespace MeterWay;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    // Connection
    public ClientType ClientType { get; set; } = ClientType.Iinact;
    public string Address { get; set; } = "ws://127.0.0.1:10501/ws";

    // Overlay
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