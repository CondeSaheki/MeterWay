using System;

using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using MeterWay.Utils;
using MeterWay.Data;
using System.Linq;
using MeterWay.Utils.Draw;

namespace MeterWay.Overlays;

public class MoguOverlay : IMeterwayOverlay
{
    public string Name => "MoguOverlay";
    private Encounter data;

    private Vector2 WindowMin { get; set; }
    private Vector2 WindowMax { get; set; }
    private List<uint> sortcache;
    public MoguOverlay()
    {
        this.data = new Encounter();
        this.WindowMin = new Vector2();
        this.WindowMax = new Vector2();
        this.sortcache = new List<uint>();
    }

    public void DataProcess(List<Encounter> data)
    {
        var currentEncounter = (data.Last().active || data.Last().finished) ? data.Last() : (data.Count() > 1 ? data[data.Count() - 2] : data.Last());

        // if (data.partyListId != this.data.partyListId) this.sortcache = Helpers.CreateDictionarySortCache(data.Players);

        // sortcache.Sort((uint first, uint second) => { return data.Players[second].TotalDamage.CompareTo(data.Players[first].TotalDamage); });

        this.data = currentEncounter;
    }

    public void Draw()
    {
        UpdateWindowSize();
        this.data.UpdateStats();
        //ImGui.GetWindowDrawList().AddRectFilled(WindowMin, WindowMax, Helpers.Color(0, 0, 0, 64));

        Vector2 cursor = WindowMin;
        var header = $"{data.duration.ToString(@"mm\:ss")} | {Helpers.HumanizeNumber(data.DPS, 2).ToString()}";
        ImGui.GetWindowDrawList().AddText(cursor + new Vector2((WindowMax.X - WindowMin.X) / 2 - Widget.CalcTextSize(header).X / 2, 0), Helpers.Color(255, 255, 255, 255), header);

        cursor.Y += (float)Math.Ceiling(ImGui.GetFontSize());

        foreach (var id in sortcache)
        {
            Player p = data.Players[id];
            //if(p.TotalDamage == 0) continue;

            Widget.JobIcon(p.Job, cursor, ImGui.GetFontSize());
            var damageinfo = $"{Helpers.HumanizeNumber(p.DPS, 2).ToString()} {p.DamagePercentage.ToString()}%"; //{p.TotalDamage.ToString()}

            ImGui.GetWindowDrawList().AddText(cursor + new Vector2(ImGui.GetFontSize(), 0), Helpers.Color(255, 255, 255, 255), $"{p.Name}");

            ImGui.GetWindowDrawList().AddText(cursor + new Vector2((WindowMax.X - WindowMin.X) - Widget.CalcTextSize(damageinfo).X, 0), Helpers.Color(255, 255, 255, 255), damageinfo);
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