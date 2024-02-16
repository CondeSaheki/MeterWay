using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using Meterway.Utils;
using System.Security.Cryptography.X509Certificates;
using Dalamud.Interface.Windowing;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

namespace Meterway.Overlays;

public class LazerOverlay : IMeterwayOverlay
{
    public string Name => "LazerOverlay";

    private CombatData? combatData;

    private Vector2 WindowMin { get; set; }
    private Vector2 WindowMax { get; set; }

    public LazerOverlay()
    {
        this.WindowMin = new Vector2();
        this.WindowMax = new Vector2();
    }

    class LerpCombatantData
    {
        public float DPS { get; set; }
        public float PctDMG { get; set; }
        public float TotalDMG { get; set; }
        public float Position { get; set; }
    }

    private float transitionDuration = 1f; // in seconds
    private float transitionTimer = 0.0f;
    private Dictionary<string, LerpCombatantData> lerpedInfo = new Dictionary<string, LerpCombatantData>();
    private Dictionary<string, LerpCombatantData> targetInfo = new Dictionary<string, LerpCombatantData>();

    // For now we hardcode the jobId's, but we should probably get them from the game data
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

    private void GenerateCombatLerping(Combatant combatant)
    {
        if (this.combatData == null)
        {
            return;
        }
        lerpedInfo[combatant.Name] = new LerpCombatantData { DPS = combatant.EncDps, PctDMG = combatant.DmgPct, TotalDMG = combatant.Dmg, Position = this.combatData.combatants.IndexOf(combatant) + 1 };
        targetInfo[combatant.Name] = new LerpCombatantData { DPS = combatant.EncDps, PctDMG = combatant.DmgPct, TotalDMG = combatant.Dmg, Position = this.combatData.combatants.IndexOf(combatant) + 1 };
    }

    private void DoLerpCombatantData(Combatant combatant)
    {
        if (this.combatData == null)
        {
            return;
        }
        if (!lerpedInfo.ContainsKey(combatant.Name))
        {
            GenerateCombatLerping(combatant);
        }

        if (transitionTimer < transitionDuration)
        {
            transitionTimer += ImGui.GetIO().DeltaTime;
            float t = Math.Min(1.0f, transitionTimer / transitionDuration);
            lerpedInfo[combatant.Name].DPS = Helpers.Lerp(lerpedInfo[combatant.Name].DPS, targetInfo[combatant.Name].DPS, t);
            lerpedInfo[combatant.Name].PctDMG = Helpers.Lerp(lerpedInfo[combatant.Name].PctDMG, targetInfo[combatant.Name].PctDMG, t);
            lerpedInfo[combatant.Name].TotalDMG = Helpers.Lerp(lerpedInfo[combatant.Name].TotalDMG, targetInfo[combatant.Name].TotalDMG, t);
            lerpedInfo[combatant.Name].Position = Helpers.Lerp(lerpedInfo[combatant.Name].Position, targetInfo[combatant.Name].Position, t);
        }

        if (targetInfo[combatant.Name].DPS != combatant.EncDps)
        {
            targetInfo[combatant.Name].DPS = combatant.EncDps;
            targetInfo[combatant.Name].PctDMG = combatant.DmgPct;
            targetInfo[combatant.Name].TotalDMG = combatant.Dmg;
            targetInfo[combatant.Name].Position = this.combatData.combatants.IndexOf(combatant) + 1;
            transitionTimer = 0.0f;
        }
    }

