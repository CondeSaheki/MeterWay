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

public partial class Overlay : IOverlay, IOverlayTab
{
    public static string Name => "Mogu"; // required

    private OverlayWindow Window { get; init; }
    private Configuration Config { get; init; }

    private Encounter Data = new();
    private Vector2 WindowMin = new();
    private Vector2 WindowMax = new();
    private List<uint> SortCache = [];

    public Overlay(OverlayWindow overlayWindow)
    {
        Config = File.Load<Configuration>(Name);
        Window = overlayWindow;
        Window.Flags = OverlayWindow.defaultflags; // temporary
    }

    public void DataProcess()
    {
        var oldPartyId = Data.Party.Id;
        Data = EncounterManager.Inst.CurrentEncounter();

        if (Data.Party.Id != oldPartyId) SortCache = Helpers.CreateDictionarySortCache(Data.Players, (x) => { return true; });
        SortCache.Sort((uint first, uint second) => { return Data.Players[second].DamageDealt.Total.CompareTo(Data.Players[first].DamageDealt.Total); });
        if (!Config.FrameCalc) Data.Calculate();
    }

    public void Draw()
    {
        var sortCache = SortCache.ToList();
        if (Config.FrameCalc && !Data.Finished && Data.Active) Data.Calculate(); // this will ignore last frame data ??
        UpdateWindowSize();
        var Draw = ImGui.GetWindowDrawList();

        if (Config.Background) Draw.AddRectFilled(WindowMin, WindowMax, Helpers.Color(Config.BackgroundColor));

        Vector2 cursor = WindowMin;
        var header = $"{Data.Duration.ToString(@"mm\:ss")} | {Helpers.HumanizeNumber(Data.Dps, 2)}";
        Draw.AddText(cursor + new Vector2((WindowMax.X - WindowMin.X) / 2 - Widget.CalcTextSize(header, 1f).X / 2, 0), Helpers.Color(255, 255, 255, 255), header);

        cursor.Y += (float)Math.Ceiling(ImGui.GetFontSize());

        foreach (var id in sortCache)
        {
            Player p = Data.Players[id];
            if (p.DamageDealt.Total == 0) continue;

            Widget.JobIcon(p.Job, cursor, ImGui.GetFontSize());
            var damageinfo = $"{Helpers.HumanizeNumber(p.Dps, 2)} {p.DamagePercent}%"; //{p.TotalDamage.ToString()}

            Draw.AddText(cursor + new Vector2(ImGui.GetFontSize(), 0), Helpers.Color(255, 255, 255, 255), $"{p.Name}");

            Draw.AddText(cursor + new Vector2(WindowMax.X - WindowMin.X - Widget.CalcTextSize(damageinfo, 1f).X, 0), Helpers.Color(255, 255, 255, 255), damageinfo);
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