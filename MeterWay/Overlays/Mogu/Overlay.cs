using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;

using MeterWay.Utils;
using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Utils.Draw;
using MeterWay.Overlay;
using MeterWay.Windows;

namespace Mogu;

public class Overlay : MeterWayOverlay, IDisposable
{
    public new static string Name => "Mogu";
    public OverlayWindow Window { get; private init; }
    public override bool HasConfigurationTab => true;

    public Configuration Config { get; }
    private Encounter data;
    private Vector2 WindowMin { get; set; }
    private Vector2 WindowMax { get; set; }
    private List<uint> SortCache;

    public Overlay(OverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        Config = Load<Configuration>();
        data = new();
        Window.Flags = OverlayWindow.defaultflags; // temporary

        WindowMin = new();
        WindowMax = new();
        SortCache = [];
    }

    public override void DataProcess()
    {
        var oldPartyId = data.Party.Id;
        data = EncounterManager.Inst.CurrentEncounter();

        if (data.Party.Id != oldPartyId) SortCache = Helpers.CreateDictionarySortCache(data.Players, (x) => { return true; });
        SortCache.Sort((uint first, uint second) => { return data.Players[second].DamageDealt.Total.CompareTo(data.Players[first].DamageDealt.Total); });
        if (!Config.FrameCalc) data.Calculate();
    }

    public override void Draw()
    {
        var sortCache = SortCache.ToList();
        if (Config.FrameCalc && !data.Finished && data.Active) data.Calculate(); // this will ignore last frame data ??
        UpdateWindowSize();
        var Draw = ImGui.GetWindowDrawList();

        if (Config.Background) Draw.AddRectFilled(WindowMin, WindowMax, Helpers.Color(Config.BackgroundColor));

        Vector2 cursor = WindowMin;
        var header = $"{data.Duration.ToString(@"mm\:ss")} | {Helpers.HumanizeNumber(data.Dps, 2)}";
        Draw.AddText(cursor + new Vector2((WindowMax.X - WindowMin.X) / 2 - Widget.CalcTextSize(header, 1f).X / 2, 0), Helpers.Color(255, 255, 255, 255), header);

        cursor.Y += (float)Math.Ceiling(ImGui.GetFontSize());

        foreach (var id in sortCache)
        {
            Player p = data.Players[id];
            if (p.DamageDealt.Total == 0) continue;

            Widget.JobIcon(p.Job, cursor, ImGui.GetFontSize());
            var damageinfo = $"{Helpers.HumanizeNumber(p.Dps, 2)} {p.DamagePercent}%"; //{p.TotalDamage.ToString()}

            Draw.AddText(cursor + new Vector2(ImGui.GetFontSize(), 0), Helpers.Color(255, 255, 255, 255), $"{p.Name}");

            Draw.AddText(cursor + new Vector2(WindowMax.X - WindowMin.X - Widget.CalcTextSize(damageinfo, 1f).X, 0), Helpers.Color(255, 255, 255, 255), damageinfo);
            cursor.Y += (float)Math.Ceiling(ImGui.GetFontSize());
        }
    }

    public override void DrawConfigurationTab() => ConfigurationTab.Draw(this);

    public override void Dispose() { }

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