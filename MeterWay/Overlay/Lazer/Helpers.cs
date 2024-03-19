using System.Numerics;
using ImGuiNET;
using System.Linq;

using MeterWay.Utils;
using MeterWay.Data;
using MeterWay.Overlay;

namespace Lazer;

public partial class Overlay : IOverlay, IOverlayTab
{
    private static readonly uint colorWhite = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
    private static readonly uint colorBlack = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));

    private class LerpPlayerData
    {
        public double DPS { get; set; }
        public double PctBar { get; set; }
        public double TotalDMG { get; set; }
        public double Position { get; set; }
    }

    private static double Lerp(double firstFloat, double secondFloat, double by)
    {
        return firstFloat + (secondFloat - firstFloat) * by;
    }

    private void GenerateCombatLerping(Player player)
    {
        double FirstPlayerDamageTotal = Combat.Players[SortCache.First()].DamageDealt.Value.Total;
        if (FirstPlayerDamageTotal == 0) FirstPlayerDamageTotal = 1; // divide by zero

        lerpedInfo[player.Id] = new LerpPlayerData
        {
            DPS = player.Dps,
            PctBar = player.DamageDealt.Value.Total / FirstPlayerDamageTotal * 100,
            TotalDMG = player.DamageDealt.Value.Total,
            Position = SortCache.IndexOf(player.Id) + 1
        };

        targetInfo[player.Id] = new LerpPlayerData
        {
            DPS = player.Dps,
            PctBar = player.DamageDealt.Value.Total / FirstPlayerDamageTotal * 100,
            TotalDMG = player.DamageDealt.Value.Total,
            Position = SortCache.IndexOf(player.Id) + 1
        };
    }

    private void DoLerpPlayerData(Player player)
    {
        double transitionDuration = 0.3f; // in seconds
        double transitionTimer = 0.0f;

        if (!lerpedInfo.ContainsKey(player.Id))
        {
            GenerateCombatLerping(player);
        }

        if (transitionTimer < transitionDuration)
        {
            transitionTimer += ImGui.GetIO().DeltaTime;
            double t = transitionTimer / transitionDuration;
            lerpedInfo[player.Id].DPS = Lerp(lerpedInfo[player.Id].DPS, targetInfo[player.Id].DPS, t);
            lerpedInfo[player.Id].PctBar = Lerp(lerpedInfo[player.Id].PctBar, targetInfo[player.Id].PctBar, t);
            lerpedInfo[player.Id].TotalDMG = Lerp(lerpedInfo[player.Id].TotalDMG, targetInfo[player.Id].TotalDMG, t);
            lerpedInfo[player.Id].Position = Lerp(lerpedInfo[player.Id].Position, targetInfo[player.Id].Position, t);
        }

        if (targetInfo[player.Id].TotalDMG <= player.DamageDealt.Value.Total)
        {
            double topPlayerTotalDamage = Combat.Players[SortCache.First()].DamageDealt.Value.Total == 0 ? 1 : Combat.Players[SortCache.First()].DamageDealt.Value.Total;
            targetInfo[player.Id].DPS = player.Dps;
            targetInfo[player.Id].PctBar = player.DamageDealt.Value.Total / topPlayerTotalDamage * 100;
            targetInfo[player.Id].TotalDMG = player.DamageDealt.Value.Total;
            targetInfo[player.Id].Position = SortCache.IndexOf(player.Id) + 1;
            transitionTimer = 0.0f;
        }
    }

    private static void DrawText(Vector2 position, string text, uint color, bool dropShadow = false)
    {
        var draw = ImGui.GetWindowDrawList();
        if (dropShadow) draw.AddText(position + new Vector2(1, 1), color, text);
        draw.AddText(position, color, text);
    }

    private void DrawJobIcon((Vector2 Min, Vector2 Max) position, uint rawJob, bool drawBorder = false)
    {
        var draw = ImGui.GetWindowDrawList();
        if (drawBorder)
        {
            draw.AddRectFilled(position.Min, position.Max, Config.color6);
            draw.AddRect(position.Min, position.Max, Config.color7);
        }
        draw.AddImage(new Job(rawJob).Icon(), position.Min, position.Max);
    }

    private static void DrawProgressBar(Vector2 min, Vector2 max, uint color, float progress)
    {
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(min, new Vector2(min.X + (max.X - min.X) * progress, max.Y), colorBlack, color, color, colorBlack);
    }
}
