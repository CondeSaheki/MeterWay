using Dalamud.Configuration;
using System;
using System.Numerics;

namespace MeterWay;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    //overlay
    public bool OverlayEnabled { get; set; } = true;
    public string OverlayName { get; set; } = string.Empty;

    public bool OverlayClickThrough { get; set; } = false;

    public bool OverlayBackground { get; set; } = true;
    public Vector4 OverlayBackgroundColor { get; set; } = new Vector4(0f, 0f, 0f, 0.25f);

    public string OverlayFontPath { get; set; } = "Inter-Bold.ttf";
    public float OverlayFontSize { get; set; } = 15f;
    public float OverlayFontScale { get; set; } = 1f;

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