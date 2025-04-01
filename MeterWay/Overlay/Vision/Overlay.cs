using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using ImGuiNET;
using Dalamud.Interface.ManagedFontAtlas;

using MeterWay.Utils;
using MeterWay.Utils.Format;
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
    private IFontHandle FontVision { get; set; }
    private IFontHandle DefaultFont => FontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(20)));

    private Encounter? Data;

    private uint? PlayerId { get; set; }

    private CancellationTokenSource? DelayToken { get; set; }

    private Format VisionFormat { get; set; }

    public Overlay(IOverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        Config = Load<Configuration>(Window.WindowName);

        Init();
        FontVision ??= DefaultFont;

        VisionFormat = new(Config.Appearance.FormatString);
    }

    void Init()
    {
        if (Config.General.NoInput) Window.Flags |= ImGuiWindowFlags.NoInputs;
        if (Config.General.NoMove) Window.Flags |= ImGuiWindowFlags.NoMove;
        if (Config.General.NoResize) Window.Flags |= ImGuiWindowFlags.NoResize;

        Window.IsOpen = Config.Visibility.Enabled || Config.Visibility.Always;

        Window.SetSize(Config.General.Size);
        Window.SetPosition(Config.General.Position);

        if (Config.Font.VisionFontSpec != null) FontVision = Config.Font.VisionFontSpec.CreateFontHandle(FontAtlas);
    }

    public override void OnEncounterUpdate()
    {
        Data = EncounterManager.Inst.CurrentEncounter();
        if (!Config.General.FrameCalc) Data.Calculate();
    }

    private void UpdatePlayerId()
    {
        if (MeterWay.Dalamud.ClientState.LocalPlayer != null) PlayerId = MeterWay.Dalamud.ClientState.LocalPlayer.EntityId;
    }

    public override void Draw()
    {
        var draw = ImGui.GetWindowDrawList();
        Canvas cursor = Window.GetCanvas();
        FontVision?.Push();

        if (Data == null)
        {
            draw.AddRect(cursor.Min, cursor.Max, Config.Font.VisionFontColor);
            var dataText = $"{Info.Name}, No Data.";
            var dataPosition = cursor.Align(dataText, Canvas.HorizontalAlign.Center, Canvas.VerticalAlign.Center);
            draw.AddText(dataPosition, Config.Font.VisionFontColor, dataText); // outlined
            FontVision?.Pop();
            return;
        }
        if (PlayerId == null)
        {
            MeterWay.Dalamud.Framework.RunOnTick(UpdatePlayerId).ConfigureAwait(false).GetAwaiter().GetResult();
            FontVision?.Pop();
            return;
        }
        if (!Data.Players.TryGetValue((uint)PlayerId, out Player? player))
        {
            FontVision?.Pop();
            return;
        }

        if (Config.General.FrameCalc && !Data.Finished && Data.Active) Data.Calculate(); // this will ignore last frame data ??

        var text = VisionFormat.Original;
        List<string> replaces = [];

        foreach (var info in VisionFormat.Infos)
        {
            if (info.PlaceHolder == null || info.PlaceHolder?.Function == null)
            {
                replaces.Add(info.Target);
                continue;
            }

            object? returnObject = info.PlaceHolder.ArgumentType?.Name switch
            {
                "IEncounter" => info.PlaceHolder.Get(Data),
                // IParty => Data,
                "IPlayer" => info.PlaceHolder.Get(player),
                // IPet => Data,
                _ => "{Not Supported by Overlay}"
            };

            string result = returnObject switch
            {
                string returnString => returnString,
                uint returnUint => Helpers.HumanizeNumber(returnUint, 2),
                float returnFloat => returnFloat.ToString("F2", CultureInfo.InvariantCulture),
                double returnDouble => Helpers.HumanizeNumber(returnDouble, 2),
                DateTime returnDateTime => returnDateTime.ToString(),
                TimeSpan returnTimeSpan => returnTimeSpan.ToString(@"mm\:ss"),
                _ => string.Empty
            };
            replaces.Add(result);
        }
        text = VisionFormat.Build(replaces);


        var textsize = ImGui.CalcTextSize(text);
        cursor = new Canvas((cursor.Min, new(cursor.Min.X + textsize.X, cursor.Min.Y + textsize.Y)));
        cursor.Padding(-2f);

        if (Config.Appearance.BackgroundEnable) draw.AddRectFilled(cursor.Min, cursor.Max, Config.Appearance.BackgroundColor);

        var position = cursor.Padding((6f, 4f)).Align(text, Canvas.HorizontalAlign.Left, Canvas.VerticalAlign.Center);
        draw.AddText(position, Config.Font.VisionFontColor, text);
        FontVision?.Pop();
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
        MeterWay.Dalamud.Framework.RunOnTick(UpdatePlayerId).ConfigureAwait(false).GetAwaiter().GetResult();
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
        FontVision?.Dispose();
        FontAtlas?.Dispose();
    }
}