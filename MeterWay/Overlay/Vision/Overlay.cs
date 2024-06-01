using System;
using System.Threading;
using System.Globalization;
using ImGuiNET;
using Dalamud.Interface.ManagedFontAtlas;

using MeterWay.Utils;
using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;

namespace Vision;

public partial class Overlay : BasicOverlay
{
    public static BasicOverlayInfo Info => new()
    {
        Name = "Vision",
        Author = "MeterWay",
        // Description = "",
    };
    
    private IOverlayWindow Window { get; init; }
    private Configuration Config { get; set; }

    private IFontAtlas FontAtlas { get; init; } = MeterWay.Dalamud.PluginInterface.UiBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async);
    private IFontHandle FontMogu { get; set; }
    private IFontHandle DefaultFont => FontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(20)));

    private Encounter? Data;

    private uint? PlayerId { get; set; }

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
        Data = EncounterManager.Inst.CurrentEncounter();
        if (!Config.General.FrameCalc) Data.Calculate();
    }

    private void UpdatePlayerId()
    {
        if (MeterWay.Dalamud.ClientState.LocalPlayer != null) PlayerId = MeterWay.Dalamud.ClientState.LocalPlayer.ObjectId;
    }

    public override void Draw()
    {
        if (Data == null) return;
        if (Config.General.FrameCalc && !Data.Finished && Data.Active) Data.Calculate(); // this will ignore last frame data ??

        if (PlayerId == null) { UpdatePlayerId(); return; }
        if (!Data.Players.TryGetValue((uint)PlayerId, out Player? player)) return;

        var draw = ImGui.GetWindowDrawList();
        Canvas cursor = Window.GetCanvas();
        FontMogu?.Push();

        var text = ReplacePlaceholders(Config.Appearance.Format, player, Data, (obj) =>
        {
            return obj switch
            {
                string stringValue => stringValue,
                uint uintValue => $"{Helpers.HumanizeNumber(uintValue, 2)}",
                int intValue => $"{Helpers.HumanizeNumber(intValue, 2)}",
                double doubleValue => $"{doubleValue.ToString("F2", CultureInfo.InvariantCulture)}",
                float floatValue => $"{floatValue.ToString("F2", CultureInfo.InvariantCulture)}",
                DateTime dateTimeValue => $"{dateTimeValue}",
                TimeSpan dateTimeValue => $"{dateTimeValue.ToString(@"mm\:ss")}",
                _ => "Empty"
            };
        });
        var textsize = ImGui.CalcTextSize(text);
        cursor = new Canvas((cursor.Min, new(cursor.Min.X + textsize.X, cursor.Min.Y + textsize.Y)));
        cursor.Padding(-2f);

        if (Config.Appearance.BackgroundEnable) draw.AddRectFilled(cursor.Min, cursor.Max, Config.Appearance.BackgroundColor);

        var position = cursor.Padding((6f, 4f)).Align(text, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
        draw.AddText(position, Config.Font.MoguFontColor, text);
        FontMogu?.Pop();
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
        UpdatePlayerId();
        DelayToken?.Cancel();
        if (Config.Visibility.Always || Config.Visibility.Combat)
        {
            Window.IsOpen = true;
            return;
        }
    }

    public override void Remove()
    {
        Delete(Window.WindowName);
    }

    public override void Dispose()
    {
        DelayToken?.Cancel();
        DelayToken?.Dispose();
        FontMogu?.Dispose();
        FontAtlas?.Dispose();
    }
}