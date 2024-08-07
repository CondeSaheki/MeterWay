using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using ImGuiNET;
using Dalamud.Interface.ImGuiFontChooserDialog;
using Dalamud.Interface.FontIdentifier;

using MeterWay.Utils;
using Dalamud.Interface;

namespace Lazer;

public partial class Overlay
{
    private static readonly uint ColorWhite = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
    private static readonly uint ColorBlack = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));

    private Lerp<Dictionary<uint, PlayerData>> CreateLerping()
    {
        return new Lerp<Dictionary<uint, PlayerData>>(() =>
        {
            var now = DateTime.Now;
            var coefficient = Easings.OutSine((now - Lerping.Begin!.Value.Time).TotalMilliseconds / (Lerping.End!.Value.Time - Lerping.Begin!.Value.Time).TotalMilliseconds);

            Dictionary<uint, PlayerData> result = new();
            foreach (var key in Lerping.End.Value.Data.Keys)
            {
                result[key] = new PlayerData
                {
                    Progress = Lerping.Begin.Value.Data[key].Progress + coefficient * (Lerping.End.Value.Data[key].Progress - Lerping.Begin.Value.Data[key].Progress),
                    Damage = Lerping.Begin.Value.Data[key].Damage + coefficient * (Lerping.End.Value.Data[key].Damage - Lerping.Begin.Value.Data[key].Damage),
                    CurrentPosition = Lerping.Begin.Value.Data[key].CurrentPosition + coefficient * (Lerping.End.Value.Data[key].CurrentPosition - Lerping.Begin.Value.Data[key].CurrentPosition)
                };
            }
            return result;
        },
        (left, right) => // Compare
        {
            foreach (var elem in left)
            {
                if (Math.Abs(elem.Value.Progress - right[elem.Key].Progress) > 0.001) return false;
                if (Math.Abs(elem.Value.Damage - right[elem.Key].Damage) >  0.001) return false;
                if (Math.Abs(elem.Value.CurrentPosition - right[elem.Key].CurrentPosition) > 0.001) return false;
            }
            return true;
        })
        {
            Interval = TimeSpan.FromSeconds(0.5)
        };
    }

    private void UpdateLerping()
    {
        if (Data == null) return;
        uint topDamage = 1;
        if (SortCache.Count != 0) topDamage = Data.Players.GetValueOrDefault(SortCache.First())!.DamageDealt.Value.Total;
        Dictionary<uint, PlayerData> updatedPlayersData = [];
        foreach (var id in SortCache)
        {
            updatedPlayersData.Add(id, new PlayerData
            {
                Progress = (float)Data.Players[id].DamageDealt.Value.Total / topDamage,
                Damage = Data.Players[id].DamageDealt.Value.Total,
                CurrentPosition = SortCache.IndexOf(id)
            });
        }
        if (!(Lerping.End != null && Lerping.End.Value.Data.Keys.ToHashSet().SetEquals([.. updatedPlayersData.Keys]))) Lerping.Reset(); // must contain same content
        Lerping.Update(updatedPlayersData);
    }

    private static void DrawJobIcon(Canvas area, Job job)
    {
        var icon = job.Icon();
        if (icon == null) return;
        ImGui.GetWindowDrawList().AddImage((nint)icon, area.Min, area.Max);
    }

    private static void DrawProgressBar((Vector2 Min, Vector2 Max) rectangle, uint color, float progress)
    {
        rectangle.Max.X = rectangle.Min.X + ((rectangle.Max.X - rectangle.Min.X) * progress);
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(rectangle.Min, rectangle.Max, ColorBlack, color, color, ColorBlack);
    }
    private void DrawProgressBar((Vector2 Min, Vector2 Max) rectangle, uint color, float progress, bool usePlayerColor)
    {
        rectangle.Max.X = rectangle.Min.X + ((rectangle.Max.X - rectangle.Min.X) * progress);
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(rectangle.Min, rectangle.Max, usePlayerColor ? Config.Appearance.YourBarColor : ColorBlack, color, color, usePlayerColor ? Config.Appearance.YourBarColor : ColorBlack);
    }


    private uint GetJobColor(Job job)
    {
        if (job == Job.Paladin || job == Job.Gladiator) return Config.Appearance.JobColors.Paladin;
        if (job == Job.Warrior || job == Job.Marauder) return Config.Appearance.JobColors.Warrior;
        if (job == Job.DarkKnight) return Config.Appearance.JobColors.DarkKnight;
        if (job == Job.GunBreaker) return Config.Appearance.JobColors.GunBreaker;

        if (job == Job.Monk || job == Job.Pugilist) return Config.Appearance.JobColors.Monk;
        if (job == Job.Ninja || job == Job.Rogue) return Config.Appearance.JobColors.Ninja;
        if (job == Job.Dragoon || job == Job.Lancer) return Config.Appearance.JobColors.Dragoon;
        if (job == Job.Samurai) return Config.Appearance.JobColors.Samurai;
        if (job == Job.Reaper) return Config.Appearance.JobColors.Reaper;
        if (job == Job.Viper) return Config.Appearance.JobColors.Viper;

        if (job == Job.WhiteMage || job == Job.Conjurer) return Config.Appearance.JobColors.WhiteMage;
        if (job == Job.Scholar) return Config.Appearance.JobColors.Scholar;
        if (job == Job.Astrologian) return Config.Appearance.JobColors.Astrologian;
        if (job == Job.Sage) return Config.Appearance.JobColors.Sage;

        if (job == Job.Bard || job == Job.Archer) return Config.Appearance.JobColors.Bard;
        if (job == Job.Machinist) return Config.Appearance.JobColors.Machinist;
        if (job == Job.Dancer) return Config.Appearance.JobColors.Dancer;

        if (job == Job.BlackMage || job == Job.Thaumaturge) return Config.Appearance.JobColors.BlackMage;
        if (job == Job.Summoner || job == Job.Arcanist) return Config.Appearance.JobColors.Summoner;
        if (job == Job.RedMage) return Config.Appearance.JobColors.RedMage;
        if (job == Job.Pictomancer) return Config.Appearance.JobColors.Pictomancer;
        if (job == Job.BlueMage) return Config.Appearance.JobColors.BlueMage;

        return Config.Appearance.JobColors.Default;
    }

    private uint GetRoleColor(Job job)
    {
        if (Job.IsTank(job)) return Config.Appearance.JobColors.Tank;
        if (Job.IsMeleeDps(job)) return Config.Appearance.JobColors.MeleeDps;
        if (Job.IsHealer(job)) return Config.Appearance.JobColors.Healer;
        if (Job.IsPhysicalRangedDps(job)) return Config.Appearance.JobColors.PhysicalRangedDps;
        if (Job.IsMagicalRangedDps(job)) return Config.Appearance.JobColors.MagicalRangedDps;
        return Config.Appearance.JobColors.Default;
    }

    private void SaveCurrentWindowData()
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

    private void _SingleFontChooserDialog(string? label, IFontSpec? current, Action<SingleFontSpec> setter)
    {
        SingleFontChooserDialog chooser = new((UiBuilder)MeterWay.Dalamud.PluginInterface.UiBuilder, false, null)
        {
            Title = label ?? "Font Chooser",
            PreviewText = "0.123456789 abcdefghijklmnopqrstuvxyzw",
            SelectedFont = (SingleFontSpec?)current ?? new SingleFontSpec { FontId = DalamudDefaultFontAndFamilyId.Instance },
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
