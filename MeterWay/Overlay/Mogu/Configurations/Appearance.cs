using System;
using System.Numerics;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Overlay;
using MeterWay.Utils;

namespace Mogu;

[Serializable]
public class Font()
{
    public uint MoguFontColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
    public SingleFontSpec? MoguFontSpec { get; set; } = null;
}

[Serializable]
public class Appearance
{
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

    // Job colors
    public (JobIds Job, uint Color)[] JobColors { get; set; } =
    [
        // tanks
        (JobIds.PLD, ImGui.ColorConvertFloat4ToU32(new(0.6588f, 0.8235f, 0.902f, 0.25f))),
        (JobIds.DRK, ImGui.ColorConvertFloat4ToU32(new(0.8196f, 0.149f, 0.8f, 0.25f))),
        (JobIds.WAR, ImGui.ColorConvertFloat4ToU32(new(0.8118f, 0.149f, 0.1294f, 0.25f))),
        (JobIds.GNB, ImGui.ColorConvertFloat4ToU32(new(0.4745f, 0.4275f, 0.1882f, 0.25f))),
        (JobIds.GLA, ImGui.ColorConvertFloat4ToU32(new(0.6588f, 0.8235f, 0.902f, 0.25f))),
        (JobIds.MRD, ImGui.ColorConvertFloat4ToU32(new(0.8118f, 0.149f, 0.1294f, 0.25f))),
        // healers
        (JobIds.SCH, ImGui.ColorConvertFloat4ToU32(new(0.5255f, 0.3412f, 1f, 0.25f))),
        (JobIds.WHM, ImGui.ColorConvertFloat4ToU32(new(1f, 0.9412f, 0.8627f, 0.25f))),
        (JobIds.AST, ImGui.ColorConvertFloat4ToU32(new(1f, 0.9059f, 0.2902f, 0.25f))),
        (JobIds.SGE, ImGui.ColorConvertFloat4ToU32(new(0.5647f, 0.6902f, 1f, 0.25f))),
        (JobIds.CNJ, ImGui.ColorConvertFloat4ToU32(new(1f, 0.9412f, 0.8627f, 0.25f))),
        // melees
        (JobIds.MNK, ImGui.ColorConvertFloat4ToU32(new(0.8392f, 0.6118f, 0f, 0.25f))),
        (JobIds.NIN, ImGui.ColorConvertFloat4ToU32(new(0.6863f, 0.098f, 0.3922f, 0.25f))),
        (JobIds.DRG, ImGui.ColorConvertFloat4ToU32(new(0.2549f, 0.3922f, 0.8039f, 0.25f))),
        (JobIds.SAM, ImGui.ColorConvertFloat4ToU32(new(0.8941f, 0.4275f, 0.0157f, 0.25f))),
        (JobIds.RPR, ImGui.ColorConvertFloat4ToU32(new(0.5882f, 0.3529f, 0.5647f, 0.25f))),
        (JobIds.PGL, ImGui.ColorConvertFloat4ToU32(new(0.8392f, 0.6118f, 0f, 0.25f))),
        (JobIds.ROG, ImGui.ColorConvertFloat4ToU32(new(0.6863f, 0.098f, 0.3922f, 0.25f))),
        (JobIds.LNC, ImGui.ColorConvertFloat4ToU32(new(0.2549f, 0.3922f, 0.8039f, 0.25f))),
        // physical rangeds
        (JobIds.BRD, ImGui.ColorConvertFloat4ToU32(new(0.5686f, 0.7294f, 0.3686f, 0.25f))),
        (JobIds.MCH, ImGui.ColorConvertFloat4ToU32(new(0.4314f, 0.8824f, 0.8392f, 0.25f))),
        (JobIds.DNC, ImGui.ColorConvertFloat4ToU32(new(0.8863f, 0.6902f, 0.6863f, 0.25f))),
        (JobIds.ARC, ImGui.ColorConvertFloat4ToU32(new(0.5686f, 0.7294f, 0.3686f, 0.25f))),
        // casters
        (JobIds.BLM, ImGui.ColorConvertFloat4ToU32(new(0.6471f, 0.4745f, 0.8392f, 0.25f))),
        (JobIds.SMN, ImGui.ColorConvertFloat4ToU32(new(0.1765f, 0.6078f, 0.4706f, 0.25f))),
        (JobIds.RDM, ImGui.ColorConvertFloat4ToU32(new(0.9098f, 0.4824f, 0.4824f, 0.25f))),
        (JobIds.BLU, ImGui.ColorConvertFloat4ToU32(new(0f, 0.7255f, 0.9686f, 0.25f))),
        (JobIds.THM, ImGui.ColorConvertFloat4ToU32(new(0.6471f, 0.4745f, 0.8392f, 0.25f))),
        (JobIds.ACN, ImGui.ColorConvertFloat4ToU32(new(0.1765f, 0.6078f, 0.4706f, 0.25f))),
    ];
    public uint JobDefaultColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0.8549f, 0.6157f, 0.1804f, 0.25f));
}

