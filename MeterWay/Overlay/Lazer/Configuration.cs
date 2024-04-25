using Dalamud.Interface.FontIdentifier;
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
    
    public float Spacing { get; set; } = 2f;

    public uint LazerFontColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
    public SingleFontSpec? LazerFontSpec { get; set; } = null;

    public uint Color1 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(26f / 255f, 26f / 255f, 26f / 255f, 192f / 255f));
    public uint Color2 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(128f / 255f, 170f / 255f, 128f / 255f, 255f / 255f));
    public uint Color3 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0.5f, 0.5f, 170f / 255f, 1f));
    public uint Color4 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(26f / 255f, 26f / 255f, 26f / 255f, 222f / 255f));
    public uint Color5 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(144f / 255f, 144f / 255f, 144f / 255f, 1f));
    public uint Color6 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(210f / 255f, 210f / 255f, 210f / 255f, 1f));
    public uint Color7 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(30f / 255f, 30f / 255f, 30f / 255f, 190f / 255f));
    public uint Color8 { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(80f / 255f, 80f / 255f, 80f / 255f, 194f / 255f));
}