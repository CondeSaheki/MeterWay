using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using MeterWay.Utils;
using MeterWay.managers;

namespace MeterWay.Overlays;

public class LazerOverlay : IMeterwayOverlay
{
    public string Name => "LazerOverlay";

    private Encounter encounterData;

    private Vector2 WindowMin { get; set; }
    private Vector2 WindowMax { get; set; }

    public LazerOverlay()
    {
        this.WindowMin = new Vector2();
        this.WindowMax = new Vector2();

        this.encounterData = new Encounter();
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
        this.encounterData = data;
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
        lerpedInfo[player.Name] = new LerpPlayerData { DPS = player.DPS, PctDMG = player.DamagePercentage, TotalDMG = player.TotalDamage, Position = this.encounterData.Players.IndexOf(player) + 1 };
        targetInfo[player.Name] = new LerpPlayerData { DPS = player.DPS, PctDMG = player.DamagePercentage, TotalDMG = player.TotalDamage, Position = this.encounterData.Players.IndexOf(player) + 1 };
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
            targetInfo[player.Name].Position = this.encounterData.Players.IndexOf(player) + 1;
            transitionTimer = 0.0f;
        }
    }

    public void Draw()
    {
        UpdateWindowSize();

        if (this.encounterData.Players == null || this.encounterData.Players.Count() == 0)
        {
            Widget.Text("No players found", new Vector2(WindowMin.X + 2, WindowMin.Y + 2), Helpers.Color(255, 255, 255, 255), fullLine: true, anchor: Widget.TextAnchor.Center, dropShadow: true);
            return;
        }
        // Add a background for the title and centralize the text

        // r
        ImGui.GetWindowDrawList().AddRectFilled(WindowMin, new Vector2(WindowMax.X, WindowMin.Y + (ConfigurationManager.Instance.Configuration.OverlayFontSize * ConfigurationManager.Instance.Configuration.OverlayFontScale) + 5), Helpers.Color(26, 26, 39, 190));
        //

        var center = new Vector2(WindowMin.X + (WindowMax.X - WindowMin.X) / 2 - Widget.CalcTextSize(this.encounterData.Name).X / 2, WindowMin.Y + 2);
        Widget.Text(this.encounterData.Name, center, Helpers.Color(255, 255, 255, 255), anchor: Widget.TextAnchor.Center);
        foreach (Player player in this.encounterData.Players)
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
        // blablabla
        // r
        float OverlayFontScale = ConfigurationManager.Instance.Configuration.OverlayFontScale;
        float OverlayFontSize = ConfigurationManager.Instance.Configuration.OverlayFontSize;
        float lineHeight = (OverlayFontSize * OverlayFontScale) + 5;
        //
        var windowMin = new Vector2(WindowMin.X, WindowMin.Y + lineHeight);
        var rowPosition = windowMin.Y + (lineHeight * (data.Position - 1));

        var textRowPosition = rowPosition + lineHeight / 2 - (OverlayFontSize * OverlayFontScale) / 2;
        var dps = $"{Helpers.HumanizeNumber(data.DPS, 1)}/s";
        var totalDMG = $"{Helpers.HumanizeNumber(data.TotalDMG, 1)}";
        // r
        Widget.JobIcon(player.Job, new Vector2(windowMin.X, rowPosition), lineHeight);
        windowMin.X += lineHeight;
        //

        ImGui.GetWindowDrawList().AddRectFilled(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), Helpers.Color(26, 26, 26, 190));
        Widget.DrawProgressBar(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), player.Name == "YOU" ? Helpers.Color(128, 170, 128, 255) : Helpers.Color(128, 128, 170, 255), data.PctDMG / 100.0f);
        Widget.DrawBorder(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), Helpers.Color(26, 26, 26, 222));

        // r
        Widget.Text(player.Name, new Vector2(windowMin.X + (Widget.CalcTextSize(player.Name).X + 5), textRowPosition), Helpers.Color(255, 255, 255, 255), false, Widget.TextAnchor.Left, false);
        Widget.Text(totalDMG, new Vector2(WindowMax.X - Widget.CalcTextSize(dps).X - Widget.CalcTextSize(totalDMG).X - 20, rowPosition + (Widget.CalcTextSize(totalDMG).Y / 6)), Helpers.Color(210, 210, 210, 255), false, Widget.TextAnchor.Right, false);
        //

        Widget.Text(dps, new Vector2(WindowMax.X - Widget.CalcTextSize(dps).X - 5, textRowPosition), Helpers.Color(255, 255, 255, 255), false, Widget.TextAnchor.Right, false);
    }
}