public partial class Overlay : IOverlay, IOverlayConfig
{
    private void DrawAppearanceTab()
    {
        using var tab = ImRaii.TabItem("Appearance");
        if (!tab) return;

        using var bar = ImRaii.TabBar("Overlay Appearance Tabs");
        if (!bar) return;

        DrawAppearanceGeralTab();
        DrawJobColorsTab();
        DrawFontsTab();
    }

    private void DrawAppearanceGeralTab()
    {
        using var tab = ImRaii.TabItem("General");
        if (!tab) return;

        var isDisabled = false;

        ImGui.Spacing();
        ImGui.Text("Customize how the Mogu looks");
        ImGui.Spacing();

        // Section: General
        ImGui.Separator();
        ImGui.Text("General");
        ImGui.Spacing();

        _ImguiCheckboxWithTooltip("Background", null, Config.Appearance.Background, newValue =>
        {
            Config.Appearance.Background = newValue;
            File.Save(Window.NameId, Config);
        });

        if (!Config.Appearance.Background) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        _ImGuiColorPick("Background Color", Config.Appearance.BackgroundColor, newValue =>
        {
            Config.Appearance.BackgroundColor = newValue;
            File.Save(Window.NameId, Config);
        });
        ImGui.Unindent();
        _ImGuiEndDisabled(ref isDisabled);

        ImGui.Spacing();
        // Section: Header
        ImGui.Separator();
        ImGui.Text("Header");
        ImGui.Spacing();

        _ImguiCheckboxWithTooltip("Header##button", null, Config.Appearance.Header, newValue =>
        {
            Config.Appearance.Header = newValue;
            File.Save(Window.NameId, Config);
        });

        if (!Config.Appearance.Header) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();

        var HeaderAlignmentValue = (int)Config.Appearance.HeaderAlignment;
        if (DrawAlingmentButtons(ref HeaderAlignmentValue))
        {
            Config.Appearance.HeaderAlignment = (Canvas.HorizontalAlign)HeaderAlignmentValue;
            File.Save(Window.NameId, Config);
        }

        _ImguiCheckboxWithTooltip("Header Background", null, Config.Appearance.HeaderBackground, newValue =>
        {
            Config.Appearance.HeaderBackground = newValue;
            File.Save(Window.NameId, Config);
        });

        if (!Config.Appearance.HeaderBackground) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        _ImGuiColorPick("Header Background Color", Config.Appearance.HeaderBackgroundColor, newValue =>
        {
            Config.Appearance.HeaderBackgroundColor = newValue;
            File.Save(Window.NameId, Config);
        });
        ImGui.Unindent();
        ImGui.Unindent();
        _ImGuiEndDisabled(ref isDisabled);

        ImGui.Spacing();
        // Section: Player
        ImGui.Separator();
        ImGui.Text("Player");
        ImGui.Spacing();

        _ImguiCheckboxWithTooltip("Job Icon", null, Config.Appearance.PlayerJobIcon, newValue =>
        {
            Config.Appearance.PlayerJobIcon = newValue;
            File.Save(Window.NameId, Config);
        });

        _ImguiCheckboxWithTooltip("Bar", null, Config.Appearance.Bar, newValue =>
        {
            Config.Appearance.Bar = newValue;
            File.Save(Window.NameId, Config);
        });

        if (!Config.Appearance.Bar) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        _ImguiCheckboxWithTooltip("job colors", null, Config.Appearance.BarColorJob, newValue =>
        {
            Config.Appearance.BarColorJob = newValue;
            File.Save(Window.NameId, Config);
        });

        if (Config.Appearance.BarColorJob) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        _ImGuiColorPick("Color", Config.Appearance.BarColor, newValue =>
        {
            Config.Appearance.BarColor = newValue;
            File.Save(Window.NameId, Config);
        });
        ImGui.Unindent();
        ImGui.Unindent();
        _ImGuiEndDisabled(ref isDisabled);
    }