    public void Draw()
    {
        UpdateWindowSize();

        if (this.combatData == null)
        {
            Widget.Text("Not in combat...", new Vector2(WindowMin.X + 2, WindowMin.Y + 2), Helpers.Color(255, 255, 255, 255), fullLine: true, anchor: Widget.TextAnchor.Center, dropShadow: true);
            return;
        }

        if (this.combatData.combatants == null || this.combatData.combatants.Count() == 0)
        {
            Widget.Text("No combatants found", new Vector2(WindowMin.X + 2, WindowMin.Y + 2), Helpers.Color(255, 255, 255, 255), fullLine: true, anchor: Widget.TextAnchor.Center, dropShadow: true);
            return;
        }
        // Add a background for the title and centralize the text
        ImGui.GetWindowDrawList().AddRectFilled(new Vector2(WindowMin.X, WindowMin.Y), new Vector2(WindowMax.X, WindowMin.Y + ImGui.GetFontSize() + 5), Helpers.Color(26, 26, 39, 190));

        var center = new Vector2(WindowMin.X + (WindowMax.X - WindowMin.X) / 2 - (Widget.CalcTextSize(this.combatData.encounter.Name).X) / 2, WindowMin.Y + 2);
        Widget.Text(this.combatData.encounter.Name, center, Helpers.Color(255, 255, 255, 255), anchor: Widget.TextAnchor.Center);
        foreach (Combatant combatant in this.combatData.combatants)
        {
            DoLerpCombatantData(combatant);
            DrawCombatantLine(lerpedInfo[combatant.Name], combatant.Name, combatant.Job);
        }
    }

    public void Dispose()
    {
    }

    // Draw helpers

    private void DrawCombatantLine(LerpCombatantData data, string name, string job)
    {
        float lineHeight = ImGui.GetFontSize() + 5;
        var windowMin = new Vector2(WindowMin.X, WindowMin.Y + lineHeight);
        var rowPosition = windowMin.Y + (lineHeight * (data.Position - 1));

        var textRowPosition = rowPosition + lineHeight / 2 - ImGui.GetFontSize() / 2;
        var dps = $"{Helpers.HumanizeNumber(data.DPS, 1)}/s";
        var totalDMG = $"{Helpers.HumanizeNumber(data.TotalDMG, 1)}";

        var drawList = ImGui.GetWindowDrawList();
        Widget.JobIcon(job, new Vector2(windowMin.X, rowPosition), lineHeight);
        windowMin.X += lineHeight;

        drawList.AddRectFilled(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), Helpers.Color(26, 26, 26, 190));
        DrawProgressBar(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), name == "YOU" ? Helpers.Color(128, 170, 128, 255) : Helpers.Color(128, 128, 170, 255), data.PctDMG / 100.0f);
        DrawBorder(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), Helpers.Color(26, 26, 26, 222));

        // scale down the font for the job and center it in the line
        Widget.Text(job.ToUpper(), new Vector2(windowMin.X + 7, rowPosition + (lineHeight / 2 - (Widget.CalcTextSize(job.ToUpper()).Y) / 2)), Helpers.Color(172, 172, 172, 255), anchor: Widget.TextAnchor.Left);

        Widget.Text(name, new Vector2(windowMin.X + 10 + (0.8f * Widget.CalcTextSize(job.ToUpper()).X), textRowPosition), Helpers.Color(255, 255, 255, 255), false, Widget.TextAnchor.Left, false);

        Widget.Text(totalDMG, new Vector2(WindowMax.X - Widget.CalcTextSize(dps).X - Widget.CalcTextSize(totalDMG).X - 5, rowPosition + (lineHeight / 2 - (Widget.CalcTextSize(totalDMG).Y) / 2)), Helpers.Color(210, 210, 210, 255), false, Widget.TextAnchor.Right, false);

        Widget.Text(dps, new Vector2(WindowMax.X - Widget.CalcTextSize(dps).X - 5, textRowPosition), Helpers.Color(255, 255, 255, 255), false, Widget.TextAnchor.Right, false);
    }

    private void DrawProgressBar(Vector2 startPoint, Vector2 endPoint, uint color, float progress)
    {
        uint blackColor = Helpers.Color(0, 0, 0, 255);
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(startPoint, new Vector2(startPoint.X + (endPoint.X - startPoint.X) * progress, endPoint.Y), blackColor, color, color, blackColor);
    }

    private void DrawBorder(Vector2 startPoint, Vector2 endPoint, uint color)
    {
        ImGui.GetWindowDrawList().AddRect(startPoint, endPoint, color);
    }
}
