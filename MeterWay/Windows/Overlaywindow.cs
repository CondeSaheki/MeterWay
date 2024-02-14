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

    public void Dispose() { }

    private float transitionDuration = 1f; // in seconds
    private float transitionTimer = 0.0f;

    private Dictionary<String, float> formerDPS = new Dictionary<string, float>();

    private Dictionary<String, float> targetDPS = new Dictionary<string, float>();


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
                    if (!formerDPS.ContainsKey(combatant.Name))
                    {
                        formerDPS[combatant.Name] = combatant.EncDps;
                        targetDPS[combatant.Name] = combatant.EncDps;
                    }

                    if (transitionTimer < transitionDuration)
                    {
                        transitionTimer += ImGui.GetIO().DeltaTime;
                        float t = Math.Min(1.0f, transitionTimer / transitionDuration);
                        formerDPS[combatant.Name] = Lerp(formerDPS[combatant.Name], targetDPS[combatant.Name], t);
                        this.plugin.PluginLog.Info($"Transtition Time: {transitionTimer}, {formerDPS[combatant.Name]}");
                    }

                    if (targetDPS[combatant.Name] != combatant.EncDps)
                    {
                        targetDPS[combatant.Name] = combatant.EncDps;
                        transitionTimer = 0.0f;
                    }

                    ImGui.Text($"[{combatant.Job.ToUpper()}] {combatant.Name} {formerDPS[combatant.Name].ToString("0")}/s");
                }
            }
        }
        else
        {
            ImGui.Text("empty");
        }
    }

    float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat + (secondFloat - firstFloat) * by;
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
