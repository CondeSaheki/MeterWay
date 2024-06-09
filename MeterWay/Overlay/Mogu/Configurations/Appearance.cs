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
    public JobColors JobColors { get; set; } = new();
}

public partial class Overlay : BasicOverlay
{
    private void DrawAppearanceTab()
    {
        using var tab = ImRaii.TabItem("Appearance");
        if (!tab) return;

        using var bar = ImRaii.TabBar("Overlay Appearance Tabs");
        if (!bar) return;

        DrawAppearanceGeralTab();
        DrawRoleJobColorsTab();
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
            Save(Window.WindowName, Config);
        });

        if (!Config.Appearance.Background) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        _ImGuiColorPick("Background Color", Config.Appearance.BackgroundColor, newValue =>
        {
            Config.Appearance.BackgroundColor = newValue;
            Save(Window.WindowName, Config);
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
            Save(Window.WindowName, Config);
        });

        if (!Config.Appearance.Header) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();

        var HeaderAlignmentValue = (int)Config.Appearance.HeaderAlignment;
        if (DrawAlingmentButtons(ref HeaderAlignmentValue))
        {
            Config.Appearance.HeaderAlignment = (Canvas.HorizontalAlign)HeaderAlignmentValue;
            Save(Window.WindowName, Config);
        }

        _ImguiCheckboxWithTooltip("Header Background", null, Config.Appearance.HeaderBackground, newValue =>
        {
            Config.Appearance.HeaderBackground = newValue;
            Save(Window.WindowName, Config);
        });

        if (!Config.Appearance.HeaderBackground) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        _ImGuiColorPick("Header Background Color", Config.Appearance.HeaderBackgroundColor, newValue =>
        {
            Config.Appearance.HeaderBackgroundColor = newValue;
            Save(Window.WindowName, Config);
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
            Save(Window.WindowName, Config);
        });

        _ImguiCheckboxWithTooltip("Bar", null, Config.Appearance.Bar, newValue =>
        {
            Config.Appearance.Bar = newValue;
            Save(Window.WindowName, Config);
        });

        if (!Config.Appearance.Bar) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        _ImguiCheckboxWithTooltip("job colors", null, Config.Appearance.BarColorJob, newValue =>
        {
            Config.Appearance.BarColorJob = newValue;
            Save(Window.WindowName, Config);
        });

        if (Config.Appearance.BarColorJob) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        _ImGuiColorPick("Color", Config.Appearance.BarColor, newValue =>
        {
            Config.Appearance.BarColor = newValue;
            Save(Window.WindowName, Config);
        });
        ImGui.Unindent();
        ImGui.Unindent();
        _ImGuiEndDisabled(ref isDisabled);
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
            Save(Window.WindowName, Config);
        });
        ImGui.SameLine();
        if (ImGui.Button("Default"))
        {
            FontMogu?.Dispose();
            FontMogu = DefaultFont;

            Config.Font.MoguFontSpec = null;
            Save(Window.WindowName, Config);
        }
        ImGui.SameLine();
        _ImGuiColorPick("##MoguFontColor", Config.Font.MoguFontColor, (newValue) =>
        {
            Config.Font.MoguFontColor = newValue;
            Save(Window.WindowName, Config);
        });
        ImGui.SameLine();
        ImGui.Text("-->");
        ImGui.SameLine();
        ImGui.Text($"{Config.Font.MoguFontSpec?.ToLocalizedString("en") ?? "Default"}");

        ImGui.EndDisabled();
    }
}
