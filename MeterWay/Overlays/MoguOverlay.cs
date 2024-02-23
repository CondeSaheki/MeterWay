using System;

using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using MeterWay.Utils;
using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Utils.Draw;
using System.Linq;


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
        data = new Encounter();
        WindowMin = new Vector2();
        WindowMax = new Vector2();
        sortcache = [];
    }

    public void DataProcess()
    {
        var oldPartyId = data.Party.Id;

        data = EncounterManager.Inst.CurrentEncounter();

        if (data.Party.Id != oldPartyId) sortcache = Helpers.CreateDictionarySortCache(data.Players, (x) => { return true; });

        sortcache.Sort((uint first, uint second) => { return data.Players[second].Damage.Total.CompareTo(data.Players[first].Damage.Total); });
    }

    private List<uint> getSortCache() { return sortcache.ToList(); }

    public void Draw()
    {
        var sortCache = getSortCache();
        if (!data.Finished && data.Active) data.Calculate(); // this will ignore last frame data ??
        UpdateWindowSize();
        var Draw = ImGui.GetWindowDrawList();
        
        //Draw.AddRectFilled(WindowMin, WindowMax, Helpers.Color(0, 0, 0, 64));

        Vector2 cursor = WindowMin;
        var header = $"{data.Duration.ToString(@"mm\:ss")} | {Helpers.HumanizeNumber(data.Dps, 2)}";
        Draw.AddText(cursor + new Vector2((WindowMax.X - WindowMin.X) / 2 - Widget.CalcTextSize(header).X / 2, 0), Helpers.Color(255, 255, 255, 255), header);

        cursor.Y += (float)Math.Ceiling(ImGui.GetFontSize());

        foreach (var id in sortCache)
        {
            Player p = data.Players[id];
            if (p.Damage.Total == 0) continue;

            Widget.JobIcon(p.Job, cursor, ImGui.GetFontSize());
            var damageinfo = $"{Helpers.HumanizeNumber(p.Dps, 2)} {p.DamagePercent}%"; //{p.TotalDamage.ToString()}

            Draw.AddText(cursor + new Vector2(ImGui.GetFontSize(), 0), Helpers.Color(255, 255, 255, 255), $"{p.Name}");

            Draw.AddText(cursor + new Vector2(WindowMax.X - WindowMin.X - Widget.CalcTextSize(damageinfo).X, 0), Helpers.Color(255, 255, 255, 255), damageinfo);
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
        WindowMin = vMin;
        WindowMax = vMax;
    }
}