using System;
using System.Numerics;
using ImGuiNET;

using MeterWay.Utils;
using MeterWay.Overlay;

namespace Mogu;

[Serializable]
public class Configuration : IConfiguration
{
    public int Version { get; set; } = 0;

    // general
    public bool FrameCalc { get; set; } = false;
    public bool ClickThrough { get; set; } = false;

    // Appearance general
    public bool Background { get; set; } = false;
    public uint BackgroundColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.25f));

    // Appearance Header
    public bool Header { get; set; } = true;
    public bool HeaderBackground { get; set; } = true;
    public uint HeaderBackgroundColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.25f));
    public Canvas.HorizontalAlign HeaderAlignment { get; set; } = Canvas.HorizontalAlign.Center;

    // Appearance Players
    public bool PlayerJobIcon { get; set; } = true;
    public bool PlayerNameColorJob { get; set; } = true;
    public uint PlayerNameColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));

    public bool Bar { get; set; } = true;
    public bool BarColorJob { get; set; } = true;
    public uint BarColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.25f));

    // fonts
    public uint MoguFontColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));

    // Job colors
    public (JobIds Job, uint Color)[] JobColors { get; set; } =
    {
        // tanks
        (JobIds.PLD, ImGui.ColorConvertFloat4ToU32(new(0.6588f, 0.8235f, 0.902f, 0.25f ))),
        (JobIds.DRK, ImGui.ColorConvertFloat4ToU32(new(0.8196f, 0.149f, 0.8f, 0.25f ))),
        (JobIds.WAR, ImGui.ColorConvertFloat4ToU32(new(0.8118f, 0.149f, 0.1294f, 0.25f ))),
        (JobIds.GNB, ImGui.ColorConvertFloat4ToU32(new(0.4745f, 0.4275f, 0.1882f, 0.25f ))),
        (JobIds.GLA, ImGui.ColorConvertFloat4ToU32(new(0.6588f, 0.8235f, 0.902f, 0.25f ))),
        (JobIds.MRD, ImGui.ColorConvertFloat4ToU32(new(0.8118f, 0.149f, 0.1294f, 0.25f ))),
        // healers
        (JobIds.SCH, ImGui.ColorConvertFloat4ToU32(new(0.5255f, 0.3412f, 1f, 0.25f ))),
        (JobIds.WHM, ImGui.ColorConvertFloat4ToU32(new(1f, 0.9412f, 0.8627f, 0.25f ))),
        (JobIds.AST, ImGui.ColorConvertFloat4ToU32(new(1f, 0.9059f, 0.2902f, 0.25f ))),
        (JobIds.SGE, ImGui.ColorConvertFloat4ToU32(new(0.5647f, 0.6902f, 1f, 0.25f ))),
        (JobIds.CNJ, ImGui.ColorConvertFloat4ToU32(new(1f, 0.9412f, 0.8627f, 0.25f ))),
        // melees
        (JobIds.MNK, ImGui.ColorConvertFloat4ToU32(new(0.8392f, 0.6118f, 0f, 0.25f ))),
        (JobIds.NIN, ImGui.ColorConvertFloat4ToU32(new(0.6863f, 0.098f, 0.3922f, 0.25f ))),
        (JobIds.DRG, ImGui.ColorConvertFloat4ToU32(new(0.2549f, 0.3922f, 0.8039f, 0.25f ))),
        (JobIds.SAM, ImGui.ColorConvertFloat4ToU32(new(0.8941f, 0.4275f, 0.0157f, 0.25f ))),
        (JobIds.RPR, ImGui.ColorConvertFloat4ToU32(new(0.5882f, 0.3529f, 0.5647f, 0.25f ))),
        (JobIds.PGL, ImGui.ColorConvertFloat4ToU32(new(0.8392f, 0.6118f, 0f, 0.25f ))),
        (JobIds.ROG, ImGui.ColorConvertFloat4ToU32(new(0.6863f, 0.098f, 0.3922f, 0.25f ))),
        (JobIds.LNC, ImGui.ColorConvertFloat4ToU32(new(0.2549f, 0.3922f, 0.8039f, 0.25f ))),
        // physical rangeds
        (JobIds.BRD, ImGui.ColorConvertFloat4ToU32(new(0.5686f, 0.7294f, 0.3686f, 0.25f ))),
        (JobIds.MCH, ImGui.ColorConvertFloat4ToU32(new(0.4314f, 0.8824f, 0.8392f, 0.25f ))),
        (JobIds.DNC, ImGui.ColorConvertFloat4ToU32(new(0.8863f, 0.6902f, 0.6863f, 0.25f ))),
        (JobIds.ARC, ImGui.ColorConvertFloat4ToU32(new(0.5686f, 0.7294f, 0.3686f, 0.25f ))),
        // casters
        (JobIds.BLM, ImGui.ColorConvertFloat4ToU32(new(0.6471f, 0.4745f, 0.8392f, 0.25f ))),
        (JobIds.SMN, ImGui.ColorConvertFloat4ToU32(new(0.1765f, 0.6078f, 0.4706f, 0.25f ))),
        (JobIds.RDM, ImGui.ColorConvertFloat4ToU32(new(0.9098f, 0.4824f, 0.4824f, 0.25f ))),
        (JobIds.BLU, ImGui.ColorConvertFloat4ToU32(new(0f, 0.7255f, 0.9686f, 0.25f ))),
        (JobIds.THM, ImGui.ColorConvertFloat4ToU32(new(0.6471f, 0.4745f, 0.8392f, 0.25f ))),
        (JobIds.ACN, ImGui.ColorConvertFloat4ToU32(new(0.1765f, 0.6078f, 0.4706f, 0.25f ))),
    };
    public uint JobDefaultColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(218f / 255f, 157f / 255f, 46f / 255f, 0.25f));
}