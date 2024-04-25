using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.ManagedFontAtlas;
using static Dalamud.Interface.Windowing.Window;

using MeterWay.Utils;
using MeterWay.Managers;
using MeterWay.Data;
using MeterWay.Overlay;

namespace Lazer;

public partial class Overlay : IOverlay, IOverlayConfig
{
    public static string Name => "Lazer"; // required
    public static string Autor => "Maotovisk";
    public static string Description => "TODO";

    private OverlayWindow Window { get; init; }
    private Configuration Config { get; init; }

    private IFontAtlas FontAtlas { get; init; } = MeterWay.Dalamud.PluginInterface.UiBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async);
    private IFontHandle FontLazer { get; set; }
    private IFontHandle DefaultFont => FontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(20)));

    private Encounter Data = new();
    private List<uint> SortCache = [];

    public Overlay(OverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        Config = File.Load<Configuration>($"{Window.Name}{Window.Id}");
        if (Config.ClickThrough) Window.Flags = OverlayWindow.defaultflags | ImGuiWindowFlags.NoInputs;
        else Window.Flags = OverlayWindow.defaultflags;
        Window.IsOpen = true;
        Window.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(320, 180),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        if (Config.LazerFontSpec != null)
        {
            FontLazer = Config.LazerFontSpec.CreateFontHandle(FontAtlas);
            MeterWay.Dalamud.Log.Info("Font handle created using custom LazerFontSpec.");
        }
        else
        {
            FontLazer = DefaultFont;
            MeterWay.Dalamud.Log.Info("Using default font as LazerFontSpec is not configured.");
        }
    }

    public void DataUpdate()
    {
        var oldPartyId = Data.Party.Id;
        Data = EncounterManager.Inst.CurrentEncounter();

        if (Data.Party.Id != oldPartyId)
        {
            SortCache = Helpers.CreateDictionarySortCache(Data.Players, (x) => { return true; });
            if (false) // ConfigurationManager.Inst.Configuration.OverlayRealtimeUpdate == true
            {
                // targetInfo = [];
                // lerpedInfo = [];
            }
        }
        SortCache.Sort((first, second) => Data.Players[second].DamageDealt.Value.Total.CompareTo(Data.Players[first].DamageDealt.Value.Total));
    }

    public void Draw()
    {
        if (!Data.Finished && Data.Active) Data.Calculate(); // this will ignore last frame data ??

        var draw = ImGui.GetWindowDrawList();
        var sortCache = SortCache.ToList();
        Canvas cursor = OverlayWindow.GetCanvas();

        FontLazer?.Push(); // or do "using (FontLazer?.Push()) draw.AddText()"
        float fontSize = ImGui.GetFontSize();
        string text = string.Empty;
        Vector2 position = new();

        // Background
        draw.AddRectFilled(cursor.Min, cursor.Max, Config.BackgroundColor);

        // Empty
        if (Data.Begin == null)
        {
            text = "Empty";
            position = cursor.Align(text, Canvas.HorizontalAlign.Center, Canvas.VerticalAlign.Center);
            draw.AddText(position, colorWhite, text); // outlined
            FontLazer?.Pop();
            return;
        }

        // Header
        cursor.Max = new(cursor.Max.X, cursor.Min.Y + fontSize + 2 * Config.Spacing);
        draw.AddRectFilled(cursor.Min, cursor.Max, Config.Color1);

        text = $"{Data.Name} - ({(!Data.Active ? "Completed in " : "")}{Data.Duration.ToString(@"mm\:ss")})";
        position = cursor.Align(text, Canvas.HorizontalAlign.Center, Canvas.VerticalAlign.Center);
        draw.AddText(position, colorWhite, text);
        cursor.Move((0, cursor.Height));

        // Players
        uint topDamage = 1;
        if (sortCache.Count != 0) topDamage = Data.Players.GetValueOrDefault(sortCache.First())!.DamageDealt.Value.Total;
        foreach (var id in sortCache)
        {
            Player player = Data.Players[id];
            Canvas line = new(cursor.Area);
            // DoLerpPlayerData(player);
            // lerpedInfo[player.Id];

            // Background
            draw.AddRectFilled(line.Min, line.Max, Config.Color1);

            // Bar
            var barColor = player.Id == MeterWay.Dalamud.ClientState.LocalPlayer?.ObjectId ? Config.Color2 : Config.Color3;
            float progress = (float)player.DamageDealt.Value.Total / topDamage;
            DrawProgressBar(line.Min, line.Max, barColor, progress);
            draw.AddRect(line.Min, line.Max, Config.Color4);

            // icon
            Canvas icon = new(line.Area);
            icon.Max = icon.Min + new Vector2(icon.Height, icon.Height);
            icon.Padding(Config.Spacing + 1);
            draw.AddRectFilled(icon.Min, icon.Max, colorBlack);
            draw.AddRect(icon.Min, icon.Max, Config.Color7);
            DrawJobIcon(icon, player.Job);
            line.AddMin(icon.Height + Config.Spacing, 0);

            // Texts
            text = $"{new Job(player.Job).Id}";
            position = line.Padding((Config.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
            draw.AddText(position, Config.Color5, text); // scale 0.8

            line.AddMin(ImGui.CalcTextSize(text).X + Config.Spacing, 0);

            text = player.Name;
            position = line.Padding((Config.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
            draw.AddText(position, colorWhite, text);
            line.AddMin(ImGui.CalcTextSize(text).X + Config.Spacing, 0);

            text = $"{Helpers.HumanizeNumber(player.PerSecounds.DamageDealt, 1)}/s";
            position = line.Padding((Config.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Right, Canvas.VerticalAlign.Center);
            draw.AddText(position, colorWhite, text);
            line.AddMax(-ImGui.CalcTextSize(text).X - Config.Spacing, 0);

            text = $"{Helpers.HumanizeNumber(player.DamageDealt.Value.Total, 1)}";
            position = line.Padding((Config.Spacing, 0)).Align(text, Canvas.HorizontalAlign.Right, Canvas.VerticalAlign.Center);
            draw.AddText(position, Config.Color6, text); // scale 0.8

            cursor.Move((0, cursor.Height));
        }
        FontLazer?.Pop();
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
}