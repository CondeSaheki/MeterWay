using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using Meterway.Utils;
using Dalamud.Interface.Windowing;
using System.ComponentModel;

namespace Meterway.Overlays;

public class LazerOverlay : IMeterwayOverlay
{
    public string Name => "LazerOverlay";
    private Plugin plugin { get; init; }

    private Vector2 WindowMin { get; set; }
    private Vector2 WindowMax { get; set; }

    public LazerOverlay(Plugin plugin)
    {
        this.WindowMin = new Vector2();
        this.WindowMax = new Vector2();
        this.plugin = plugin;
    }

    class TempCombatData
    {
        public float DPS { get; set; }
        public float PctDMG { get; set; }
        public float TotalDMG { get; set; }
        public float Position { get; set; }
    }

    private float transitionDuration = 1f; // in seconds
    private float transitionTimer = 0.0f;
    private Dictionary<String, TempCombatData> formerInfo = new Dictionary<string, TempCombatData>();
    private Dictionary<String, TempCombatData> targetInfo = new Dictionary<string, TempCombatData>();

    // For now we hardcode the jobId's, but we should probably get them from the game
    private static Dictionary<string, uint> JobIds = new Dictionary<string, uint>()
    {
        { "ADV", 0 },
        { "GLA", 1 },
        { "PGL", 2 },
        { "MRD", 3 },
        { "LNC", 4 },
        { "ARC", 5 },
        { "CNJ", 6 },
        { "THM", 7 },
        { "CRP", 8 },
        { "BSM", 9 },
        { "ARM", 10 },
        { "GSM", 11 },
        { "LTW", 12 },
        { "WVR", 13 },
        { "ALC", 14 },
        { "CUL", 15 },
        { "MIN", 16 },
        { "BTN", 17 },
        { "FSH", 18 },
        { "PLD", 19 },
        { "MNK", 20 },
        { "WAR", 21 },
        { "DRG", 22 },
        { "BRD", 23 },
        { "WHM", 24 },
        { "BLM", 25 },
        { "ACN", 26 },
        { "SMN", 27 },
        { "SCH", 28 },
        { "ROG", 29 },
        { "NIN", 30 },
        { "MCH", 31 },
        { "DRK", 32 },
        { "AST", 33 },
        { "SAM", 34 },
        { "RDM", 35 },
        { "BLU", 36 },
        { "GNB", 37 },
        { "DNC", 38 },
        { "RPR", 39 },
        { "SGE", 40 }
    };

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

    // Draw

    public void Draw()
    {

        UpdateWindowSize();

        if (plugin.dataManager.Combat == null || plugin.dataManager.Combat.Count() == 0)
        {
            ImGui.GetWindowDrawList().AddText(new Vector2(WindowMin.X + (WindowMax.X - WindowMin.X) / 2 - (CalcTextSize("Not in combat...").X) / 2, WindowMin.Y + 2), Helpers.Color(255, 255, 255, 255), "Not in combat...");
            return;
        }

        if (plugin.dataManager.Combat.Last().combatants == null || plugin.dataManager.Combat.Last().combatants.Count() == 0)
        {
            return;
        }

        var currentCombat = plugin.dataManager.Combat.Last();

        // Add a background for the title and centralize the text
        ImGui.GetWindowDrawList().AddRectFilled(new Vector2(WindowMin.X, WindowMin.Y), new Vector2(WindowMax.X, WindowMin.Y + ImGui.GetFontSize() + 5), Helpers.Color(26, 26, 39, 190));

        var center = new Vector2(WindowMin.X + (WindowMax.X - WindowMin.X) / 2 - (CalcTextSize(currentCombat.encounter.Name).X) / 2, WindowMin.Y + 2);
        ImGui.GetWindowDrawList().AddText(center, Helpers.Color(255, 255, 255, 255), currentCombat.encounter.Name);
        foreach (Combatant combatant in currentCombat.combatants)
        {
            if (!formerInfo.ContainsKey(combatant.Name))
            {
                formerInfo[combatant.Name] = new TempCombatData { DPS = combatant.EncDps, PctDMG = combatant.DmgPct, TotalDMG = combatant.Dmg, Position = currentCombat.combatants.IndexOf(combatant) + 1 };
                targetInfo[combatant.Name] = new TempCombatData { DPS = combatant.EncDps, PctDMG = combatant.DmgPct, TotalDMG = combatant.Dmg, Position = currentCombat.combatants.IndexOf(combatant) + 1 };
            }

            if (transitionTimer < transitionDuration)
            {
                transitionTimer += ImGui.GetIO().DeltaTime;
                float t = Math.Min(1.0f, transitionTimer / transitionDuration);
                formerInfo[combatant.Name].DPS = Helpers.Lerp(formerInfo[combatant.Name].DPS, targetInfo[combatant.Name].DPS, t);
                formerInfo[combatant.Name].PctDMG = Helpers.Lerp(formerInfo[combatant.Name].PctDMG, targetInfo[combatant.Name].PctDMG, t);
                formerInfo[combatant.Name].TotalDMG = Helpers.Lerp(formerInfo[combatant.Name].TotalDMG, targetInfo[combatant.Name].TotalDMG, t);
                formerInfo[combatant.Name].Position = Helpers.Lerp(formerInfo[combatant.Name].Position, targetInfo[combatant.Name].Position, t);
            }

            if (targetInfo[combatant.Name].DPS != combatant.EncDps)
            {
                targetInfo[combatant.Name].DPS = combatant.EncDps;
                targetInfo[combatant.Name].PctDMG = combatant.DmgPct;
                targetInfo[combatant.Name].TotalDMG = combatant.Dmg;
                targetInfo[combatant.Name].Position = currentCombat.combatants.IndexOf(combatant) + 1;
                transitionTimer = 0.0f;
            }

            DrawCombatantLine(formerInfo[combatant.Name], combatant.Name, combatant.Job);


        }
    }

