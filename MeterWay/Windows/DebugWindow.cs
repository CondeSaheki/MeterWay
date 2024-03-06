#if DEBUG

using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using MeterWay.Managers;
using MeterWay.Utils;

namespace MeterWay.Windows;

public class DebugWindow : Window, IDisposable
{
    private Vector2 WindowMin { get; set; }
    private Vector2 WindowMax { get; set; }

    private static int indexEncounter = 0;

    public DebugWindow() : base("MeterWay DebugWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(160, 90),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        WindowMin = new Vector2();
        WindowMax = new Vector2();
    }

    public override void Draw()
    {
        UpdateWindowSize();
        var draw = ImGui.GetWindowDrawList();
        var textheight = (float)Math.Ceiling(ImGui.GetFontSize());
        Vector2 cursor = WindowMin;

        IndexArrows();
        cursor.Y += 25;

        var data = EncounterManager.Inst.encounters[indexEncounter];
        if (data.Active) data.Calculate();

        // encounter

        string text = $"Encounter:";
        draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
        cursor.Y += textheight * 2;

        text = $"Name={data.Name} Active={(data.Active ? "true" : "false")} Finished={(data.Finished ? "true" : "false")} Duration={data.Duration}";
        draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
        cursor.Y += textheight * 2;

        text = $"DamageDealt:\n{data.DamageDealt}";
        draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
        cursor.Y += textheight * 2;

        text = $"DamageReceived:\n{data.DamageReceived}";
        draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
        cursor.Y += textheight * 2;

        text = $"HealingDealt:\n{data.HealDealt}";
        draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
        cursor.Y += textheight * 2;

        text = $"HealingReceived:\n{data.HealReceived}";
        draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
        cursor.Y += textheight * 2;

        text = $"> All other Calculated data should be here <";
        draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
        cursor.Y += textheight * 2;

        text = $"Players:";
        draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
        cursor.Y += textheight * 2;
        // players
        foreach (var p in data.Players)
        {
            var player = p.Value;

            text = $"Name={player.Name} World={player.World} Job={player.Job} Id={player.Id} IsActive={(player.IsActive ? "true" : "false")}";
            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight * 2;

            text = $"DamageDealt:\n{player.DamageDealt}";
            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight * 2;

            text = $"DamageReceived:\n{player.DamageReceived}";
            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight * 2;

            text = $"HealingDealt:\n{player.HealDealt}";
            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight * 2;

            text = $"HealingReceived:\n{player.HealReceived}";
            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight * 2;

            text = $"> All other Calculated data should be here <";
            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight * 2;
        }
    }

    public void Dispose() { }

    private void UpdateWindowSize()
    {
        Vector2 vMin = ImGui.GetWindowContentRegionMin();
        Vector2 vMax = ImGui.GetWindowContentRegionMax();

        vMin.X += ImGui.GetWindowPos().X;
        vMin.Y += ImGui.GetWindowPos().Y;
        vMax.X += ImGui.GetWindowPos().X;
        vMax.Y += ImGui.GetWindowPos().Y;
        WindowMin = vMin;
        WindowMax = vMax;
    }

    private static void IndexArrows()
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