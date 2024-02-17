using System;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using MeterWay.Utils;
using MeterWay.managers;

namespace MeterWay.Overlays;

public class LazerOverlay : IMeterwayOverlay
{
    public string Name => "LazerOverlay";

    private Encounter combat;

    private Vector2 WindowMin { get; set; }
    private Vector2 WindowMax { get; set; }

    public LazerOverlay()
    {
        this.WindowMin = new Vector2();
        this.WindowMax = new Vector2();

        this.combat = new Encounter();
    }

    class LerpPlayerData
    {
        public float DPS { get; set; }
        public float PctDMG { get; set; }
        public float TotalDMG { get; set; }
        public float Position { get; set; }
    }


    public void DataProcess(Encounter data)
    {
        data.Players.Sort((p1, p2) => p2.DPS.CompareTo(p1.DPS));

        this.combat = data;
    }

    private float transitionDuration = 1f; // in seconds
    private float transitionTimer = 0.0f;
    private Dictionary<string, LerpPlayerData> lerpedInfo = new Dictionary<string, LerpPlayerData>();
    private Dictionary<string, LerpPlayerData> targetInfo = new Dictionary<string, LerpPlayerData>();

    private void UpdateWindowSize()
    {
        Vector2 vMin = ImGui.GetWindowContentRegionMin();
        Vector2 vMax = ImGui.GetWindowContentRegionMax();

        vMin.X += ImGui.GetWindowPos().X;
        vMin.Y += ImGui.GetWindowPos().Y;
        vMax.X += ImGui.GetWindowPos().X;
        vMax.Y += ImGui.GetWindowPos().Y;
        this.WindowMin = vMin;
        this.WindowMax = vMax;
    }


    private void GenerateCombatLerping(Player player)
    {
        lerpedInfo[player.Name] = new LerpPlayerData { DPS = player.DPS, PctDMG = player.DamagePercentage, TotalDMG = player.TotalDamage, Position = this.combat.Players.IndexOf(player) + 1 };
        targetInfo[player.Name] = new LerpPlayerData { DPS = player.DPS, PctDMG = player.DamagePercentage, TotalDMG = player.TotalDamage, Position = this.combat.Players.IndexOf(player) + 1 };
    }

    private void DoLerpPlayerData(Player player)
    {
        if (!lerpedInfo.ContainsKey(player.Name))
        {
            GenerateCombatLerping(player);
        }

        if (transitionTimer < transitionDuration)
        {
            transitionTimer += ImGui.GetIO().DeltaTime;
            float t = Math.Min(1.0f, transitionTimer / transitionDuration);
            lerpedInfo[player.Name].DPS = Helpers.Lerp(lerpedInfo[player.Name].DPS, targetInfo[player.Name].DPS, t);
            lerpedInfo[player.Name].PctDMG = Helpers.Lerp(lerpedInfo[player.Name].PctDMG, targetInfo[player.Name].PctDMG, t);
            lerpedInfo[player.Name].TotalDMG = Helpers.Lerp(lerpedInfo[player.Name].TotalDMG, targetInfo[player.Name].TotalDMG, t);
            lerpedInfo[player.Name].Position = Helpers.Lerp(lerpedInfo[player.Name].Position, targetInfo[player.Name].Position, t);
        }

        if (targetInfo[player.Name].DPS != player.DPS)
        {
            targetInfo[player.Name].DPS = player.DPS;
            targetInfo[player.Name].PctDMG = player.DamagePercentage;
            targetInfo[player.Name].TotalDMG = player.TotalDamage;
            targetInfo[player.Name].Position = this.combat.Players.IndexOf(player) + 1;
            transitionTimer = 0.0f;
        }
    }

    public void Draw()
    {
        UpdateWindowSize();
        ImGui.GetWindowDrawList().AddRectFilled(WindowMin, WindowMax, Helpers.Color(ConfigurationManager.Instance.Configuration.OverlayBackgroundColor));

        ImGui.GetWindowDrawList().AddRectFilled(WindowMin, new Vector2(WindowMax.X, WindowMin.Y + (ImGui.GetFont().FontSize + 5) * ConfigurationManager.Instance.Configuration.OverlayFontScale), Helpers.Color(26, 26, 39, 190));


        if (!this.combat.active && this.combat.duration.Seconds == 0) {
           Widget.Text("Not in combat", WindowMin, Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, anchor: Widget.TextAnchor.Center, dropShadow: true);
           return;
        }


        Widget.Text($"{this.combat.Name} - ({(!this.combat.active ? "Completed in " : "")}{this.combat.duration.ToString(@"mm\:ss")})", WindowMin, Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, anchor: Widget.TextAnchor.Center);
        foreach (Player player in this.combat.Players)
        {
            DoLerpPlayerData(player);
            DrawPlayerLine(lerpedInfo[player.Name], player);
        }
    }

    public void Dispose()
    {
    }
    private void DrawPlayerLine(LerpPlayerData data, Player player)
    {
        float lineHeight = ImGui.GetFontSize() + 5;
        var windowMin = new Vector2(WindowMin.X, WindowMin.Y + lineHeight);
        var rowPosition = windowMin.Y + (lineHeight * (data.Position - 1));

        var textRowPosition = rowPosition + lineHeight / 2 - ImGui.GetFontSize() / 2;
        var dps = $"{Helpers.HumanizeNumber(data.DPS, 1)}/s";
        var totalDMG = $"{Helpers.HumanizeNumber(data.TotalDMG, 1)}";
        Widget.JobIcon(player.Job, new Vector2(windowMin.X, rowPosition), lineHeight, true);
        windowMin.X += lineHeight;
        ImGui.GetWindowDrawList().AddRectFilled(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), Helpers.Color(26, 26, 26, 190));
        Widget.DrawProgressBar(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), player.Name == "YOU" ? Helpers.Color(128, 170, 128, 255) : Helpers.Color(128, 128, 170, 255), data.PctDMG / 100.0f);
        Widget.DrawBorder(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), Helpers.Color(26, 26, 26, 222));

        Widget.Text(Job.GetName(player.Job), new Vector2(windowMin.X + 8f, textRowPosition), Helpers.Color(144, 144, 144, 190), WindowMin, WindowMax, false, Widget.TextAnchor.Left, false, scale: 0.8f);
        Widget.Text(player.Name, new Vector2(windowMin.X + (Widget.CalcTextSize(Job.GetName(player.Job)).X * 0.8f) + 16f, textRowPosition), Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, false, Widget.TextAnchor.Left, false);
        Widget.Text(dps, new Vector2(WindowMax.X - 5, textRowPosition), Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, false, Widget.TextAnchor.Right, false);
        Widget.Text(totalDMG, new Vector2(WindowMax.X - (Widget.CalcTextSize(dps).X * 1 / 0.8f) - 10f, textRowPosition), Helpers.Color(210, 210, 210, 255), WindowMin, WindowMax, false, Widget.TextAnchor.Right, false, scale: 0.8f);
    }
}
