using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ImGuiNET;
using Dalamud.Interface.ManagedFontAtlas;

using MeterWay.Utils;
using MeterWay.Managers;
using MeterWay.Data;
using MeterWay.Overlay;

namespace Lazer;

public partial class Overlay : BasicOverlay
{
    public static BasicOverlayInfo Info => new()
    {
        Name = "Lazer",
        Author = "Maotovisk",
        Description = "The Lazer overlay is your companion for your gaming journey, and for a any journey you need stunning aesthetics.",
    };

    private IOverlayWindow Window { get; init; }
    private Configuration Config { get; set; }

    private IFontAtlas FontAtlas { get; init; } = MeterWay.Dalamud.PluginInterface.UiBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async);
    private IFontHandle FontLazer { get; set; }
    private IFontHandle DefaultFont => FontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(20)));

    private Encounter? Data;
    private List<uint> SortCache = [];

    private Lerp<Dictionary<uint, PlayerData>> Lerping { get; init; }

    private CancellationTokenSource? DelayToken { get; set; }
    
    private uint _playerId = MeterWay.Dalamud.ClientState.LocalPlayer?.EntityId ?? 0;

    public struct PlayerData
    {
        public double Progress { get; set; }
        public double Damage { get; set; }
        public double CurrentPosition { get; set; }
    }

    public Overlay(IOverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        Config = Load<Configuration>(Window.WindowName);
        Lerping = CreateLerping();

        Init();
        FontLazer ??= DefaultFont;
    }

    public void Init()
    {
        if (Config.General.NoInput) Window.Flags |= ImGuiWindowFlags.NoInputs;
        if (Config.General.NoMove) Window.Flags |= ImGuiWindowFlags.NoMove;
        if (Config.General.NoResize) Window.Flags |= ImGuiWindowFlags.NoResize;

        Window.IsOpen = Config.Visibility.Enabled || Config.Visibility.Always;

        Window.SetSize(Config.General.Size);
        Window.SetPosition(Config.General.Position);

        if (Config.Font.LazerFontSpec != null) FontLazer = Config.Font.LazerFontSpec.CreateFontHandle(FontAtlas);
    }

    public override void OnEncounterUpdate()
    {
        var oldPartyId = Data?.Party.Id ?? 0;
        Data = EncounterManager.Inst.CurrentEncounter();

        if (Data.Party.Id != oldPartyId)
        {
            SortCache = Helpers.CreateDictionarySortCache(Data.Players, (x) => { return true; });
            Lerping.Reset();
        }

        SortCache.Sort((first, second) => Data.Players[second].DamageDealt.Value.Total.CompareTo(Data.Players[first].DamageDealt.Value.Total));
        UpdateLerping();
    }

    public override void Draw()
    {
        var draw = ImGui.GetWindowDrawList();
        var sortCache = SortCache.ToList();
        Canvas cursor = Window.GetCanvas();

        FontLazer?.Push(); // or do "using (FontLazer?.Push()) draw.AddText()"
        float fontSize = ImGui.GetFontSize();
        string text = string.Empty;
        Vector2 position = new();

        // Background
        draw.AddRectFilled(cursor.Min, cursor.Max, Config.Appearance.BackgroundColor, Config.Appearance.Rounding);

        // Set up the clip rect
        ImGui.PushClipRect(cursor.Min, cursor.Max, true);

        if (Data == null)
        {
            draw.AddRect(cursor.Min, cursor.Max, Config.Font.LazerFontColor, Config.Appearance.Rounding);
            text = $"{Info.Name}, No Data.";
            position = cursor.Align(text, Canvas.HorizontalAlign.Center, Canvas.VerticalAlign.Center);
            draw.AddText(position, Config.Font.LazerFontColor, text); // outlined
            FontLazer?.Pop();

            // Restore the previous clip rect
            ImGui.PopClipRect();
            return;
        }
        
        if (!Data.Finished && Data.Active) Data.Calculate(); // this will ignore last frame data ??

        // Header
        cursor.Max = new(cursor.Max.X, cursor.Min.Y + fontSize + 2 * Config.Appearance.Spacing);
        draw.AddRectFilled(cursor.Min, cursor.Max, Config.Appearance.HeaderBackgroundColor, Config.Appearance.Rounding, ImDrawFlags.RoundCornersTop);

        text = $"[{Data.Duration.ToString(@"mm\:ss")}] - {Data.Name}";
        position = cursor.Align(text, Canvas.HorizontalAlign.Center, Canvas.VerticalAlign.Center);
        draw.AddText(position, ColorWhite, text);
        cursor.Move((0, cursor.Height));

        // Players
        var dataLerped = Lerping.Now();

        var lineHeight = cursor.Height;

        foreach (var id in sortCache)
        {
            Player player = Data.Players[id];
            var playerLerped = dataLerped?[id];
            float currentPosition = (float)(playerLerped?.CurrentPosition ?? 0);

            Canvas line = new((
                    new(cursor.Min.X, cursor.Min.Y + currentPosition * lineHeight),
                    new(cursor.Max.X, cursor.Min.Y + (currentPosition + 1) * lineHeight)
                ));

            // Background
            draw.AddRectFilled(line.Min, line.Max, Config.Appearance.HeaderBackgroundColor);

            // Bar
            var barColor = Config.Appearance.JobColors.Override ? Helpers.ColorU32AlphaOverride(GetJobColor(player.Job), Config.Appearance.JobColors.Opacity) :GetJobColor(player.Job);
            float progress = (float)(playerLerped?.Progress ?? 1);
            line.Padding(0);
            DrawProgressBar(line.Area, barColor, progress, player.Id == _playerId);
            draw.AddRect(line.Min, line.Max, _playerId == player.Id ? Config.Appearance.YourBarColor : Config.Appearance.BarBorderColor,0, ImDrawFlags.None, player.Id == _playerId ? 2: 1);

            // icon
            Canvas icon = new(line.Area);
            icon.Max = icon.Min + new Vector2(icon.Height, icon.Height);
            icon.Padding(1);    
            draw.AddRectFilled(icon.Min, icon.Max, ColorBlack);
            draw.AddRect(icon.Min, icon.Max, Config.Appearance.JobIconBorderColor);
            DrawJobIcon(icon, player.Job);
            line.AddMin(icon.Height + Config.Appearance.Spacing, 0);
            
            // Texts
            if (Config.Appearance.Layout.DisplayJobAcronym)
            {
                text = player.Job.Acronym;
                position = line.Padding((Config.Appearance.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
                draw.AddText(position, Config.Appearance.JobNameTextColor, text);
                line.AddMin(ImGui.CalcTextSize(text).X + Config.Appearance.Spacing, 0);
            }

            text = $"{sortCache.IndexOf(id) + 1} » {player.Name.Split(" ")[0]} {player.Name.Split(" ")[1][0]}.";
            position = line.Padding((Config.Appearance.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
            draw.AddText(position, ColorWhite, text);
            line.AddMin(ImGui.CalcTextSize(text).X + Config.Appearance.Spacing, 0);

            text = $"({Helpers.HumanizeNumber(player.PerSeconds.DamageDealt, 1)})";
            position = line.Padding((Config.Appearance.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Right, Canvas.VerticalAlign.Center);
            draw.AddText(position, ColorWhite, text);
            line.AddMax(-ImGui.CalcTextSize(text).X - Config.Appearance.Spacing, 0);
            if (Config.Appearance.Layout.DisplayTotalDamage)
            {
                text = $"{Helpers.HumanizeNumber(playerLerped?.Damage ?? 0, 1)}";
                position = line.Padding((Config.Appearance.Spacing, 0))
                    .Align(text, Canvas.HorizontalAlign.Right, Canvas.VerticalAlign.Center);
                draw.AddText(position, Config.Appearance.TotalDamageTextColor, text); // scale 0.8
            }
        }

        draw.AddRect(Window.GetCanvas().Min, Window.GetCanvas().Max, Config.Appearance.BorderColor, Config.Appearance.Rounding);

        // Restore the previous clip rect
        ImGui.PopClipRect();

        FontLazer?.Pop();
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
        
        _playerId = MeterWay.Dalamud.ClientState.LocalPlayer?.EntityId ?? 0;
    }

    public override void OnClose()
    {
        SaveCurrentWindowData();
    }

    public static void Remove(IOverlayWindow window)
    {
        Delete(window.WindowName);
    }

    public override void Dispose()
    {
        SaveCurrentWindowData();
        DelayToken?.Cancel();
        DelayToken?.Dispose();
        FontLazer?.Dispose();
        FontAtlas?.Dispose();
    }
}
