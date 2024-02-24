#if DEBUG

using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using MeterWay.Data;
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
        cursor.Y += 50;
        var data = EncounterManager.Inst.encounters[indexEncounter];
        if(data.Active) data.Calculate();
        
        var encounterinfo = $"Name: {data.Name} Active: {(data.Active ? "true" : "false")} Finished {(data.Finished ? "true" : "false")} Duration:{data.Duration}";
        draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), encounterinfo);
        cursor.Y += textheight;
        encounterinfo = $"Dps {data.Dps}";
        draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), encounterinfo);
        cursor.Y += textheight;

        foreach (var p in data.Players)
        {
            var player = p.Value;
            string text = $"{player.Name}: ";

            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight;
            text = $"damage: {player.Damage.Total} Crt: {player.Damage.TotalCrit} Dps: {player.Dps} d%: {player.DamagePercent}% Crt%: {player.CritPercent}";
            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight;

            text = $"Heal:{player.Healing.Total} healCrt {player.Healing.TotalCrit} hps: {player.Hps} h%: {player.HealsPercent}% healCrt%: {player.Crithealspercent}";
            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight;

            text = $"Crits: {player.Damage.Count.Crit} DHits: {player.Damage.Count.Dh}% DCrits: {player.Damage.Count.CritDh} HealCrits: {player.Healing.Count.Crit}";
            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight;

            text = $"TakenDmg: {player.DamageTaken.Total} times: {player.DamageTaken.Count.Hit + player.DamageTaken.Count.Crit} TakenHeal: {player.HealingTaken.Total} times: {player.HealingTaken.Count.Hit + player.HealingTaken.Count.Crit}";
            draw.AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight;

            cursor.Y += textheight;
            ImGui.Separator();
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
        if(size < indexEncounter) indexEncounter = size;

        float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
        ImGui.PushButtonRepeat(true);
        if (ImGui.ArrowButton("##left", ImGuiDir.Left))
        {
            if(indexEncounter != 0) indexEncounter--;
        }
        ImGui.SameLine(0.0f, spacing);
        if (ImGui.ArrowButton("##right", ImGuiDir.Right))
        {
            if(size != indexEncounter) indexEncounter++;
        }
        ImGui.PopButtonRepeat();
        ImGui.SameLine();
        ImGui.Text($"index {indexEncounter} | size {size}");
    }
}

#endif