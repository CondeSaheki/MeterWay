using System.Linq;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using Dalamud.Interface.ManagedFontAtlas;

using MeterWay.Utils;
using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;
using MeterWay.Windows;

namespace Mogu;

public partial class Overlay : IOverlay, IOverlayTab
{
    public static string Name => "Mogu"; // required

    private OverlayWindow Window { get; init; }
    private Configuration Config { get; init; }

    private IFontAtlas FontAtlas { get; init; } = MeterWay.Dalamud.PluginInterface.UiBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async);
    private IFontHandle FontMogu { get; set; }
    private int FontsIndex = 0;
    private IFontHandle DefaultFont => FontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(20)));

    private Encounter Data = new();
    private List<uint> SortCache = [];

    public Overlay(OverlayWindow overlayWindow)
    {
        Config = File.Load<Configuration>(Name);
        Window = overlayWindow;
        Window.Flags = OverlayWindow.defaultflags; // temporary
        FontMogu = DefaultFont; // TODO load font from config
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
        Canvas windowCanvas = OverlayWindow.GetCanvas();
        Canvas cursor = new(windowCanvas.Area);

        FontMogu.Push();
        float fontSize = ImGui.GetFontSize();
        uint spacing = 5;
        string text = "";
        //Vector2 position = new();


        // Background
        if (Config.Background) draw.AddRectFilled(cursor.Min, cursor.Max, Config.BackgroundColor);
        cursor.Padding(15);

        // Header
        if (Config.Header)
        {
            if (Config.HeaderBackground)
            {
                draw.AddRectFilled(windowCanvas.Min, new Vector2(windowCanvas.Max.X, windowCanvas.Min.Y + fontSize), Config.HeaderBackgroundColor);
            }

            text = $"{Data.Duration.ToString(@"mm\:ss")} | {Helpers.HumanizeNumber(Data.Dps, 2)}";
            draw.AddText(cursor.Align(text), Config.MoguFontColor, text);
            cursor.Move((0, fontSize + spacing));
        }

        // Players
        foreach (var id in SortCache.ToList())
        {
            Player player = Data.Players[id];
            if (player.DamageDealt.Value.Total == 0) continue;

            if (Config.Bar)
            {
                var progress = Data.DamageDealt.Value.Total != 0 ? player.DamageDealt.Value.Total / Data.DamageDealt.Value.Total : 1;
                var barColor = Config.BarColorJob ? GetJobColor(player.Job) : Config.BarColor;
                DrawProgressBar(cursor.Area, progress, barColor);
            }

            if (Config.PlayerJobIcon)
            {
                DrawJobIcon(cursor.Min + new Vector2(1, 1), fontSize, player.Job);
                draw.AddText(cursor.Align(player.Name) + new Vector2(fontSize, 0), Config.MoguFontColor, player.Name);
            }
            else draw.AddText(cursor.Align(player.Name) + new Vector2(fontSize, 0), Config.MoguFontColor, player.Name);

            var damageinfo = $"{Helpers.HumanizeNumber(player.Dps, 2)} {player.DamageDealt.Value.Percent.Neutral}%";
            draw.AddText(cursor.Align(damageinfo), Config.MoguFontColor, damageinfo);
            cursor.Move((0, fontSize + spacing));
        }

        FontMogu.Pop();
    }

    public void Dispose()
    {
        FontMogu.Dispose();
        FontAtlas.Dispose();
    }
}