using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.ManagedFontAtlas;

using MeterWay.Utils;
using MeterWay.Managers;
using MeterWay.Data;
using MeterWay.Overlay;

namespace Lazer;

public partial class Overlay : IOverlay, IOverlayConfig
{
    public static string Name => "Lazer"; // required

    private OverlayWindow Window { get; init; }
    private Configuration Config { get; init; }

    private Encounter Combat = new();
    private List<uint> SortCache = [];

    private IFontAtlas FontAtlas { get; init; } = MeterWay.Dalamud.PluginInterface.UiBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async);
    private IFontHandle FontLazer { get; set; }
    private IFontHandle DefaultFont => FontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(20)));

    private Dictionary<uint, LerpPlayerData> lerpedInfo = [];
    private Dictionary<uint, LerpPlayerData> targetInfo = [];

    public Overlay(OverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        Config = File.Load<Configuration>($"{Window.Name}{Window.Id}");
        Window.Flags = OverlayWindow.defaultflags; // temporary
        FontLazer = DefaultFont; // TODO load font from config
    }

    public void DataUpdate()
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

        SortCache.Sort((uint first, uint second) => { return Combat.Players[second].DamageDealt.Value.Total.CompareTo(Combat.Players[first].DamageDealt.Value.Total); });
    }

    public void Draw()
    {
        if (!Combat.Finished && Combat.Active) Combat.Calculate(); // this will ignore last frame data ??

        var draw = ImGui.GetWindowDrawList();
        var sortCache = SortCache.ToList();
        Canvas windowCanvas = OverlayWindow.GetCanvas();
        Canvas cursor = new((windowCanvas.Min, new Vector2(windowCanvas.Max.X, windowCanvas.Max.Y + ImGui.GetFontSize() + 5)));
        string text = string.Empty;
        Vector2 position = new();

        //FontLazer.Push();

        // Background
        draw.AddRectFilled(windowCanvas.Min, windowCanvas.Max, Config.BackgroundColor);
        draw.AddRectFilled(windowCanvas.Min, new Vector2(windowCanvas.Max.X, windowCanvas.Min.Y + (ImGui.GetFont().FontSize + 5)), Config.color1);

        // Empty
        if (Combat.Begin == null)
        {
            text = "Not in combat";
            position = windowCanvas.Align(text, Canvas.HorizontalAlign.Center, Canvas.VerticalAlign.Center);
            DrawText(position, text, colorWhite, true);
            return;
        }

        // Header
        text = $"{Combat.Name} - ({(!Combat.Active ? "Completed in " : "")}{Combat.Duration.ToString(@"mm\:ss")})";
        position = cursor.Align(text, Canvas.HorizontalAlign.Center);
        DrawText(position, text, colorWhite);
        cursor.Move((ImGui.GetFontSize() + 5, 0));

        // Players
        foreach (var id in sortCache)
        {
            Player player = Combat.Players[id];
            DoLerpPlayerData(player);

            DrawPlayerLine(windowCanvas, player, lerpedInfo[player.Id]);
            cursor.Move((ImGui.GetFontSize() + 5, 0));
        }

        //FontLazer.Pop();
    }

    public void Dispose() 
    {
        FontLazer?.Dispose();
        FontAtlas?.Dispose();
    }
    
    public void Remove()
    {
        File.Delete($"{Window.Name}{Window.Id}");
    }

    private void DrawPlayerLine(Canvas canvas, Player player, LerpPlayerData data)
    {
        var draw = ImGui.GetWindowDrawList();

        // Icon
        if (true)
        {
            var max = new Vector2(canvas.Min.X + canvas.Height, canvas.Min.Y + canvas.Height);
            draw.AddRectFilled(canvas.Min, max, Config.color6);
            draw.AddRect(canvas.Min, max, Config.color7);
            draw.AddImage(new Job(player.Job).Icon(), canvas.Min, max);
            canvas.AddMin(canvas.Height, 0);
        }

        // Background
        ImGui.GetWindowDrawList().AddRectFilled(canvas.Min, canvas.Max, Config.color1);

        // Bar
        var barColor = player.Name == "YOU" ? Config.color2 : Config.color3; // broken
        DrawProgressBar(canvas.Min, canvas.Max, barColor, (float)data.PctBar / 100.0f);
        draw.AddRect(canvas.Min, canvas.Max, Config.color4);

        // Texts
        string text = $"{new Job(player.Job).Id}";
        Vector2 position = canvas.Align(text, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
        canvas.AddMin(8f, 0);
        DrawText(position, text, Config.color5); // scale 0.8
        canvas.AddMin(ImGui.CalcTextSize(text).X, 0);

        text = player.Name;
        position = canvas.Align(text, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
        canvas.AddMin(8f, 0);
        DrawText(position, text, colorWhite);
        canvas.AddMin(ImGui.CalcTextSize(text).X, 0);

        text = $"{Helpers.HumanizeNumber(data.DPS, 1)}/s";
        position = canvas.Align(text, Canvas.HorizontalAlign.Right, Canvas.VerticalAlign.Center);
        DrawText(position, text, colorWhite);
        canvas.AddMax(-ImGui.CalcTextSize(text).X, 0);

        text = $"{Helpers.HumanizeNumber(data.TotalDMG, 1)}";
        position = canvas.Align(text, Canvas.HorizontalAlign.Right, Canvas.VerticalAlign.Center);
        DrawText(position, text, Config.color6); // scale 0.8f
    }
}