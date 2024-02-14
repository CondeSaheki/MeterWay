using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;

namespace Meterway.Windows;

public class OverlayWindow : Window, IDisposable
{
    private Plugin plugin;

    public const ImGuiWindowFlags defaultflags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground;

    public OverlayWindow(Plugin plugin) : base("OverlayWindow")
    {
        this.plugin = plugin;
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(80, 45),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.IsOpen = true;
        this.RespectCloseHotkey = false;
        this.Flags = Gerateflags();
    }
    public ImGuiWindowFlags Gerateflags()
    {
        var flags = defaultflags;
        flags |= this.plugin.Configuration.OverlayClickThrough ? ImGuiWindowFlags.NoInputs : ImGuiWindowFlags.None;
        //flags |= !this.plugin.Configuration.OverlayBackground ? ImGuiWindowFlags.NoBackground : ImGuiWindowFlags.None;
        return flags;
    }

    class TempCombatData
    {
        public float DPS { get; set; }
        public float PctDMG { get; set; }
        public float TotalDMG { get; set; }
        public float Position { get; set; }
    }

    public void Dispose() { }

    private float transitionDuration = 1f; // in seconds
    private float transitionTimer = 0.0f;

    private Dictionary<String, TempCombatData> formerInfo = new Dictionary<string, TempCombatData>();

    private Dictionary<String, TempCombatData> targetInfo = new Dictionary<string, TempCombatData>();


    public override void Draw()
    {
        ImGui.SetWindowFontScale(this.plugin.Configuration.OverlayFontSize);

        // draw background
        if (this.plugin.Configuration.OverlayBackground)
        {
            Background(this.plugin.Configuration.OverlayBackgroundColor);
        }

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

    float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat + (secondFloat - firstFloat) * by;
    }

    void DrawCombatantLine(TempCombatData data, string name, string job)
    {
        float barHeight = ImGui.GetFontSize() + 5;
        //Draw a progress bar using the percentage from the combatant data, show the info overlapping the bar.
        var windowMin = ImGui.GetCursorScreenPos();
        var windowMax = new Vector2(ImGui.GetWindowSize().X + windowMin.X - 32, windowMin.Y + 32);
        var rowPosition = windowMin.Y + (barHeight * (data.Position - 1)); // Modify the row position calculation

        ImGui.GetWindowDrawList().AddRectFilled(new Vector2(windowMin.X, rowPosition), new Vector2(windowMax.X, rowPosition + barHeight), Color(26, 26, 26, 190));
        DrawProgressBar(new Vector2(windowMin.X, rowPosition), new Vector2(windowMax.X, rowPosition + barHeight), name == "YOU" ? Color(128, 170, 128, 255) : Color(128, 128, 170, 255), data.PctDMG / 100.0f);
        DrawBorder(new Vector2(windowMin.X, rowPosition), new Vector2(windowMax.X, rowPosition + barHeight), Color(26, 26, 26, 222));
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMin.X + 5, rowPosition), Color(172, 172, 172, 255), job.ToUpper());
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMin.X + 35, rowPosition), Color(255, 255, 255, 255), name);
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMax.X - ImGui.CalcTextSize(data.DPS.ToString("0") + "/s").X - 5, rowPosition), Color(255, 255, 255, 255), data.DPS.ToString("0") + "/s");
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

    public void Background(Vector4 color)
    {
        Vector2 vMin = ImGui.GetWindowContentRegionMin();
        Vector2 vMax = ImGui.GetWindowContentRegionMax();

        vMin.X += ImGui.GetWindowPos().X;
        vMin.Y += ImGui.GetWindowPos().Y;
        vMax.X += ImGui.GetWindowPos().X;
        vMax.Y += ImGui.GetWindowPos().Y;

        ImGui.GetWindowDrawList().AddRectFilled(vMin, vMax, Color(color));
    }

    public static uint Color(byte r, byte g, byte b, byte a) { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }

    public static uint Color(Vector4 color)
    {
        return Color(Convert.ToByte(Math.Min(Math.Round(color.X * 255), 255)), Convert.ToByte(Math.Min(Math.Round(color.Y * 255), 255)),
            Convert.ToByte(Math.Min(Math.Round(color.Z * 255), 255)), Convert.ToByte(Math.Round(color.W * 255)));
    }
}
