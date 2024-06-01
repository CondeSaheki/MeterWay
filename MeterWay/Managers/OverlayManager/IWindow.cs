
using System.Numerics;
using System.Collections.Generic;
using ImGuiNET;
using Dalamud.Interface.Windowing;

namespace MeterWay.Overlay;

/// <summary>
/// Interface created to allow MeterWay overlay creators to modify the Dalamud.Interface.Windowing.Window directly
/// </summary>
public interface IWindow
{
    string? Namespace { get; set; }
    string WindowName { get; set; }
    bool IsFocused { get; }
    bool RespectCloseHotkey { get; set; }
    bool DisableWindowSounds { get; set; }
    uint OnOpenSfxId { get; set; }
    uint OnCloseSfxId { get; set; }
    Vector2? Position { get; set; }
    ImGuiCond PositionCondition { get; set; }
    Vector2? Size { get; set; }
    ImGuiCond SizeCondition { get; set; }
    Window.WindowSizeConstraints? SizeConstraints { get; set; }
    bool? Collapsed { get; set; }
    ImGuiCond CollapsedCondition { get; set; }
    ImGuiWindowFlags Flags { get; set; }
    bool ForceMainWindow { get; set; }
    float? BgAlpha { get; set; }
    bool ShowCloseButton { get; set; }
    bool AllowPinning { get; set; }
    bool AllowClickthrough { get; set; }
    List<Window.TitleBarButton> TitleBarButtons { get; set; }
    bool IsOpen { get; set; }

    void Toggle();
    void BringToFront();

    // hided these, if needed can be added
    /*
        void PreOpenCheck();
        bool DrawConditions();
        void PreDraw();
        void PostDraw();
    */
}