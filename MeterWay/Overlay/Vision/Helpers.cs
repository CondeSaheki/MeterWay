using System;
using System.Numerics;
using ImGuiNET;
using Dalamud.Plugin.Services;
using Dalamud.Interface.ImGuiFontChooserDialog;
using Dalamud.Interface.FontIdentifier;

using MeterWay.Utils;
using MeterWay.Overlay;

namespace Vision;

public partial class Overlay : BasicOverlay
{
    private static void DrawTextShadow(Vector2 position, uint color, string text, uint shadowColor)
    {
        ImGui.GetWindowDrawList().AddText(position + new Vector2(1, 1), shadowColor, text);
        ImGui.GetWindowDrawList().AddText(position, color, text);
    }

    private static void DrawProgressBar((Vector2 Min, Vector2 Max) rectangle, float progress, uint color)
    {
        rectangle.Max.X = rectangle.Min.X + ((rectangle.Max.X - rectangle.Min.X) * progress);
        ImGui.GetWindowDrawList().AddRectFilled(rectangle.Min, rectangle.Max, color);
    }

    private static bool DrawAlingmentButtons(ref int horizontal)
    {
        var result = false;
        if (ImGui.RadioButton("Left", ref horizontal, 0)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Center", ref horizontal, 1)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Right", ref horizontal, 2)) result = true;
        return result;
    }

    private static bool DrawAlingmentButtons(ref int horizontal, ref int vertical)
    {
        var result = false;
        if (ImGui.RadioButton("Left", ref horizontal, 0)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Center", ref horizontal, 1)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Right", ref horizontal, 2)) result = true;

        ImGui.Spacing();
        if (ImGui.RadioButton("Top", ref vertical, 0)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Center", ref vertical, 1)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Botton", ref vertical, 2)) result = true;
        return result;
    }

    private static void DrawJobIcon(Canvas area, uint job)
    {
        var icon = MeterWay.Dalamud.Textures.GetIcon(job + 62000u, ITextureProvider.IconFlags.None);
        if (icon == null) return;

        ImGui.GetWindowDrawList().AddImage(icon.ImGuiHandle, area.Min, area.Max);
    }
    
    private void SavaCurrentWindowData()
    {
        if (Window.CurrentPosition != null) Config.General.Position = (Vector2)Window.CurrentPosition;
        if (Window.CurrentSize != null) Config.General.Size = (Vector2)Window.CurrentSize;
        Save(Window.WindowName, Config);
    }

    public void _ImGuiBeginDisabled(ref bool value)
    {
        if (!value) ImGui.BeginDisabled();
        value = true;
    }

    public void _ImGuiEndDisabled(ref bool value)
    {
        if (value) ImGui.EndDisabled();
        value = false;
    }

    public static void _ImguiCheckboxWithTooltip(string? label, string? tooltip, bool current, Action<bool> setter)
    {
        var currentValue = current;
        if (ImGui.Checkbox(label ?? "", ref currentValue) && currentValue != current) setter(currentValue);

        if (!string.IsNullOrEmpty(tooltip) && ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(tooltip);
            ImGui.EndTooltip();
        }
    }

    private static void _ImGuiColorPick(string Label, uint current, Action<uint> setter)
    {
        var colorValue = ImGui.ColorConvertU32ToFloat4(current);
        if (ImGui.ColorEdit4(Label, ref colorValue,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault))
        {
            var converted = ImGui.ColorConvertFloat4ToU32(colorValue);
            if (current != converted) setter(ImGui.ColorConvertFloat4ToU32(colorValue));
        }
    }

    private void _SingleFontChooserDialog(string? label, SingleFontSpec? current, Action<SingleFontSpec> setter)
    {
        SingleFontChooserDialog chooser = new(MeterWay.Dalamud.PluginInterface.UiBuilder, false, null)
        {
            Title = label ?? "Font Chooser",
            PreviewText = "0.123456789 abcdefghijklmnopqrstuvxyzw",
            SelectedFont = current ?? new SingleFontSpec { FontId = DalamudDefaultFontAndFamilyId.Instance },
            IsModal = false,
        };
        chooser.ResultTask.ContinueWith(chooserTask =>
        {
            _ = chooserTask.Exception; // not needed ?
            if (chooserTask.IsCompletedSuccessfully) setter(chooserTask.Result);
            MeterWay.Dalamud.PluginInterface.UiBuilder.Draw -= chooser.Draw;
            chooser.Dispose();
        });
        MeterWay.Dalamud.PluginInterface.UiBuilder.Draw += chooser.Draw;
    }
}