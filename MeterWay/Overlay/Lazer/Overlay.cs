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

    public struct PlayerData
    {
        public double Progress { get; set; }
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
        draw.AddRectFilled(cursor.Min, cursor.Max, Config.Appearance.BackgroundColor);

        if (Data == null)
        {
            draw.AddRect(cursor.Min, cursor.Max, Config.Font.LazerFontColor);
            text = $"{Info.Name}, No Data.";
            position = cursor.Align(text, Canvas.HorizontalAlign.Center, Canvas.VerticalAlign.Center);
            draw.AddText(position, Config.Font.LazerFontColor, text); // outlined
            FontLazer?.Pop();
            return;
        }

        // TODO Config.General.FrameCalc
        if (!Data.Finished && Data.Active) Data.Calculate(); // this will ignore last frame data ??

        // Header
        cursor.Max = new(cursor.Max.X, cursor.Min.Y + fontSize + 2 * Config.Appearance.Spacing);
        draw.AddRectFilled(cursor.Min, cursor.Max, Config.Appearance.HeaderBackgroundColor);

        text = $"{Data.Name} - ({(!Data.Active ? "Completed in " : "")}{Data.Duration.ToString(@"mm\:ss")})";
        position = cursor.Align(text, Canvas.HorizontalAlign.Center, Canvas.VerticalAlign.Center);
        draw.AddText(position, colorWhite, text);
        cursor.Move((0, cursor.Height));

        // Players
        var dataLerped = Lerping.Now();
        foreach (var id in sortCache)
        {
            Player player = Data.Players[id];
            var playerLerped = dataLerped?[id];
            Canvas line = new(cursor.Area);

            // Background
            draw.AddRectFilled(line.Min, line.Max, Config.Appearance.HeaderBackgroundColor);

            // Bar
            var barColor = player.Id == MeterWay.Dalamud.ClientState.LocalPlayer?.ObjectId ? Config.Appearance.YourBarColor : Config.Appearance.BarColor;
            float progress = (float)(playerLerped?.Progress ?? 1);
            DrawProgressBar(line.Area, barColor, progress);
            draw.AddRect(line.Min, line.Max, Config.Appearance.BarBorderColor);

            // icon
            Canvas icon = new(line.Area);
            icon.Max = icon.Min + new Vector2(icon.Height, icon.Height);
            icon.Padding(Config.Appearance.Spacing + 1);
            draw.AddRectFilled(icon.Min, icon.Max, colorBlack);
            draw.AddRect(icon.Min, icon.Max, Config.Appearance.JobIconBorderColor);
            DrawJobIcon(icon, player.Job);
            line.AddMin(icon.Height + Config.Appearance.Spacing, 0);

            // Texts
            text = player.Job.Acronym;
            position = line.Padding((Config.Appearance.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
            draw.AddText(position, Config.Appearance.JobNameTextColor, text); // scale 0.8

            line.AddMin(ImGui.CalcTextSize(text).X + Config.Appearance.Spacing, 0);

            text = player.Name;
            position = line.Padding((Config.Appearance.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
            draw.AddText(position, colorWhite, text);
            line.AddMin(ImGui.CalcTextSize(text).X + Config.Appearance.Spacing, 0);

            text = $"{Helpers.HumanizeNumber(player.PerSeconds.DamageDealt, 1)}/s";
            position = line.Padding((Config.Appearance.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Right, Canvas.VerticalAlign.Center);
            draw.AddText(position, colorWhite, text);
            line.AddMax(-ImGui.CalcTextSize(text).X - Config.Appearance.Spacing, 0);

            text = $"{Helpers.HumanizeNumber(player.DamageDealt.Value.Total, 1)}";
            position = line.Padding((Config.Appearance.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Right, Canvas.VerticalAlign.Center);
            draw.AddText(position, Config.Appearance.TotalDamageTextColor, text); // scale 0.8

            cursor.Move((0, cursor.Height));
        }
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
    }

    public override void OnClose()
    {
        SavaCurrentWindowData();
    }

    public static void Remove(IOverlayWindow window)
    {
        Delete(window.WindowName);
    }

    public override void Dispose()
    {
        SavaCurrentWindowData();
        DelayToken?.Cancel();
        DelayToken?.Dispose();
        FontLazer?.Dispose();
        FontAtlas?.Dispose();
    }
}