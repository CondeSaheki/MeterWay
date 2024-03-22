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

    public DebugWindow() : base("MeterWay DebugWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(160, 90),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        var draw = ImGui.GetWindowDrawList();
        var textheight = ImGui.GetFontSize();
        var white = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
        (Vector2 Min, Vector2 Max) window = GetWindow();
        Vector2 cursor = window.Min;

        DrawIndexArrows();
        cursor.Y += 25;

        var data = EncounterManager.Inst.encounters[indexEncounter];
        if (data.Active) data.Calculate();

        // encounter

        string text = $"Encounter:";
        draw.AddText(cursor, white, text);
        cursor.Y += ImGui.CalcTextSize(text).Y;

        text = $"Name={data.Name} Active={(data.Active ? "true" : "false")} Finished={(data.Finished ? "true" : "false")} Duration={data.Duration}";
        draw.AddText(cursor, white, text);
        cursor.Y += ImGui.CalcTextSize(text).Y;

        text = $"DamageDealt:\n{data.DamageDealt}";
        draw.AddText(cursor, white, text);
        cursor.Y += ImGui.CalcTextSize(text).Y;

        text = $"DamageReceived:\n{data.DamageReceived}";
        draw.AddText(cursor, white, text);
        cursor.Y += ImGui.CalcTextSize(text).Y;

        text = $"HealingDealt:\n{data.HealDealt}";
        draw.AddText(cursor, white, text);
        cursor.Y += ImGui.CalcTextSize(text).Y;

        text = $"HealingReceived:\n{data.HealReceived}";
        draw.AddText(cursor, white, text);
        cursor.Y += ImGui.CalcTextSize(text).Y;

        text = $"> All other Calculated data should be here <";
        draw.AddText(cursor, white, text);
        cursor.Y += ImGui.CalcTextSize(text).Y;
        cursor.Y += textheight;

        text = $"Players:";
        draw.AddText(cursor, white, text);
        cursor.Y += ImGui.CalcTextSize(text).Y;
        // players
        foreach (var p in data.Players)
        {
            var player = p.Value;

            text = $"Name={player.Name} World={player.World} Job={player.Job} Id={player.Id} IsActive={(player.IsActive ? "true" : "false")}";
            draw.AddText(cursor, white, text);
            cursor.Y += ImGui.CalcTextSize(text).Y;

            text = $"DamageDealt:\n{player.DamageDealt}";
            draw.AddText(cursor, white, text);
            cursor.Y += ImGui.CalcTextSize(text).Y;

            text = $"DamageReceived:\n{player.DamageReceived}";
            draw.AddText(cursor, white, text);
            cursor.Y += ImGui.CalcTextSize(text).Y;

            text = $"HealingDealt:\n{player.HealDealt}";
            draw.AddText(cursor, white, text);
            cursor.Y += ImGui.CalcTextSize(text).Y;

            text = $"HealingReceived:\n{player.HealReceived}";
            draw.AddText(cursor, white, text);
            cursor.Y += ImGui.CalcTextSize(text).Y;

            text = $"> All other Calculated data should be here <";
            draw.AddText(cursor, white, text);
            cursor.Y += ImGui.CalcTextSize(text).Y;
            cursor.Y += textheight;
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