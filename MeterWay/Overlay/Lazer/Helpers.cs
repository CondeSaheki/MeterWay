using System.Numerics;
using ImGuiNET;
using System.Linq;

using MeterWay.Utils.Draw;
using MeterWay.Utils;
using MeterWay.Data;
using MeterWay.Overlay;

namespace Lazer;

public partial class Overlay : IOverlay, IOverlayTab
{
    private void GenerateCombatLerping(Player player)
    {
        double topPlayerDamagePercentage = Combat.Players[SortCache.First()].DamagePercent;
        if (topPlayerDamagePercentage == 0)
        {
            topPlayerDamagePercentage = 1; // Failsafe to avoid division by zero
        }

        lerpedInfo[player.Id] = new LerpPlayerData
        {
            DPS = player.Dps,
            PctBar = player.DamagePercent / topPlayerDamagePercentage * 100,
            TotalDMG = player.DamageDealt.Total,
            Position = SortCache.IndexOf(player.Id) + 1
        };

        targetInfo[player.Id] = new LerpPlayerData
        {
            DPS = player.Dps,
            PctBar = player.DamagePercent / topPlayerDamagePercentage * 100,
            TotalDMG = player.DamageDealt.Total,
            Position = SortCache.IndexOf(player.Id) + 1
        };
    }

    private void DoLerpPlayerData(Player player)
    {
        if (!lerpedInfo.ContainsKey(player.Id))
        {
            GenerateCombatLerping(player);
        }

        if (transitionTimer < transitionDuration)
        {
            transitionTimer += ImGui.GetIO().DeltaTime;
            double t = transitionTimer / transitionDuration;
            lerpedInfo[player.Id].DPS = Helpers.Lerp(lerpedInfo[player.Id].DPS, targetInfo[player.Id].DPS, t);
            lerpedInfo[player.Id].PctBar = Helpers.Lerp(lerpedInfo[player.Id].PctBar, targetInfo[player.Id].PctBar, t);
            lerpedInfo[player.Id].TotalDMG = Helpers.Lerp(lerpedInfo[player.Id].TotalDMG, targetInfo[player.Id].TotalDMG, t);
            lerpedInfo[player.Id].Position = Helpers.Lerp(lerpedInfo[player.Id].Position, targetInfo[player.Id].Position, t);
        }

        if (targetInfo[player.Id].TotalDMG <= player.DamageDealt.Total)
        {
            double topPlayerTotalDamage = Combat.Players[SortCache.First()].DamageDealt.Total == 0 ? 1 : Combat.Players[SortCache.First()].DamageDealt.Total;
            targetInfo[player.Id].DPS = player.Dps;
            targetInfo[player.Id].PctBar = (player.DamageDealt.Total / topPlayerTotalDamage) * 100;
            targetInfo[player.Id].TotalDMG = player.DamageDealt.Total;
            targetInfo[player.Id].Position = SortCache.IndexOf(player.Id) + 1;
            transitionTimer = 0.0f;
        }
    }

    private void DrawPlayerLine(LerpPlayerData data, Player player)
    {
        float lineHeight = ImGui.GetFontSize() + 5;
        var windowMin = new Vector2(WindowMin.X, WindowMin.Y + lineHeight);
        float rowPosition = windowMin.Y + (lineHeight * (float)(data.Position - 1));

        var textRowPosition = rowPosition + lineHeight / 2 - ImGui.GetFontSize() / 2;
        var dps = $"{Helpers.HumanizeNumber(data.DPS, 1)}/s";
        var totalDMG = $"{Helpers.HumanizeNumber(data.TotalDMG, 1)}";
        Widget.JobIcon(player.Job, new Vector2(windowMin.X, rowPosition), lineHeight, true);
        windowMin.X += lineHeight;
        ImGui.GetWindowDrawList().AddRectFilled(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), Helpers.Color(26, 26, 26, 190));
        Widget.DrawProgressBar(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), player.Name == "YOU" ? Helpers.Color(128, 170, 128, 255) : Helpers.Color(128, 128, 170, 255), (float)data.PctBar / 100.0f);
        Widget.DrawBorder(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), Helpers.Color(26, 26, 26, 222));

        Widget.Text(Job.GetName(player.Job), new Vector2(windowMin.X + 8f, textRowPosition), Helpers.Color(144, 144, 144, 190), WindowMin, WindowMax, false, Widget.TextAnchor.Left, false, scale: 0.8f, fontScale: Config.FontScale);
        Widget.Text(player.Name, new Vector2(windowMin.X + (Widget.CalcTextSize(Job.GetName(player.Job), Config.FontScale).X * 0.8f) + 16f, textRowPosition), Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, false, Widget.TextAnchor.Left, false, fontScale: Config.FontScale);
        Widget.Text(dps, new Vector2(WindowMax.X - 5, textRowPosition), Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, false, Widget.TextAnchor.Right, false, fontScale: Config.FontScale);
        Widget.Text(totalDMG, new Vector2(WindowMax.X - (Widget.CalcTextSize(dps, Config.FontScale).X * 1 / 0.8f) - 10f, textRowPosition), Helpers.Color(210, 210, 210, 255), WindowMin, WindowMax, false, Widget.TextAnchor.Right, false, scale: 0.8f, fontScale: Config.FontScale);
    }
}
