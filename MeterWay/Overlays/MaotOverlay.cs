using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;
using Dalamud.Game.Gui.PartyFinder.Types;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Globalization;

namespace Meterway.Overlays;

public class MaotOverlay : IOverlay
{
    // OverlayWindow

    public string Name => "Maot Overlay";
    private Plugin plugin { get; init; }

    public MaotOverlay(Plugin plugin) : base(plugin)
    {
        
        this.plugin = plugin;
    }
    
    // MaotOverlay
    
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

    // fn

    private float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat + (secondFloat - firstFloat) * by;
    }

    private string HumanizeNumber(float number, int decimals = 1)
    {
        string[] suffixes = { "", "k", "M", "B", "T" };
        int suffixIndex = 0;

        while (number >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            number /= 1000;
            suffixIndex++;
        }
        // return with no decimais if below 1k
        if (suffixIndex == 0)
        {
            return $"{number.ToString("0")}";
        }

        // use . for decimals
        return $"{number.ToString($"0.{new string('0', decimals)}", CultureInfo.InvariantCulture)}{suffixes[suffixIndex]}";
    }

    public static uint Color(byte r, byte g, byte b, byte a)
    {
        uint ret = a;
        ret <<= 8;
        ret += b;
        ret <<= 8;
        ret += g;
        ret <<= 8;
        ret += r;
        return ret;
    }

    public static uint Color(Vector4 color)
    {
        return Color(Convert.ToByte(Math.Min(Math.Round(color.X * 255), 255)), Convert.ToByte(Math.Min(Math.Round(color.Y * 255), 255)),
            Convert.ToByte(Math.Min(Math.Round(color.Z * 255), 255)), Convert.ToByte(Math.Round(color.W * 255)));
    }

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
                        formerInfo[combatant.Name].DPS = Lerp(formerInfo[combatant.Name].DPS, targetInfo[combatant.Name].DPS, t);
                        formerInfo[combatant.Name].PctDMG = Lerp(formerInfo[combatant.Name].PctDMG, targetInfo[combatant.Name].PctDMG, t);
                        formerInfo[combatant.Name].TotalDMG = Lerp(formerInfo[combatant.Name].TotalDMG, targetInfo[combatant.Name].TotalDMG, t);
                        formerInfo[combatant.Name].Position = Lerp(formerInfo[combatant.Name].Position, targetInfo[combatant.Name].Position, t);
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
    
    public void Dispose() { }

    // Draw helpers

    private void DrawCombatantLine(TempCombatData data, string name, string job)
    {
        float barHeight = ImGui.GetFontSize() + 5;
        var windowMin = ImGui.GetCursorScreenPos();
        var windowMax = new Vector2(ImGui.GetWindowSize().X + windowMin.X - 32, windowMin.Y + 32);
        var rowPosition = windowMin.Y + (barHeight * (data.Position - 1));

        var textRowPosition = rowPosition + barHeight / 2 - ImGui.GetFontSize() / 2;
        var totalDPSStr = $"{HumanizeNumber(data.DPS, 1)}/s";
        var totalDPMStr = $"{HumanizeNumber(data.TotalDMG, 1)}";
        DrawJobIcon(new Vector2(windowMin.X, rowPosition), barHeight, job);
        windowMin.X += barHeight;

        ImGui.GetWindowDrawList().AddRectFilled(new Vector2(windowMin.X, rowPosition), new Vector2(windowMax.X, rowPosition + barHeight), Color(26, 26, 26, 190));
        DrawProgressBar(new Vector2(windowMin.X, rowPosition), new Vector2(windowMax.X, rowPosition + barHeight), name == "YOU" ? Color(128, 170, 128, 255) : Color(128, 128, 170, 255), data.PctDMG / 100.0f);
        DrawBorder(new Vector2(windowMin.X, rowPosition), new Vector2(windowMax.X, rowPosition + barHeight), Color(26, 26, 26, 222));

        // scale down the font for the job and center it in the line
        ImGui.SetWindowFontScale(0.8f * this.plugin.Configuration.OverlayFontSize);
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMin.X + 7, rowPosition + (barHeight / 2 - (ImGui.CalcTextSize(job.ToUpper()).Y) / 2)), Color(172, 172, 172, 255), job.ToUpper());
        ImGui.SetWindowFontScale(this.plugin.Configuration.OverlayFontSize);

        // Draw shadow for name
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMin.X + 10 + (0.8f * ImGui.CalcTextSize(job.ToUpper()).X), textRowPosition + 1), Color(0, 0, 0, 255), name);
        // Draw name
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMin.X + 10 + (0.8f * ImGui.CalcTextSize(job.ToUpper()).X), textRowPosition), Color(255, 255, 255, 255), name);

        ImGui.SetWindowFontScale(0.8f * this.plugin.Configuration.OverlayFontSize);
        //shadow for total damage
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMax.X - (ImGui.CalcTextSize(totalDPSStr).X * (1 / 0.8f)) - ImGui.CalcTextSize(totalDPMStr).X - 5, textRowPosition + 1), Color(0, 0, 0, 150), totalDPMStr);
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMax.X - (ImGui.CalcTextSize(totalDPSStr).X * (1 / 0.8f)) - ImGui.CalcTextSize(totalDPMStr).X - 5, textRowPosition), Color(210, 210, 210, 255), totalDPMStr);
        ImGui.SetWindowFontScale(this.plugin.Configuration.OverlayFontSize);

        // Draw shadow for DPS
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMax.X - ImGui.CalcTextSize(totalDPSStr).X - 5, textRowPosition + 1), Color(0, 0, 0, 255), totalDPSStr);
        // Draw DPS
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMax.X - ImGui.CalcTextSize(totalDPSStr).X - 5, textRowPosition), Color(255, 255, 255, 255), totalDPSStr);
    }

    private void DrawProgressBar(Vector2 pmin, Vector2 pmax, uint color, float progress)
    {
        uint blackColor = Color(0, 0, 0, 255);
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
            ImGui.GetWindowDrawList().AddRectFilled(position, new Vector2(position.X + size, position.Y + size), Color(26, 26, 39, 190));
            ImGui.GetWindowDrawList().AddRect(position, new Vector2(position.X + size, position.Y + size), Color(160, 160, 160, 190));
            ImGui.GetWindowDrawList().AddImage(jobIcon.ImGuiHandle, position, new Vector2(position.X + size, position.Y + size), new Vector2(0, 0), new Vector2(1, 1), Color(255, 255, 255, 255));
        }
    }

}