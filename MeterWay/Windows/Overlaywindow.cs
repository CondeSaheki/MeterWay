using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

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

    public override void Draw()
    {
        // draw background
        if (this.plugin.Configuration.OverlayBackground)
        {
            Backgound(this.plugin.Configuration.OverlayBackgroundColor);
        }

        if (plugin.dataManager.Combat != null && plugin.dataManager.Combat.Count() != 0)
        {
            ImGui.Text("duration: " + plugin.dataManager.Combat.Last().encounter.Duration.ToString());

            if (plugin.dataManager.Combat.Last().combatants != null && plugin.dataManager.Combat.Last().combatants.Count() != 0)
            {
                foreach (Combatant combatant in plugin.dataManager.Combat.Last().combatants)
                {
                    ImGui.Text(combatant.Job + " ");
                    ImGui.SameLine();
                    ImGui.Text(combatant.Name + " ");
                    ImGui.SameLine();
                    ImGui.Text(combatant.EncDps + " ");
                    ImGui.SameLine();
                    ImGui.Text(combatant.DmgPct + "%");
                    //DrawProgressBarWithText(32, Color(255, 128, 128, 255), combatant.DmgPct / 100.0f, "");
                }
            }

        }
        else
        {
            ImGui.Text("empty");
        }

    }

    public void Backgound(Vector4 color)
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
        return Color(Convert.ToByte(Math.Round(color.X * 255)), Convert.ToByte(Math.Round(color.Y * 255)), 
            Convert.ToByte(Math.Round(color.Z * 255)), Convert.ToByte(Math.Round(color.W * 255)));
    }
}
