using System;
using System.Numerics;
using ImGuiNET;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.Utility.Raii;

using MeterWay.Overlay;

namespace Lazer;

[Serializable]
public class Font()
{
    public uint LazerFontColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
    public SingleFontSpec? LazerFontSpec { get; set; } = null;
}

public partial class Overlay : BasicOverlay
{
    private void DrawFontsTab()
    {
        using var tab = ImRaii.TabItem("Fonts");
        if (!tab) return;

        ImGui.TextColored(new Vector4(1, 0, 0, 1), "Temporarily disabled, sorry.");
        //ImGui.BeginDisabled();

        ImGui.Spacing();
        ImGui.Text("Change the fonts used in the overlay");
        ImGui.Spacing();

        ImGui.Text("Geral Font");
        if (ImGui.Button("Change")) _SingleFontChooserDialog(null, Config.Font.LazerFontSpec, (result) =>
        {
            FontLazer?.Dispose();
            FontLazer = result.CreateFontHandle(FontAtlas);

            Config.Font.LazerFontSpec = result;
            Save(Window.WindowName, Config);
        });
        ImGui.SameLine();
        if (ImGui.Button("Default"))
        {
            FontLazer?.Dispose();
            FontLazer = DefaultFont;

            Config.Font.LazerFontSpec = null;
            Save(Window.WindowName, Config);
        }
        ImGui.SameLine();
        _ImGuiColorPick("##MoguFontColor", Config.Font.LazerFontColor, (value) =>
        {
            Config.Font.LazerFontColor = value;
            Save(Window.WindowName, Config);
        });
        ImGui.SameLine();
        ImGui.Text("-->");
        ImGui.SameLine();
        ImGui.Text($"{Config.Font.LazerFontSpec?.ToLocalizedString("en") ?? "Default"}");

        //ImGui.EndDisabled();
    }
}