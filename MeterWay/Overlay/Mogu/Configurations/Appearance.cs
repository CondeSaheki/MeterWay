using System;
using System.Linq;
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
    public IFontSpec? MoguFontSpec { get; set; } = null;
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
    public bool PlayerJobName { get; set; } = true;
    public Overlay.JobDisplay PlayerJobNameDisplay { get; set; } = Overlay.JobDisplay.Acronym;

    public Overlay.NameDisplay PlayerNameDisplay { get; set; } = Overlay.NameDisplay.FullName;
    public Overlay.ColorSelected PlayerNameColor { get; set; } = Overlay.ColorSelected.Default;
    public uint PlayerNameCustomColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.25f));

    public bool Bar { get; set; } = true;
    public Overlay.ColorSelected BarColor { get; set; } = Overlay.ColorSelected.Job;
    public uint BarCustomColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.25f));
    // Job colors
    public JobColors JobColors { get; set; } = new();
}

[Serializable]
public class Header
{
    public string Format { get; set; } = string.Empty;

    public bool Background { get; set; } = true;
    public uint BackgroundColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.25f));
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

        // _ImguiCheckboxWithTooltip("Job Name", null, Config.Appearance.PlayerJobName, newValue =>
        // {
        //     Config.Appearance.PlayerJobName = newValue;
        //     Save(Window.WindowName, Config);
        // });
        
        // if (!Config.Appearance.PlayerJobName) _ImGuiBeginDisabled(ref isDisabled);
        // ImGui.Indent();

        // string[] JobDisplays =
        // [   
        //     "Acroyn",
        //     "Full Name",
        // ];
        // var PlayerJobNameDisplayValue = (int)Config.Appearance.PlayerJobNameDisplay;
        // if (ImGui.Combo("Job Display", ref PlayerJobNameDisplayValue, JobDisplays, JobDisplays.Length) && Config.Appearance.PlayerJobNameDisplay != (JobDisplay)PlayerJobNameDisplayValue)
        // {
        //     Config.Appearance.PlayerJobNameDisplay = (JobDisplay)PlayerJobNameDisplayValue;
        //     Save(Window.WindowName, Config);
        // }

        // ImGui.Unindent();
        // _ImGuiEndDisabled(ref isDisabled);

        string[] NameDisplays =
        [
            "No Name",
            "Full Name",
            "Surname",
            "Surname Abbreviated",
            "Forename Abbreviated And Surname",
            "Forename",
            "Forename Abbreviated",
            "Forename And SurnameAbbreviated",
            "Initials"

        ];
        var NameDisplayValue = (int)Config.Appearance.PlayerNameDisplay;
        if (ImGui.Combo("Name Display", ref NameDisplayValue, NameDisplays, NameDisplays.Length) && Config.Appearance.PlayerNameDisplay != (NameDisplay)NameDisplayValue)
        {
            Config.Appearance.PlayerNameDisplay = (NameDisplay)NameDisplayValue;
            Save(Window.WindowName, Config);
        }

        DrawColorSelected("Name Color", Config.Appearance.PlayerNameColor, (newValue) =>
        {
            Config.Appearance.PlayerNameColor = newValue;
            Save(Window.WindowName, Config);
        });

        if (Config.Appearance.PlayerNameColor != ColorSelected.Custom) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        _ImGuiColorPick("Name Custom Color", Config.Appearance.PlayerNameCustomColor, newValue =>
        {
            Config.Appearance.PlayerNameCustomColor = newValue;
            Save(Window.WindowName, Config);
        });
        ImGui.Unindent();
        _ImGuiEndDisabled(ref isDisabled);


        _ImguiCheckboxWithTooltip("Bar", null, Config.Appearance.Bar, newValue =>
        {
            Config.Appearance.Bar = newValue;
            Save(Window.WindowName, Config);
        });

        if (!Config.Appearance.Bar) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        DrawColorSelected("Bar Color", Config.Appearance.BarColor, (newValue) =>
        {
            Config.Appearance.BarColor = newValue;
            Save(Window.WindowName, Config);
        });
        
        if (Config.Appearance.BarColor != ColorSelected.Custom) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();
        _ImGuiColorPick("Bar Custom Color", Config.Appearance.BarCustomColor, newValue =>
        {
            Config.Appearance.BarCustomColor = newValue;
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
    }
}
