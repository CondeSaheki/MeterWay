using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using Meterway.Utils;

namespace Meterway.Overlays;

public class MaotOverlay : IMeterwayOverlay
{
    public string Name => "MaotOverlay";
    private Plugin plugin { get; init; }

    public MaotOverlay(Plugin plugin)
    {
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

    // Draw

    public void Draw()
    {
        if (plugin.dataManager.Combat != null && plugin.dataManager.Combat.Count() != 0)
        {
            ImGui.Text("In combat for " + plugin.dataManager.Combat.Last().encounter.Duration.ToString() + " seconds.");

            if (plugin.dataManager.Combat.Last().combatants != null && plugin.dataManager.Combat.Last().combatants.Count() != 0)
            {
                foreach (Combatant combatant in plugin.dataManager.Combat.Last().combatants)
                {
                    if (!formerInfo.ContainsKey(combatant.Name))
                    {
                        formerInfo[combatant.Name] = new TempCombatData { DPS = combatant.EncDps, PctDMG = combatant.DmgPct, TotalDMG = combatant.Dmg, Position = plugin.dataManager.Combat.Last().combatants.IndexOf(combatant) + 1 };
                        targetInfo[combatant.Name] = new TempCombatData { DPS = combatant.EncDps, PctDMG = combatant.DmgPct, TotalDMG = combatant.Dmg, Position = plugin.dataManager.Combat.Last().combatants.IndexOf(combatant) + 1 };
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
                        targetInfo[combatant.Name].Position = plugin.dataManager.Combat.Last().combatants.IndexOf(combatant) + 1;
                        transitionTimer = 0.0f;
                    }

                    DrawCombatantLine(formerInfo[combatant.Name], combatant.Name, combatant.Job);
                }
            }
        }
        else
        {
            ImGui.Text("Not in combat...");
        }
    }

    public void Dispose()
    {
    }

    // Draw helpers

    private void DrawCombatantLine(TempCombatData data, string name, string job)
    {
        float barHeight = ImGui.GetFontSize() + 5;
        var windowMin = ImGui.GetCursorScreenPos();
        var windowMax = new Vector2(ImGui.GetWindowSize().X + windowMin.X - 32, windowMin.Y + 32);
        var rowPosition = windowMin.Y + (barHeight * (data.Position - 1));

        var textRowPosition = rowPosition + barHeight / 2 - ImGui.GetFontSize() / 2;
        var totalDPSStr = $"{Helpers.HumanizeNumber(data.DPS, 1)}/s";
        var totalDPMStr = $"{Helpers.HumanizeNumber(data.TotalDMG, 1)}";

        var drawList = ImGui.GetWindowDrawList();
        DrawJobIcon(new Vector2(windowMin.X, rowPosition), barHeight, job);
        windowMin.X += barHeight;

        drawList.AddRectFilled(new Vector2(windowMin.X, rowPosition), new Vector2(windowMax.X, rowPosition + barHeight), Helpers.Color(26, 26, 26, 190));
        DrawProgressBar(new Vector2(windowMin.X, rowPosition), new Vector2(windowMax.X, rowPosition + barHeight), name == "YOU" ? Helpers.Color(128, 170, 128, 255) : Helpers.Color(128, 128, 170, 255), data.PctDMG / 100.0f);
        DrawBorder(new Vector2(windowMin.X, rowPosition), new Vector2(windowMax.X, rowPosition + barHeight), Helpers.Color(26, 26, 26, 222));

        // scale down the font for the job and center it in the line
        ImGui.SetWindowFontScale(0.8f * this.plugin.Configuration.OverlayFontSize);
        drawList.AddText(new Vector2(windowMin.X + 7, rowPosition + (barHeight / 2 - (CalcTextSize(job.ToUpper()).Y) / 2)), Helpers.Color(172, 172, 172, 255), job.ToUpper());
        ImGui.SetWindowFontScale(this.plugin.Configuration.OverlayFontSize);

        // Draw shadow for name
        drawList.AddText(new Vector2(windowMin.X + 10 + (0.8f * CalcTextSize(job.ToUpper()).X), textRowPosition + 1), Helpers.Color(0, 0, 0, 255), name);
        // Draw name
        drawList.AddText(new Vector2(windowMin.X + 10 + (0.8f * CalcTextSize(job.ToUpper()).X), textRowPosition), Helpers.Color(255, 255, 255, 255), name);

        ImGui.SetWindowFontScale(0.8f * this.plugin.Configuration.OverlayFontSize);
        //shadow for total damage
        drawList.AddText(new Vector2(windowMax.X - (CalcTextSize(totalDPSStr).X * (1 / 0.8f)) - CalcTextSize(totalDPMStr).X - 5, textRowPosition + 1), Helpers.Color(0, 0, 0, 150), totalDPMStr);
        drawList.AddText(new Vector2(windowMax.X - (CalcTextSize(totalDPSStr).X * (1 / 0.8f)) - CalcTextSize(totalDPMStr).X - 5, textRowPosition), Helpers.Color(210, 210, 210, 255), totalDPMStr);
        ImGui.SetWindowFontScale(this.plugin.Configuration.OverlayFontSize);

        // Draw shadow for DPS
        drawList.AddText(new Vector2(windowMax.X - CalcTextSize(totalDPSStr).X - 5, textRowPosition + 1), Helpers.Color(0, 0, 0, 255), totalDPSStr);
        // Draw DPS
        drawList.AddText(new Vector2(windowMax.X - CalcTextSize(totalDPSStr).X - 5, textRowPosition), Helpers.Color(255, 255, 255, 255), totalDPSStr);
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
            width += ImGui.GetFont().GetCharAdvance(c) * height * ImGui.GetIO().FontGlobalScale;
            this.plugin.PluginLog.Info("Char: " + c + " Width: " + width);
        }


        return new Vector2(width, height);
    }

}