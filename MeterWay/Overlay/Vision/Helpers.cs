using System;
using System.Text;
using System.Numerics;
using ImGuiNET;
using Dalamud.Plugin.Services;
using Dalamud.Interface.ImGuiFontChooserDialog;
using Dalamud.Interface.FontIdentifier;

using MeterWay.Utils;
using MeterWay.Overlay;
using MeterWay.Data;

namespace Vision;

public partial class Overlay : BasicOverlay
{
    public static string ReplacePlaceholders(string format, Player player, Encounter encounter, Func<object?, string> converter)
    {
        var result = new StringBuilder(format);
        int startIndex = 0;

        static int IndexOf(StringBuilder sb, char value, int startIndex)
        {
            for (int i = startIndex; i != sb.Length; ++i) if (sb[i] == value) return i;
            return -1;
        }

        while (true)
        {
            int openIndex = IndexOf(result, '{', startIndex);
            if (openIndex == -1) break;

            int closeIndex = IndexOf(result, '}', openIndex);
            if (closeIndex == -1) break;

            string placeholder = result.ToString(openIndex, closeIndex - openIndex + 1);
            object? replacementObj = placeholder switch
            {
                "{Encounter.Name}" => encounter.Name,
                "{Encounter.Begin}" => encounter.Begin,
                "{Encounter.End}" => encounter.End,
                "{Encounter.Duration}" => encounter.Duration,
                "{Encounter.DamageDealt}" => encounter.DamageDealt,
                "{Encounter.DamageDealt.Count}" => encounter.DamageDealt.Count,
                "{Encounter.DamageDealt.Value}" => encounter.DamageDealt.Value,
                "{Encounter.DamageDealt.Value.Total}" => encounter.DamageDealt.Value.Total,
                "{Encounter.DamageDealt.Value.Critical}" => encounter.DamageDealt.Value.Critical,
                "{Encounter.DamageDealt.Value.Direct}" => encounter.DamageDealt.Value.Direct,
                "{Encounter.DamageDealt.Value.CriticalDirect}" => encounter.DamageDealt.Value.CriticalDirect,
                "{Encounter.DamageDealt.Value.Percent}" => encounter.DamageDealt.Value.Percent,
                "{Encounter.DamageDealt.Value.Percent.Neutral}" => encounter.DamageDealt.Value.Percent.Neutral,
                "{Encounter.DamageDealt.Value.Percent.Critical}" => encounter.DamageDealt.Value.Percent.Critical,
                "{Encounter.DamageDealt.Value.Percent.Direct}" => encounter.DamageDealt.Value.Percent.Direct,
                "{Encounter.DamageDealt.Value.Percent.CriticalDirect}" => encounter.DamageDealt.Value.Percent.CriticalDirect,
                "{Encounter.DamageDealt.Count.Total}" => encounter.DamageDealt.Count.Total,
                "{Encounter.DamageDealt.Count.Critical}" => encounter.DamageDealt.Count.Critical,
                "{Encounter.DamageDealt.Count.Direct}" => encounter.DamageDealt.Count.Direct,
                "{Encounter.DamageDealt.Count.CriticalDirect}" => encounter.DamageDealt.Count.CriticalDirect,
                "{Encounter.DamageDealt.Count.Miss}" => encounter.DamageDealt.Count.Miss,
                "{Encounter.DamageDealt.Count.Parry}" => encounter.DamageDealt.Count.Parry,
                "{Encounter.DamageDealt.Count.Percent}" => encounter.DamageDealt.Count.Percent,
                "{Encounter.DamageDealt.Count.Percent.Neutral}" => encounter.DamageDealt.Count.Percent.Neutral,
                "{Encounter.DamageDealt.Count.Percent.Critical}" => encounter.DamageDealt.Count.Percent.Critical,
                "{Encounter.DamageDealt.Count.Percent.Direct}" => encounter.DamageDealt.Count.Percent.Direct,
                "{Encounter.DamageDealt.Count.Percent.CriticalDirect}" => encounter.DamageDealt.Count.Percent.CriticalDirect,
                "{Encounter.DamageDealt.Count.Percent.Miss}" => encounter.DamageDealt.Count.Percent.Miss,
                "{Encounter.DamageDealt.Count.Percent.Parry}" => encounter.DamageDealt.Count.Percent.Parry,
                "{Encounter.DamageReceived}" => encounter.DamageReceived,
                "{Encounter.DamageReceived.Count}" => encounter.DamageReceived.Count,
                "{Encounter.DamageReceived.Value}" => encounter.DamageReceived.Value,
                "{Encounter.DamageReceived.Value.Total}" => encounter.DamageReceived.Value.Total,
                "{Encounter.DamageReceived.Value.Critical}" => encounter.DamageReceived.Value.Critical,
                "{Encounter.DamageReceived.Value.Direct}" => encounter.DamageReceived.Value.Direct,
                "{Encounter.DamageReceived.Value.CriticalDirect}" => encounter.DamageReceived.Value.CriticalDirect,
                "{Encounter.DamageReceived.Value.Percent}" => encounter.DamageReceived.Value.Percent,
                "{Encounter.DamageReceived.Value.Percent.Neutral}" => encounter.DamageReceived.Value.Percent.Neutral,
                "{Encounter.DamageReceived.Value.Percent.Critical}" => encounter.DamageReceived.Value.Percent.Critical,
                "{Encounter.DamageReceived.Value.Percent.Direct}" => encounter.DamageReceived.Value.Percent.Direct,
                "{Encounter.DamageReceived.Value.Percent.CriticalDirect}" => encounter.DamageReceived.Value.Percent.CriticalDirect,
                "{Encounter.DamageReceived.Count.Total}" => encounter.DamageReceived.Count.Total,
                "{Encounter.DamageReceived.Count.Critical}" => encounter.DamageReceived.Count.Critical,
                "{Encounter.DamageReceived.Count.Direct}" => encounter.DamageReceived.Count.Direct,
                "{Encounter.DamageReceived.Count.CriticalDirect}" => encounter.DamageReceived.Count.CriticalDirect,
                "{Encounter.DamageReceived.Count.Miss}" => encounter.DamageReceived.Count.Miss,
                "{Encounter.DamageReceived.Count.Parry}" => encounter.DamageReceived.Count.Parry,
                "{Encounter.DamageReceived.Count.Percent}" => encounter.DamageReceived.Count.Percent,
                "{Encounter.DamageReceived.Count.Percent.Neutral}" => encounter.DamageReceived.Count.Percent.Neutral,
                "{Encounter.DamageReceived.Count.Percent.Critical}" => encounter.DamageReceived.Count.Percent.Critical,
                "{Encounter.DamageReceived.Count.Percent.Direct}" => encounter.DamageReceived.Count.Percent.Direct,
                "{Encounter.DamageReceived.Count.Percent.CriticalDirect}" => encounter.DamageReceived.Count.Percent.CriticalDirect,
                "{Encounter.DamageReceived.Count.Percent.Miss}" => encounter.DamageReceived.Count.Percent.Miss,
                "{Encounter.DamageReceived.Count.Percent.Parry}" => encounter.DamageReceived.Count.Percent.Parry,
                "{Encounter.HealDealt}" => encounter.HealDealt,
                "{Encounter.HealDealt.Value}" => encounter.HealDealt.Value,
                "{Encounter.HealDealt.Value.Total}" => encounter.HealDealt.Value.Total,
                "{Encounter.HealDealt.Value.Critical}" => encounter.HealDealt.Value.Critical,
                "{Encounter.HealDealt.Value.Percent}" => encounter.HealDealt.Value.Percent,
                "{Encounter.HealDealt.Value.Percent.Neutral}" => encounter.HealDealt.Value.Percent.Neutral,
                "{Encounter.HealDealt.Value.Percent.Critical}" => encounter.HealDealt.Value.Percent.Critical,
                "{Encounter.HealDealt.Count}" => encounter.HealDealt.Count,
                "{Encounter.HealDealt.Count.Total}" => encounter.HealDealt.Count.Total,
                "{Encounter.HealDealt.Count.Critical}" => encounter.HealDealt.Count.Critical,
                "{Encounter.HealDealt.Count.Percent}" => encounter.HealDealt.Count.Percent,
                "{Encounter.HealDealt.Count.Percent.Neutral}" => encounter.HealDealt.Count.Percent.Neutral,
                "{Encounter.HealDealt.Count.Percent.Critical}" => encounter.HealDealt.Count.Percent.Critical,
                "{Encounter.HealReceived}" => encounter.HealReceived,
                "{Encounter.HealReceived.Value}" => encounter.HealReceived.Value,
                "{Encounter.HealReceived.Value.Total}" => encounter.HealReceived.Value.Total,
                "{Encounter.HealReceived.Value.Critical}" => encounter.HealReceived.Value.Critical,
                "{Encounter.HealReceived.Value.Percent}" => encounter.HealReceived.Value.Percent,
                "{Encounter.HealReceived.Value.Percent.Neutral}" => encounter.HealReceived.Value.Percent.Neutral,
                "{Encounter.HealReceived.Value.Percent.Critical}" => encounter.HealReceived.Value.Percent.Critical,
                "{Encounter.HealReceived.Count}" => encounter.HealReceived.Count,
                "{Encounter.HealReceived.Count.Total}" => encounter.HealReceived.Count.Total,
                "{Encounter.HealReceived.Count.Critical}" => encounter.HealReceived.Count.Critical,
                "{Encounter.HealReceived.Count.Percent}" => encounter.HealReceived.Count.Percent,
                "{Encounter.HealReceived.Count.Percent.Neutral}" => encounter.HealReceived.Count.Percent.Neutral,
                "{Encounter.HealReceived.Count.Percent.Critical}" => encounter.HealReceived.Count.Percent.Critical,
                "{Player.Name}" => player.Name,
                "{Player.World}" => player.World,
                "{Player.Job}" => player.Job,
                "{Player.DamageDealt}" => player.DamageDealt,
                "{Player.DamageDealt.Value}" => player.DamageDealt.Value,
                "{Player.DamageDealt.Value.Total}" => player.DamageDealt.Value.Total,
                "{Player.DamageDealt.Value.Critical}" => player.DamageDealt.Value.Critical,
                "{Player.DamageDealt.Value.Direct}" => player.DamageDealt.Value.Direct,
                "{Player.DamageDealt.Value.CriticalDirect}" => player.DamageDealt.Value.CriticalDirect,
                "{Player.DamageDealt.Value.Percent}" => player.DamageDealt.Value.Percent,
                "{Player.DamageDealt.Value.Percent.Neutral}" => player.DamageDealt.Value.Percent.Neutral,
                "{Player.DamageDealt.Value.Percent.Critical}" => player.DamageDealt.Value.Percent.Critical,
                "{Player.DamageDealt.Value.Percent.Direct}" => player.DamageDealt.Value.Percent.Direct,
                "{Player.DamageDealt.Value.Percent.CriticalDirect}" => player.DamageDealt.Value.Percent.CriticalDirect,
                "{Player.DamageDealt.Count}" => player.DamageDealt.Count,
                "{Player.DamageDealt.Count.Total}" => player.DamageDealt.Count.Total,
                "{Player.DamageDealt.Count.Critical}" => player.DamageDealt.Count.Critical,
                "{Player.DamageDealt.Count.Direct}" => player.DamageDealt.Count.Direct,
                "{Player.DamageDealt.Count.CriticalDirect}" => player.DamageDealt.Count.CriticalDirect,
                "{Player.DamageDealt.Count.Miss}" => player.DamageDealt.Count.Miss,
                "{Player.DamageDealt.Count.Parry}" => player.DamageDealt.Count.Parry,
                "{Player.DamageDealt.Count.Percent}" => player.DamageDealt.Count.Percent,
                "{Player.DamageDealt.Count.Percent.Neutral}" => player.DamageDealt.Count.Percent.Neutral,
                "{Player.DamageDealt.Count.Percent.Critical}" => player.DamageDealt.Count.Percent.Critical,
                "{Player.DamageDealt.Count.Percent.Direct}" => player.DamageDealt.Count.Percent.Direct,
                "{Player.DamageDealt.Count.Percent.CriticalDirect}" => player.DamageDealt.Count.Percent.CriticalDirect,
                "{Player.DamageDealt.Count.Percent.Miss}" => player.DamageDealt.Count.Percent.Miss,
                "{Player.DamageDealt.Count.Percent.Parry}" => player.DamageDealt.Count.Percent.Parry,
                "{Player.DamageReceived}" => player.DamageReceived,
                "{Player.DamageReceived.Value}" => player.DamageReceived.Value,
                "{Player.DamageReceived.Value.Total}" => player.DamageReceived.Value.Total,
                "{Player.DamageReceived.Value.Critical}" => player.DamageReceived.Value.Critical,
                "{Player.DamageReceived.Value.Direct}" => player.DamageReceived.Value.Direct,
                "{Player.DamageReceived.Value.CriticalDirect}" => player.DamageReceived.Value.CriticalDirect,
                "{Player.DamageReceived.Value.Percent}" => player.DamageReceived.Value.Percent,
                "{Player.DamageReceived.Value.Percent.Neutral}" => player.DamageReceived.Value.Percent.Neutral,
                "{Player.DamageReceived.Value.Percent.Critical}" => player.DamageReceived.Value.Percent.Critical,
                "{Player.DamageReceived.Value.Percent.Direct}" => player.DamageReceived.Value.Percent.Direct,
                "{Player.DamageReceived.Value.Percent.CriticalDirect}" => player.DamageReceived.Value.Percent.CriticalDirect,
                "{Player.DamageReceived.Count}" => player.DamageReceived.Count,
                "{Player.DamageReceived.Count.Total}" => player.DamageReceived.Count.Total,
                "{Player.DamageReceived.Count.Critical}" => player.DamageReceived.Count.Critical,
                "{Player.DamageReceived.Count.Direct}" => player.DamageReceived.Count.Direct,
                "{Player.DamageReceived.Count.CriticalDirect}" => player.DamageReceived.Count.CriticalDirect,
                "{Player.DamageReceived.Count.Miss}" => player.DamageReceived.Count.Miss,
                "{Player.DamageReceived.Count.Parry}" => player.DamageReceived.Count.Parry,
                "{Player.DamageReceived.Count.Percent}" => player.DamageReceived.Count.Percent,
                "{Player.DamageReceived.Count.Percent.Neutral}" => player.DamageReceived.Count.Percent.Neutral,
                "{Player.DamageReceived.Count.Percent.Critical}" => player.DamageReceived.Count.Percent.Critical,
                "{Player.DamageReceived.Count.Percent.Direct}" => player.DamageReceived.Count.Percent.Direct,
                "{Player.DamageReceived.Count.Percent.CriticalDirect}" => player.DamageReceived.Count.Percent.CriticalDirect,
                "{Player.DamageReceived.Count.Percent.Miss}" => player.DamageReceived.Count.Percent.Miss,
                "{Player.DamageReceived.Count.Percent.Parry}" => player.DamageReceived.Count.Percent.Parry,
                "{Player.HealDealt}" => player.HealDealt,
                "{Player.HealDealt.Value}" => player.HealDealt.Value,
                "{Player.HealDealt.Value.Total}" => player.HealDealt.Value.Total,
                "{Player.HealDealt.Value.Critical}" => player.HealDealt.Value.Critical,
                "{Player.HealDealt.Value.Percent}" => player.HealDealt.Value.Percent,
                "{Player.HealDealt.Value.Percent.Neutral}" => player.HealDealt.Value.Percent.Neutral,
                "{Player.HealDealt.Value.Percent.Critical}" => player.HealDealt.Value.Percent.Critical,
                "{Player.HealDealt.Count}" => player.HealDealt.Count,
                "{Player.HealDealt.Count.Total}" => player.HealDealt.Count.Total,
                "{Player.HealDealt.Count.Critical}" => player.HealDealt.Count.Critical,
                "{Player.HealDealt.Count.Percent}" => player.HealDealt.Count.Percent,
                "{Player.HealDealt.Count.Percent.Neutral}" => player.HealDealt.Count.Percent.Neutral,
                "{Player.HealDealt.Count.Percent.Critical}" => player.HealDealt.Count.Percent.Critical,
                "{Player.HealReceived}" => player.HealReceived,
                "{Player.HealReceived.Value}" => player.HealReceived.Value,
                "{Player.HealReceived.Value.Total}" => player.HealReceived.Value.Total,
                "{Player.HealReceived.Value.Critical}" => player.HealReceived.Value.Critical,
                "{Player.HealReceived.Value.Percent}" => player.HealReceived.Value.Percent,
                "{Player.HealReceived.Value.Percent.Neutral}" => player.HealReceived.Value.Percent.Neutral,
                "{Player.HealReceived.Value.Percent.Critical}" => player.HealReceived.Value.Percent.Critical,
                "{Player.HealReceived.Count}" => player.HealReceived.Count,
                "{Player.HealReceived.Count.Total}" => player.HealReceived.Count.Total,
                "{Player.HealReceived.Count.Critical}" => player.HealReceived.Count.Critical,
                "{Player.HealReceived.Count.Percent}" => player.HealReceived.Count.Percent,
                "{Player.HealReceived.Count.Percent.Neutral}" => player.HealReceived.Count.Percent.Neutral,
                "{Player.HealReceived.Count.Percent.Critical}" => player.HealReceived.Count.Percent.Critical,
                _ => placeholder
            };
            var replacement = converter(replacementObj);

            result.Remove(openIndex, closeIndex - openIndex + 1);
            result.Insert(openIndex, replacement);
            startIndex = openIndex + replacement.Length;
        }
        return result.ToString();
    }

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