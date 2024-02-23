using System;
using System.Numerics;
using ImGuiNET;

using MeterWay.Utils;
using MeterWay.Data;
using MeterWay.Managers;
using System.Linq;

namespace MeterWay.Overlays;

public class DebugOverlay : IMeterwayOverlay
{
    public string Name => "DebugOverlay";
    private Encounter data;

    private Vector2 WindowMin { get; set; }
    private Vector2 WindowMax { get; set; }
    
    public DebugOverlay()
    {
        data = new Encounter();
        WindowMin = new Vector2();
        WindowMax = new Vector2();
    }

    public void DataProcess()
    {
        EncounterManager.Inst.CurrentEncounter().Calculate();
        data = EncounterManager.Inst.encounters.Last();
    }

    public void Draw()
    {
        data.Calculate();
        UpdateWindowSize();

        // background
        ImGui.GetWindowDrawList().AddRectFilled(WindowMin, WindowMax, Helpers.Color(0, 0, 0, 255));

        var textheight = (float)Math.Ceiling(ImGui.GetFontSize());
        Vector2 cursor = WindowMin;
        
        var encounterinfo = $"{data.Duration.ToString(@"mm\:ss")} | {data.Dps}";
        ImGui.GetWindowDrawList().AddText(cursor, Helpers.Color(255, 255, 255, 255), encounterinfo);
        cursor.Y += textheight;

        foreach (var p in data.Players)
        {
            var player = p.Value;
            string text = $"{player.Name}: ";
            ImGui.GetWindowDrawList().AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight;
            text = $"damage: {player.Damage.Total} Crt: {player.Damage.TotalCrit} Dps: {player.Dps} d%: {player.DamagePercent}% Crt%: {player.CritPercent}";
            ImGui.GetWindowDrawList().AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight;
            
            text = $"Heal:{player.Healing.Total} healCrt {player.Healing.TotalCrit} hps: {player.Hps} h%: {player.HealsPercent}% healCrt%: {player.Crithealspercent}";
            ImGui.GetWindowDrawList().AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight;
            
            text = $"Crits: {player.Damage.Count.Crit} DHits: {player.Damage.Count.Dh}% DCrits: {player.Damage.Count.CritDh} HealCrits: {player.Healing.Count.Crit}";
            ImGui.GetWindowDrawList().AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
            cursor.Y += textheight;

            text = $"TakenDmg: {player.DamageTaken.Total} times: {player.DamageTaken.Count.Hit + player.DamageTaken.Count.Crit} TakenHeal: {player.HealingTaken.Total} times: {player.HealingTaken.Count.Hit + player.HealingTaken.Count.Crit}";
            ImGui.GetWindowDrawList().AddText(cursor, Helpers.Color(255, 255, 255, 255), text);
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
}