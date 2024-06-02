#if DEBUG

using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using MeterWay.Managers;

namespace MeterWay.Windows;

public class DebugWindow : Window, IDisposable
{
    private static int indexEncounter = 0;

    public DebugWindow() : base("MeterWay DebugWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(640, 360),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        DrawIndexArrows();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var data = EncounterManager.Inst.encounters[indexEncounter];
        if (data.Active) data.Calculate();

        string text = string.Empty;
        // encounter
        if (ImGui.CollapsingHeader("Encounter"))
        {
            ImGui.Text($"Name={data.Name} Active={(data.Active ? "true" : "false")} Finished={(data.Finished ? "true" : "false")} Duration={data.Duration}");
            ImGui.Text($"DamageDealt:\n{data.DamageDealt}");
            ImGui.Text($"DamageReceived:\n{data.DamageReceived}");
            ImGui.Text($"HealingDealt:\n{data.HealDealt}");
            ImGui.Text($"HealingReceived:\n{data.HealReceived}");
            ImGui.Text($"DataPerSeconds:\n{data.PerSeconds}");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        text = $"Players:";
        ImGui.Text(text);
        ImGui.Spacing();
        // players
        foreach (var p in data.Players)
        {
            var player = p.Value;
            if (ImGui.CollapsingHeader(player.Name))
            {
                ImGui.Text($"Name={player.Name} World={player.World} Job={player.Job} Id={player.Id} IsActive={(player.IsActive ? "true" : "false")}");
                ImGui.Text($"DamageDealt:\n{player.DamageDealt}");
                ImGui.Text($"DamageReceived:\n{player.DamageReceived}");
                ImGui.Text($"HealingDealt:\n{player.HealDealt}");
                ImGui.Text($"HealingReceived:\n{player.HealReceived}");
                ImGui.Text($"DataPerSeconds:\n{player.PerSeconds}");
            }
        }
    }

    public void Dispose() { }

    private static (Vector2 Min, Vector2 Max) GetWindow()
    {
        Vector2 min = ImGui.GetWindowContentRegionMin();
        Vector2 max = ImGui.GetWindowContentRegionMax();

        min.X += ImGui.GetWindowPos().X;
        min.Y += ImGui.GetWindowPos().Y;
        max.X += ImGui.GetWindowPos().X;
        max.Y += ImGui.GetWindowPos().Y;
        return (min, max);
    }

    private static void DrawIndexArrows()
    {
        var size = EncounterManager.Inst.encounters.Count - 1;
        if (size < indexEncounter) indexEncounter = size;

        float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
        ImGui.PushButtonRepeat(true);
        if (ImGui.ArrowButton("##left", ImGuiDir.Left))
        {
            if (indexEncounter != 0) indexEncounter--;
        }
        ImGui.SameLine(0.0f, spacing);
        if (ImGui.ArrowButton("##right", ImGuiDir.Right))
        {
            if (size != indexEncounter) indexEncounter++;
        }
        ImGui.PopButtonRepeat();
        ImGui.SameLine();
        ImGui.Text($"index {indexEncounter} | size {size}");
    }
}

#endif