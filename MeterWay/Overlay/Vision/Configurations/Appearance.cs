using System;
using System.Numerics;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Overlay;
using MeterWay.Utils;

namespace Vision;

[Serializable]
public class Font()
{
    public uint VisionFontColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
    public SingleFontSpec? VisionFontSpec { get; set; } = null;
}

[Serializable]
public class Appearance
{
    public StringFormater Format { get; set; } = new("Crt {Player.DamageDealt.Count.Percent.Critical}% | Dh {Player.DamageDealt.Count.Percent.Direct}% | CrtDh {Player.DamageDealt.Count.Percent.CriticalDirect}% | {Player.DamageDealt.Value.Total}");
    public bool BackgroundEnable { get; set; } = false;
    public uint BackgroundColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));


    public string[] SingleFormat { get; set; } = [];
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
        DrawFontsTab();
    }

    private void DrawAppearanceGeralTab()
    {
        using var tab = ImRaii.TabItem("General");
        if (!tab) return;

        var isDisabled = false;

        ImGui.Spacing();
        ImGui.Text("Customize how the Vision looks");
        ImGui.Spacing();

        _ImguiCheckboxWithTooltip("Header Background", null, Config.Appearance.BackgroundEnable, newValue =>
        {
            Config.Appearance.BackgroundEnable = newValue;
            Save(Window.WindowName, Config);
        });

        if (!Config.Appearance.BackgroundEnable) _ImGuiBeginDisabled(ref isDisabled);
        ImGui.Indent();

        _ImGuiColorPick("Background Color", Config.Appearance.BackgroundColor, newValue =>
        {
            Config.Appearance.BackgroundColor = newValue;
            Save(Window.WindowName, Config);
        });

        ImGui.Unindent();
        _ImGuiEndDisabled(ref isDisabled);

        if (ImGui.Button("Format builder"))
        {
            var formatBuilder = new FormatBuilderDialog(Config.Appearance.Format, (newValue) =>
            {
                Config.Appearance.Format = newValue;
                Save(Window.WindowName, Config);
            });

        }

        var formatchoser = new FormatChoser("Header Format", Config.Appearance.SingleFormat, (newValue) =>
        {
            Config.Appearance.SingleFormat = newValue.FullName;
            Save(Window.WindowName, Config);
        });
        formatchoser.Draw();
        ImGui.SameLine();
        ImGui.Text($"> {(Config.Appearance.SingleFormat.Length == 0 ? "Empty" : string.Join(" > ", Config.Appearance.SingleFormat))}");

        var asdasd = Job.IsRanged(Job.Summoner);
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
        if (ImGui.Button("Change")) _SingleFontChooserDialog(null, Config.Font.VisionFontSpec, (result) =>
        {
            FontVision?.Dispose();
            FontVision = result.CreateFontHandle(FontAtlas);

            Config.Font.VisionFontSpec = result;
            Save(Window.WindowName, Config);
        });
        ImGui.SameLine();
        if (ImGui.Button("Default"))
        {
            FontVision?.Dispose();
            FontVision = DefaultFont;

            Config.Font.VisionFontSpec = null;
            Save(Window.WindowName, Config);
        }
        ImGui.SameLine();
        _ImGuiColorPick("##VisionFontColor", Config.Font.VisionFontColor, (value) =>
        {
            Config.Font.VisionFontColor = value;
            Save(Window.WindowName, Config);
        });
        ImGui.SameLine();
        ImGui.Text("-->");
        ImGui.SameLine();
        ImGui.Text($"{Config.Font.VisionFontSpec?.ToLocalizedString("en") ?? "Default"}");

        ImGui.EndDisabled();
    }
}
