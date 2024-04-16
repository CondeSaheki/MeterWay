using System.Linq;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using Dalamud.Interface.ManagedFontAtlas;
using System;
using System.Globalization;
using static Dalamud.Interface.Windowing.Window;

using MeterWay.Utils;
using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;

namespace Mogu;

public partial class Overlay : IOverlay, IOverlayTab
{
    public static string Name => "Mogu"; // required

    private OverlayWindow Window { get; init; }
    private Configuration Config { get; init; }

    private IFontAtlas FontAtlas { get; init; } = MeterWay.Dalamud.PluginInterface.UiBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async);
    private IFontHandle FontMogu { get; set; }
    private IFontHandle DefaultFont => FontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(20)));

    private Encounter Data = new();
    private List<uint> SortCache = [];

    public Overlay(OverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        Config = File.Load<Configuration>($"{Window.Name}{Window.Id}");
        Window.Flags = OverlayWindow.defaultflags; // temporary
        Window.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(320, 180),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        FontMogu = Config.MoguFontSpec != null ? Config.MoguFontSpec.CreateFontHandle(FontAtlas) : DefaultFont;
    }

    public void DataUpdate()
    {
        var oldPartyId = Data.Party.Id;
        Data = EncounterManager.Inst.CurrentEncounter();

        if (Data.Party.Id != oldPartyId) SortCache = Helpers.CreateDictionarySortCache(Data.Players, (x) => { return true; });
        SortCache.Sort((uint first, uint second) => { return Data.Players[second].DamageDealt.Value.Total.CompareTo(Data.Players[first].DamageDealt.Value.Total); });
        if (!Config.FrameCalc) Data.Calculate();
    }

    public void Draw()
    {
        if (Config.FrameCalc && !Data.Finished && Data.Active) Data.Calculate(); // this will ignore last frame data ??

        var draw = ImGui.GetWindowDrawList();
        // Canvas windowCanvas = OverlayWindow.GetCanvas();
        Canvas cursor = OverlayWindow.GetCanvas(); //new(windowCanvas.Area);

        FontMogu.Push();
        float fontSize = ImGui.GetFontSize();
        float spacing = 2f;
        string text = "";
        Vector2 position = cursor.Min;

        // Background
        if (Config.Background) draw.AddRectFilled(cursor.Min, cursor.Max, Config.BackgroundColor);
        cursor.Padding(spacing);
        cursor.Max = new(cursor.Max.X, cursor.Min.Y + fontSize + 2 * spacing);

        // Header
        if (Config.Header)
        {
            if (Config.HeaderBackground)
            {
                draw.AddRectFilled(cursor.Min, cursor.Max, Config.HeaderBackgroundColor);
            }

            text = $"{Data.Duration.ToString(@"mm\:ss")} | {Helpers.HumanizeNumber(Data.Dps, 2)}";
            position = cursor.Padding((spacing, 0)).Align(text, Config.HeaderAlignment, Canvas.VerticalAlign.Center);
            draw.AddText(position, Config.MoguFontColor, text);
            cursor.Move((0, cursor.Height + spacing));
        }

        // Players
        var cache = SortCache.ToList();
        uint topDamage = 1;
        if (cache.Count != 0) topDamage = Data.Players.GetValueOrDefault(cache.First())!.DamageDealt.Value.Total;
        foreach (var id in cache)
        {
            Player player = Data.Players[id];
            if (player.DamageDealt.Value.Total == 0) continue;
            float progress = (float)player.DamageDealt.Value.Total / topDamage;
            float pct = Data.DamageDealt.Value.Total != 0 ? (float)player.DamageDealt.Value.Total / Data.DamageDealt.Value.Total : 1;

            if (Config.Bar)
            {
                var barColor = Config.BarColorJob ? GetJobColor(player.Job) : Config.BarColor;
                DrawProgressBar(cursor.Area, progress, barColor);
            }

            position = cursor.Padding((spacing, 0)).Align(player.Name, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
            if (Config.PlayerJobIcon)
            {

                Canvas iconArea = new(cursor.Area);
                iconArea.Max = iconArea.Min + new Vector2(iconArea.Height, iconArea.Height);
                iconArea.Padding(spacing + 1);
                DrawJobIcon(iconArea, player.Job);
                draw.AddText(position + new Vector2(iconArea.Width + 2 * spacing, 0), Config.MoguFontColor, player.Name);
            }
            else draw.AddText(position, Config.MoguFontColor, player.Name);

            var damageinfo = $"{Helpers.HumanizeNumber(player.Dps, 2)} {Math.Round(pct * 100, 2).ToString(CultureInfo.InvariantCulture)}%";
            position = cursor.Padding((spacing, 0)).Align(damageinfo, Canvas.HorizontalAlign.Right, Canvas.VerticalAlign.Center);
            draw.AddText(position, Config.MoguFontColor, damageinfo);
            cursor.Move((0, cursor.Height + spacing));
        }

        FontMogu.Pop();
    }
    
    public void Remove()
    {
        File.Delete($"{Window.Name}{Window.Id}");
    }

    public void Dispose()
    {
        FontMogu?.Dispose();
        FontAtlas?.Dispose();
    }
}