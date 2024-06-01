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
    public uint MoguFontColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
    public SingleFontSpec? MoguFontSpec { get; set; } = null;
}

[Serializable]
public class Appearance
{
    public string Format { get; set; } = "Crt {Player.DamageDealt.Count.Percent.Critical}% | Dh {Player.DamageDealt.Count.Percent.Direct}% | CrtDh {Player.DamageDealt.Count.Percent.CriticalDirect}% | {Player.DamageDealt}";
    public bool BackgroundEnable { get; set; } = false;
    public uint BackgroundColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));
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
        ImGui.Text("Customize how the Mogu looks");
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

        if (ImGui.Button("change format"))
        {
            PopupWindow format = new("Change Format Window", (TaskSource) =>
            {
                string formatValue = Config.Appearance.Format;
                int selectedItem = 0;

                ImGui.Text("Enter your text below:");

                ImGui.BeginChild("text box", new(ImGui.GetContentRegionAvail().X, 50), false, ImGuiWindowFlags.HorizontalScrollbar);
                var boxX = Math.Max(ImGui.CalcTextSize(formatValue).X, ImGui.GetContentRegionAvail().X);

                if (ImGui.InputTextMultiline("##BigTextInput", ref formatValue, 2048, new(boxX, 30), ImGuiInputTextFlags.NoHorizontalScroll))
                {
                    Config.Appearance.Format = formatValue;
                    Save(Window.WindowName, Config);
                }

                ImGui.EndChild();

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                if (ImGui.Button("Add"))
                {
                    formatValue += PlaceHolders[selectedItem];
                    Config.Appearance.Format = formatValue;
                    Save(Window.WindowName, Config);
                }
                ImGui.SameLine();
                if (ImGui.BeginCombo("##ComboBox", PlaceHolders[selectedItem]))
                {
                    for (int i = 0; i < PlaceHolders.Length; i++)
                    {
                        bool isSelected = selectedItem == i;
                        if (ImGui.Selectable(PlaceHolders[i], isSelected))
                        {
                            selectedItem = i;
                        }
                        if (isSelected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    ImGui.EndCombo();
                }





















            }, false, new(640, 360));
            // {
            //     WindowFlags = ImGuiWindowFlags.no
            // }

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
        _ImGuiColorPick("##MoguFontColor", Config.Font.MoguFontColor, (value) =>
        {
            Config.Font.MoguFontColor = value;
            Save(Window.WindowName, Config);
        });
        ImGui.SameLine();
        ImGui.Text("-->");
        ImGui.SameLine();
        ImGui.Text($"{Config.Font.MoguFontSpec?.ToLocalizedString("en") ?? "Default"}");

        ImGui.EndDisabled();
    }
}
