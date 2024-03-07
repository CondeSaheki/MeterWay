using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

using MeterWay.Utils.Draw;
using MeterWay.Utils;
using MeterWay.Managers;
using MeterWay.Data;

namespace MeterWay.Overlays;

public class LazerOverlay : MeterwayOverlay
{
    public new static string Name => "Lazer";
    public static string Name2 = "Lazer";

    private Encounter combat;

    private List<uint> sortCache;

    private Vector2 WindowMin { get; set; }
    private Vector2 WindowMax { get; set; }

    class LerpPlayerData
    {
        public double DPS { get; set; }
        public double PctBar { get; set; }
        public double TotalDMG { get; set; }
        public double Position { get; set; }
    }

    private double transitionDuration = 0.3f; // in seconds
    private double transitionTimer = 0.0f;
    private Dictionary<uint, LerpPlayerData> lerpedInfo;
    private Dictionary<uint, LerpPlayerData> targetInfo;

    public LazerOverlay()
    {
        WindowMin = new Vector2();
        WindowMax = new Vector2();

        lerpedInfo = [];
        targetInfo = [];

        combat = new Encounter();
        sortCache = [];
    }

    public override void DataProcess()
    {
        var currentEncounter = EncounterManager.Inst.CurrentEncounter();
        var oldListId = combat.Party.Id;
        var oldCombatId = combat.Id;

        combat = currentEncounter;

        if (currentEncounter.Id != oldCombatId)
        {
            targetInfo = [];
            lerpedInfo = [];

            sortCache = Helpers.CreateDictionarySortCache(combat.Players, (x) => { return true; });
        }

        if (oldListId != currentEncounter.Party.Id)
        {
            sortCache = Helpers.CreateDictionarySortCache(combat.Players, (x) => { return true; });
        }

        sortCache.Sort((uint first, uint second) => { return combat.Players[second].DamageDealt.Total.CompareTo(combat.Players[first].DamageDealt.Total); });
    }

    private List<uint> LocalSortCache() => sortCache.ToList();

    public override void Draw()
    {
        var sortCache = LocalSortCache();
        UpdateWindowSize();
        if (!combat.Finished && combat.Active) combat.Calculate(); // this will ignore last frame data ??

        ImGui.GetWindowDrawList().AddRectFilled(WindowMin, WindowMax, Helpers.Color(ConfigurationManager.Inst.Configuration.OverlayBackgroundColor));

        ImGui.GetWindowDrawList().AddRectFilled(WindowMin, new Vector2(WindowMax.X, WindowMin.Y + (ImGui.GetFont().FontSize + 5) * ConfigurationManager.Inst.Configuration.OverlayFontScale), Helpers.Color(26, 26, 39, 190));

        if (combat.Begin == null)
        {
            Widget.Text("Not in combat", WindowMin, Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, anchor: Widget.TextAnchor.Center, dropShadow: true);
            return;
        }

        Widget.Text($"{combat.Name} - ({(!combat.Active ? "Completed in " : "")}{combat.Duration.ToString(@"mm\:ss")})", WindowMin, Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, anchor: Widget.TextAnchor.Center);

        foreach (var id in sortCache)
        {
            Player player = combat.Players[id];
            player.Calculate();
            DoLerpPlayerData(player);
            DrawPlayerLine(lerpedInfo[player.Id], player);
        }
    }

    public override void Dispose() { }

    private void UpdateWindowSize()
    {
        Vector2 vMin = ImGui.GetWindowContentRegionMin();
        Vector2 vMax = ImGui.GetWindowContentRegionMax();

        vMin.X += ImGui.GetWindowPos().X;
        vMin.Y += ImGui.GetWindowPos().Y;
        vMax.X += ImGui.GetWindowPos().X;
        vMax.Y += ImGui.GetWindowPos().Y;
        WindowMin = vMin;
        WindowMax = vMax;
    }

    private void GenerateCombatLerping(Player player)
    {
        double topPlayerDamagePercentage = combat.Players[sortCache.First()].DamagePercent;
        if (topPlayerDamagePercentage == 0)
        {
            topPlayerDamagePercentage = 1; // Failsafe to avoid division by zero
        }

        lerpedInfo[player.Id] = new LerpPlayerData
        {
            DPS = player.Dps,
            PctBar = player.DamagePercent / topPlayerDamagePercentage * 100,
            TotalDMG = player.DamageDealt.Total,
            Position = sortCache.IndexOf(player.Id) + 1
        };

        targetInfo[player.Id] = new LerpPlayerData
        {
            DPS = player.Dps,
            PctBar = player.DamagePercent / topPlayerDamagePercentage * 100,
            TotalDMG = player.DamageDealt.Total,
            Position = sortCache.IndexOf(player.Id) + 1
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
            double topPlayerTotalDamage = combat.Players[sortCache.First()].DamageDealt.Total == 0 ? 1 : combat.Players[sortCache.First()].DamageDealt.Total;
            targetInfo[player.Id].DPS = player.Dps;
            targetInfo[player.Id].PctBar = (player.DamageDealt.Total / topPlayerTotalDamage) * 100;
            targetInfo[player.Id].TotalDMG = player.DamageDealt.Total;
            targetInfo[player.Id].Position = sortCache.IndexOf(player.Id) + 1;
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

        Widget.Text(Job.GetName(player.Job), new Vector2(windowMin.X + 8f, textRowPosition), Helpers.Color(144, 144, 144, 190), WindowMin, WindowMax, false, Widget.TextAnchor.Left, false, scale: 0.8f);
        Widget.Text(player.Name, new Vector2(windowMin.X + (Widget.CalcTextSize(Job.GetName(player.Job)).X * 0.8f) + 16f, textRowPosition), Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, false, Widget.TextAnchor.Left, false);
        Widget.Text(dps, new Vector2(WindowMax.X - 5, textRowPosition), Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, false, Widget.TextAnchor.Right, false);
        Widget.Text(totalDMG, new Vector2(WindowMax.X - (Widget.CalcTextSize(dps).X * 1 / 0.8f) - 10f, textRowPosition), Helpers.Color(210, 210, 210, 255), WindowMin, WindowMax, false, Widget.TextAnchor.Right, false, scale: 0.8f);
    }
}