    private void DrawJobColorsTab()
    {
        using var tab = ImRaii.TabItem("Job Colors");
        if (!tab) return;

        ImGui.Spacing();
        ImGui.Text("Job Colors");
        ImGui.Spacing();

        ImGui.BeginTable("##ColorTable", 4, ImGuiTableFlags.SizingFixedFit); // ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.RowBg
        for (var i = 0; i != Config.Appearance.JobColors.Length; ++i)
        {
            ImGui.TableNextColumn();
            var ColorValue = ImGui.ColorConvertU32ToFloat4(Config.Appearance.JobColors[i].Color);
            if (ImGui.ColorEdit4(Config.Appearance.JobColors[i].Job.ToString(), ref ColorValue,
                ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
            {
                Config.Appearance.JobColors[i].Color = ImGui.ColorConvertFloat4ToU32(ColorValue);
                File.Save(Window.NameId, Config);
            }
        }
        ImGui.EndTable();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        var DefaultColorValue = ImGui.ColorConvertU32ToFloat4(Config.Appearance.JobDefaultColor);
        if (ImGui.ColorEdit4("Job Default Color", ref DefaultColorValue,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
        {
            Config.Appearance.JobDefaultColor = ImGui.ColorConvertFloat4ToU32(DefaultColorValue);
            File.Save(Window.NameId, Config);
        }
    }

    private void DrawFontsTab()
    {
        using var tab = ImRaii.TabItem("Fonts");
        if (!tab) return;

        ImGui.TextColored(new Vector4(1, 0, 0, 1), "Temporarily disabled, sorry.");
        ImGui.BeginDisabled();

        ImGui.Spacing();
        ImGui.Text("Change the fonts used in the overlay");
        ImGui.Spacing();

        ImGui.Text("Geral Font");
        if (ImGui.Button("Change")) _SingleFontChooserDialog(null, Config.Font.MoguFontSpec, (result) =>
        {
            FontMogu?.Dispose();
            FontMogu = result.CreateFontHandle(FontAtlas);

            Config.Font.MoguFontSpec = result;
            File.Save(Window.NameId, Config);
        });
        ImGui.SameLine();
        if (ImGui.Button("Default"))
        {
            FontMogu?.Dispose();
            FontMogu = DefaultFont;

            Config.Font.MoguFontSpec = null;
            File.Save(Window.NameId, Config);
        }
        ImGui.SameLine();
        _ImGuiColorPick("##MoguFontColor", Config.Font.MoguFontColor, (value) =>
        {
            Config.Font.MoguFontColor = value;
            File.Save(Window.NameId, Config);
        });
        ImGui.SameLine();
        ImGui.Text("-->");
        ImGui.SameLine();
        ImGui.Text($"{Config.Font.MoguFontSpec?.ToLocalizedString("en") ?? "Default"}");

        ImGui.EndDisabled();
    }
}
