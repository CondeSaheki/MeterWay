using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using MeterWay.Utils;
using MeterWay.managers;
using Lumina.Excel.GeneratedSheets;

namespace MeterWay.Overlays;

public class MoguOverlay : IMeterwayOverlay
{
    public string Name => "MoguOverlay";
    private Encounter data;

    private Vector2 WindowMin { get; set; }
    private Vector2 WindowMax { get; set; }

    public MoguOverlay()
    {
        this.data = new Encounter();
        this.WindowMin = new Vector2();
        this.WindowMax = new Vector2();
    }

    public void DataProcess(Encounter data)
    {
        this.data = data;

    }

    public void Draw()
    {
        UpdateWindowSize();
        ImGui.GetWindowDrawList().AddRectFilled(WindowMin, WindowMax, Helpers.Color(0, 0, 0, 64));

        Vector2 cursor = WindowMin;
        var info = $"{data.Name} | {data.duration.ToString()}";
        ImGui.GetWindowDrawList().AddText(cursor, Helpers.Color(255, 255, 255, 255), info);
        
        cursor.Y += (float)Math.Ceiling(ImGui.GetFontSize());

        foreach (KeyValuePair<uint, Player> p in data.Players)
        {
            Widget.JobIcon(p.Value.Job, cursor, ImGui.GetFontSize());
            var playerinfo = $"{p.Value.Name} | {p.Value.TotalDamage.ToString()}";
            ImGui.GetWindowDrawList().AddText(cursor + new Vector2(ImGui.GetFontSize(), 0), Helpers.Color(255, 255, 255, 255), playerinfo);
            cursor.Y += (float)Math.Ceiling(ImGui.GetFontSize());
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
        this.WindowMin = vMin;
        this.WindowMax = vMax;
    }
}