using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

using MeterWay.Utils.Draw;
using MeterWay.Utils;
using MeterWay.Managers;
using MeterWay.Data;
using MeterWay.Windows;
using MeterWay.Overlay;

namespace Lazer;

public partial class Overlay : IOverlay, IOverlayTab
{
    public static string Name => "Lazer"; // required

    private OverlayWindow Window { get; init; }
    private Configuration Config { get; init; }

    private Encounter Combat = new();
    private List<uint> SortCache = [];
    private Vector2 WindowMin = new();
    private Vector2 WindowMax = new();

    private class LerpPlayerData
    {
        public double DPS { get; set; }
        public double PctBar { get; set; }
        public double TotalDMG { get; set; }
        public double Position { get; set; }
    }

    private double transitionDuration = 0.3f; // in seconds
    private double transitionTimer = 0.0f;
    private Dictionary<uint, LerpPlayerData> lerpedInfo = [];
    private Dictionary<uint, LerpPlayerData> targetInfo = [];

    public Overlay(OverlayWindow overlayWindow)
    {
        Config = File.Load<Configuration>(Name);
        Window = overlayWindow;
        Window.Flags = OverlayWindow.defaultflags; // temporary
    }

    public void DataProcess()
    {
        var currentEncounter = EncounterManager.Inst.CurrentEncounter();
        var oldListId = Combat.Party.Id;
        var oldCombatId = Combat.Id;

        Combat = currentEncounter;

        if (currentEncounter.Id != oldCombatId)
        {
            targetInfo = [];
            lerpedInfo = [];

            SortCache = Helpers.CreateDictionarySortCache(Combat.Players, (x) => { return true; });
        }

        if (oldListId != currentEncounter.Party.Id)
        {
            SortCache = Helpers.CreateDictionarySortCache(Combat.Players, (x) => { return true; });
        }

        SortCache.Sort((uint first, uint second) => { return Combat.Players[second].DamageDealt.Total.CompareTo(Combat.Players[first].DamageDealt.Total); });
    }

    private List<uint> LocalSortCache() => SortCache.ToList();

    public void Draw()
    {
        var sortCache = LocalSortCache();
        UpdateWindowSize();
        if (!Combat.Finished && Combat.Active) Combat.Calculate(); // this will ignore last frame data ??

        ImGui.SetWindowFontScale(Config.FontScale);

        ImGui.GetWindowDrawList().AddRectFilled(WindowMin, WindowMax, Helpers.Color(Config.BackgroundColor));

        ImGui.GetWindowDrawList().AddRectFilled(WindowMin, new Vector2(WindowMax.X, WindowMin.Y + (ImGui.GetFont().FontSize + 5) * Config.FontScale), Helpers.Color(26, 26, 39, 190));

        if (Combat.Begin == null)
        {
            Widget.Text("Not in combat", WindowMin, Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, anchor: Widget.TextAnchor.Center, dropShadow: true);
            return;
        }

        Widget.Text($"{Combat.Name} - ({(!Combat.Active ? "Completed in " : "")}{Combat.Duration.ToString(@"mm\:ss")})", WindowMin, Helpers.Color(255, 255, 255, 255), WindowMin, WindowMax, anchor: Widget.TextAnchor.Center);

        foreach (var id in sortCache)
        {
            Player player = Combat.Players[id];
            player.Calculate();
            DoLerpPlayerData(player);
            DrawPlayerLine(lerpedInfo[player.Id], player);
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
