using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using ImGuiNET;
using Dalamud.Plugin.Services;
using Dalamud.Interface.ImGuiFontChooserDialog;
using Dalamud.Interface.FontIdentifier;

using MeterWay.Utils;
using MeterWay.Overlay;

namespace Lazer;

public partial class Overlay : IOverlay, IOverlayConfig
{
    private static readonly uint colorWhite = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
    private static readonly uint colorBlack = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));

    private Lerp<Dictionary<uint, PlayerData>> CreateLerping()
    {
        return new(() => // Linear Interpolation
        {
            var now = DateTime.Now;
            double M = (now - Lerping!.Begin!.Value.Time).TotalMilliseconds / (Lerping.End!.Value.Time - Lerping.Begin!.Value.Time).TotalMilliseconds;

            Dictionary<uint, PlayerData> result = [];
            foreach (var key in Lerping.End.Value.Data.Keys)
            {
                result[key] = new PlayerData
                {
                    Progress = Lerping.Begin.Value.Data[key].Progress + M * (Lerping.End.Value.Data[key].Progress - Lerping.Begin.Value.Data[key].Progress),
                };
            }
            return result;
        },
        (left, right) => // Compare
        {
            foreach (var elem in left)
            {
                if (elem.Value.Progress != right[elem.Key].Progress) return false;
            }
            return true;
        });
    }

    private void UpdateLerping()
    {
        uint topDamage = 1;
        if (SortCache.Count != 0) topDamage = Data.Players.GetValueOrDefault(SortCache.First())!.DamageDealt.Value.Total;
        Dictionary<uint, PlayerData> updatedPlayersData = [];
        foreach (var id in SortCache)
        {
            updatedPlayersData.Add(id, new()
            {
                Progress = (float)Data.Players[id].DamageDealt.Value.Total / topDamage,
            });
        }
        if (!(Lerping.End != null && Lerping.End.Value.Data.Keys.ToHashSet().SetEquals([.. updatedPlayersData.Keys]))) Lerping.Reset(); // must contain same content
        Lerping.Update(updatedPlayersData);
    }

    private static void DrawJobIcon(Canvas area, uint job)
    {
        var icon = MeterWay.Dalamud.Textures.GetIcon(job + 62000u, ITextureProvider.IconFlags.None);
        if (icon == null) return;
        ImGui.GetWindowDrawList().AddImage(icon.ImGuiHandle, area.Min, area.Max);
    }

    private static void DrawProgressBar((Vector2 Min, Vector2 Max) rectangle, uint color, float progress)
    {
        rectangle.Max.X = rectangle.Min.X + ((rectangle.Max.X - rectangle.Min.X) * progress);
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(rectangle.Min, rectangle.Max, colorBlack, color, color, colorBlack);
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
