using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using ImGuiNET;
using Dalamud.Interface.ManagedFontAtlas;

using MeterWay.Utils;
using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;

namespace Mogu;

public partial class Overlay : BasicOverlay
{
    public static BasicOverlayInfo Info => new()
    {
        Name = "Mogu",
        Author = "MeterWay",
        Description = "Mogu is a overlay designed to provide a versatile and complete solution for a wide range of needs this overlay offers a clean and flexible interface tailored to enhance your experience across various contexts with no amogus please.",
    };

    private IOverlayWindow Window { get; init; }
    private Configuration Config { get; set; }

    private IFontAtlas FontAtlas { get; init; } = MeterWay.Dalamud.PluginInterface.UiBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async);
    private IFontHandle FontMogu { get; set; }
    private IFontHandle DefaultFont => FontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(20)));

    private Encounter? Data;
    private List<uint> SortCache = [];

    private CancellationTokenSource? DelayToken { get; set; }

    public Overlay(IOverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        Config = Load<Configuration>(Window.WindowName);

        Init();
        FontMogu ??= DefaultFont;
    }

    void Init()
    {
        if (Config.General.NoInput) Window.Flags |= ImGuiWindowFlags.NoInputs;
        if (Config.General.NoMove) Window.Flags |= ImGuiWindowFlags.NoMove;
        if (Config.General.NoResize) Window.Flags |= ImGuiWindowFlags.NoResize;

        Window.IsOpen = Config.Visibility.Enabled || Config.Visibility.Always;

        Window.SetPosition(Config.General.Position);
        Window.SetSize(Config.General.Size);

        if (Config.Font.MoguFontSpec != null) FontMogu = Config.Font.MoguFontSpec.CreateFontHandle(FontAtlas);
    }

    public override void OnEncounterUpdate()
    {
        var oldPartyId = Data?.Party.Id ?? 0;
        Data = EncounterManager.Inst.CurrentEncounter();

        if (Data.Party.Id != oldPartyId) SortCache = Helpers.CreateDictionarySortCache(Data.Players, (x) => { return true; });
        SortCache.Sort((first, second) => Data.Players[second].DamageDealt.Value.Total.CompareTo(Data.Players[first].DamageDealt.Value.Total));
        if (!Config.General.FrameCalc) Data.Calculate();
    }

    public override void Draw()
    {
        if (Data == null) return;
        if (Config.General.FrameCalc && !Data.Finished && Data.Active) Data.Calculate(); // this will ignore last frame data ??

        var draw = ImGui.GetWindowDrawList();
        Canvas cursor = Window.GetCanvas();

        FontMogu.Push();
        float fontSize = ImGui.GetFontSize();
        float spacing = 2f;
        string text = "";
        Vector2 position = cursor.Min;

        // Background
        if (Config.Appearance.Background) draw.AddRectFilled(cursor.Min, cursor.Max, Config.Appearance.BackgroundColor);
        cursor.Padding(spacing);
        cursor.Max = new(cursor.Max.X, cursor.Min.Y + fontSize + 2 * spacing);

        // Header
        if (Config.Appearance.Header)
        {
            if (Config.Appearance.HeaderBackground)
            {
                draw.AddRectFilled(cursor.Min, cursor.Max, Config.Appearance.HeaderBackgroundColor);
            }

            text = $"{Data.Duration.ToString(@"mm\:ss")} | {Helpers.HumanizeNumber(Data.PerSecounds.DamageDealt, 2)}";
            position = cursor.Padding((spacing, 0)).Align(text, Config.Appearance.HeaderAlignment, Canvas.VerticalAlign.Center);
            draw.AddText(position, Config.Font.MoguFontColor, text);
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

            if (Config.Appearance.Bar)
            {
                var barColor = Config.Appearance.BarColorJob ? GetJobColor(player.Job) : Config.Appearance.BarColor;
                DrawProgressBar(cursor.Area, progress, barColor);
            }

            position = cursor.Padding((spacing, 0)).Align(player.Name, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
            if (Config.Appearance.PlayerJobIcon)
            {
                Canvas iconArea = new(cursor.Area);
                iconArea.Max = iconArea.Min + new Vector2(iconArea.Height, iconArea.Height);
                iconArea.Padding(spacing + 1);
                DrawJobIcon(iconArea, player.Job);
                draw.AddText(position + new Vector2(iconArea.Width + 2 * spacing, 0), Config.Font.MoguFontColor, player.Name);
            }
            else draw.AddText(position, Config.Font.MoguFontColor, player.Name);

            var damageinfo = $"{Helpers.HumanizeNumber(player.PerSecounds.DamageDealt, 2)} {Math.Round(pct * 100, 2).ToString(CultureInfo.InvariantCulture)}%";
            position = cursor.Padding((spacing, 0)).Align(damageinfo, Canvas.HorizontalAlign.Right, Canvas.VerticalAlign.Center);
            draw.AddText(position, Config.Font.MoguFontColor, damageinfo);
            cursor.Move((0, cursor.Height + spacing));
        }

        FontMogu.Pop();
    }

    public override void OnEncounterEnd()
    {
        if (Config.Visibility.Always || !Config.Visibility.Combat) return;

        if (!Config.Visibility.Delay)
        {
            Window.IsOpen = false;
            return;
        }

        DelayToken?.Cancel();
        DelayToken = Helpers.DelayedAction(Config.Visibility.DelayDuration, () =>
        {
            Window.IsOpen = false;
        });
    }

    public override void OnEncounterBegin()
    {
        DelayToken?.Cancel();
        if (Config.Visibility.Always || Config.Visibility.Combat)
        {
            Window.IsOpen = true;
            return;
        }
    }
    
    public override void OnOpen()
    {
        Window.SetSize(Config.General.Size);
        Window.SetPosition(Config.General.Position);
    }

    public override void OnClose()
    {
        if (Window.CurrentPosition != null) Config.General.Position = (Vector2)Window.CurrentPosition;
        if (Window.CurrentSize != null) Config.General.Size = (Vector2)Window.CurrentSize;
        Save(Window.WindowName, Config);
    }

    public static void Remove(IOverlayWindow window)
    {
        Delete(window.WindowName);
    }

    // public string CommandHelpMessage(string? command)
    // {
    //     StringBuilder builder = new();

    //     builder.Append("command");
    //     builder.Append("command");

    //     return builder.ToString();
    // }

    // public Action? OnCommand(List<string> args)
    // {
    //     Action? handler = args[0] switch
    //     {
    //         "config" when args.Count == 1 => () =>
    //         {
    //             MeterWay.Dalamud.Chat.Print("hi");
    //         },
    //         "" when args.Count == 1 => null,
    //         _ => null
    //     };
    //     return handler;
    // }

    public override void Dispose()
    {
        DelayToken?.Cancel();
        DelayToken?.Dispose();
        FontMogu?.Dispose();
        FontAtlas?.Dispose();
    }
}