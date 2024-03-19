using ImGuiNET;
using MeterWay.Overlay;
using System;
using System.Numerics;

namespace Lazer;

[Serializable]
public class Configuration : IConfiguration
{
    public int Version { get; set; } = 0;

    public bool ClickThrough { get; set; } = false;

    public uint BackgroundColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.25f));

    public uint color1 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(26f / 255f, 26f / 255f, 26f / 255f, 192f / 255f));
    public uint color2 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(128f / 255f, 170f / 255f, 128f / 255f, 255f / 255f));
    public uint color3 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0.5f, 0.5f, 170f / 255f, 1f));
    public uint color4 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(26f / 255f, 26f / 255f, 26f / 255f, 222f / 255f));
    public uint color5 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(144f / 255f, 144f / 255f, 144f / 255f, 1f));
    public uint color6 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(210f / 255f, 210f / 255f, 210f / 255f, 1f));
    public uint color7 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(30f / 255f, 30f / 255f, 30f / 255f, 190f / 255f));
    public uint color8 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(80f / 255f, 80f / 255f, 80f / 255f, 194f / 255f));
}