    public void Dispose()
    {
    }

    // Draw helpers

    private void DrawCombatantLine(TempCombatData data, string name, string job)
    {
        float lineHeight = ImGui.GetFontSize() + 5;
        var windowMin = new Vector2(WindowMin.X, WindowMin.Y + lineHeight);
        var rowPosition = windowMin.Y + (lineHeight * (data.Position - 1));

        var textRowPosition = rowPosition + lineHeight / 2 - ImGui.GetFontSize() / 2;
        var totalDPSStr = $"{Helpers.HumanizeNumber(data.DPS, 1)}/s";
        var totalDPMStr = $"{Helpers.HumanizeNumber(data.TotalDMG, 1)}";

        var drawList = ImGui.GetWindowDrawList();
        DrawJobIcon(new Vector2(windowMin.X, rowPosition), lineHeight, job);
        windowMin.X += lineHeight;

        drawList.AddRectFilled(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), Helpers.Color(26, 26, 26, 190));
        DrawProgressBar(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), name == "YOU" ? Helpers.Color(128, 170, 128, 255) : Helpers.Color(128, 128, 170, 255), data.PctDMG / 100.0f);
        DrawBorder(new Vector2(windowMin.X, rowPosition), new Vector2(WindowMax.X, rowPosition + lineHeight), Helpers.Color(26, 26, 26, 222));

        // scale down the font for the job and center it in the line
        ImGui.SetWindowFontScale(0.8f * this.plugin.Configuration.OverlayFontSize);
        drawList.AddText(new Vector2(windowMin.X + 7, rowPosition + (lineHeight / 2 - (CalcTextSize(job.ToUpper()).Y) / 2)), Helpers.Color(172, 172, 172, 255), job.ToUpper());
        ImGui.SetWindowFontScale(this.plugin.Configuration.OverlayFontSize);

        // Draw shadow for name
        drawList.AddText(new Vector2(windowMin.X + 10 + (0.8f * CalcTextSize(job.ToUpper()).X), textRowPosition + 1), Helpers.Color(0, 0, 0, 255), name);
        // Draw name
        drawList.AddText(new Vector2(windowMin.X + 10 + (0.8f * CalcTextSize(job.ToUpper()).X), textRowPosition), Helpers.Color(255, 255, 255, 255), name);

        ImGui.SetWindowFontScale(0.8f * this.plugin.Configuration.OverlayFontSize);
        //shadow for total damage
        drawList.AddText(new Vector2(WindowMax.X - CalcTextSize(totalDPSStr).X - CalcTextSize(totalDPMStr).X - 5, rowPosition + (lineHeight / 2 - (CalcTextSize(totalDPMStr).Y) / 2) + 1), Helpers.Color(0, 0, 0, 150), totalDPMStr);
        drawList.AddText(new Vector2(WindowMax.X - CalcTextSize(totalDPSStr).X - CalcTextSize(totalDPMStr).X - 5, rowPosition + (lineHeight / 2 - (CalcTextSize(totalDPMStr).Y) / 2)), Helpers.Color(210, 210, 210, 255), totalDPMStr);
        ImGui.SetWindowFontScale(this.plugin.Configuration.OverlayFontSize);

        // Draw shadow for DPS
        drawList.AddText(new Vector2(WindowMax.X - CalcTextSize(totalDPSStr).X - 5, textRowPosition + 1), Helpers.Color(0, 0, 0, 255), totalDPSStr);
        // Draw DPS
        drawList.AddText(new Vector2(WindowMax.X - CalcTextSize(totalDPSStr).X - 5, textRowPosition), Helpers.Color(255, 255, 255, 255), totalDPSStr);
    }



    // Draw helpers
    private void DrawProgressBar(Vector2 pmin, Vector2 pmax, uint color, float progress)
    {
        uint blackColor = Helpers.Color(0, 0, 0, 255);
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(pmin, new Vector2(pmin.X + (pmax.X - pmin.X) * progress, pmax.Y), blackColor, color, color, blackColor);
    }
    private void DrawBorder(Vector2 windowmin, Vector2 windowmax, uint color)
    {
        ImGui.GetWindowDrawList().AddRect(windowmin, windowmax, color);
    }

    private void DrawJobIcon(Vector2 position, float size, string job)
    {
        uint jobIconId = 62000u + JobIds[job.ToUpper()];
        var jobIcon = this.plugin.TextureProvider.GetIcon(jobIconId, Dalamud.Plugin.Services.ITextureProvider.IconFlags.None);
        if (jobIcon != null)
        {
            // Draw a background for the icon
            ImGui.GetWindowDrawList().AddRectFilled(position, new Vector2(position.X + size, position.Y + size), Helpers.Color(26, 26, 39, 190));
            ImGui.GetWindowDrawList().AddRect(position, new Vector2(position.X + size, position.Y + size), Helpers.Color(160, 160, 160, 190));
            ImGui.GetWindowDrawList().AddImage(jobIcon.ImGuiHandle, position, new Vector2(position.X + size, position.Y + size), new Vector2(0, 0), new Vector2(1, 1), Helpers.Color(255, 255, 255, 255));
        }
    }

    // Implement CalcTextSize() manually;
    private Vector2 CalcTextSize(string text)
    {
        float height = ImGui.GetFontSize();
        float width = 0;

        foreach (char c in text)
        {
            width += ImGui.GetFont().GetCharAdvance(c) * ImGui.GetIO().FontGlobalScale;
        }
        return new Vector2(width, height);
    }

}