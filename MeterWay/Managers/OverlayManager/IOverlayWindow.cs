
using System;
using System.Numerics;
using System.Collections.Generic;
using ImGuiNET;
using Dalamud.Interface.Windowing;

using MeterWay.Utils;

namespace MeterWay.Overlay;

/// <summary>
/// Interface for overlay windows.
/// </summary>
public interface IOverlayWindow : IWindow, IDisposable
{
    /// <summary>
    /// Gets the id of the overlay window, no overlays have the same id.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets the name + id of the overlay window.
    /// </summary>
    public string NameId { get; }

    /// <summary>
    /// Gets the comment for the overlay window.
    /// </summary>
    public string Comment { get; }

    /// <summary>
    /// Gets the current size of the overlay window.
    /// </summary>
    public Vector2 CurrentSize { get; }

    /// <summary>
    /// Gets the current position of the overlay window.
    /// </summary>
    public Vector2 CurrentPos { get; }

    /// <summary>
    /// Set overlay window size
    /// </summary>
    /// <param name="size"></param>
    public void SetSize(Vector2 size);

    /// <summary>
    /// Set overlay window position
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Vector2 position);

    /// <summary>
    /// Retrieves the current canvas area for the ImGui window.
    /// Calculates the minimum and maximum coordinates of the content region and returns them as a <see cref="Canvas"/> object.
    /// </summary>
    /// <returns>A <see cref="Canvas"/> object representing the current window's content region.</returns>
    public Canvas GetCanvas();
}

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

    // hided these, if needed can be added back
    /*
        void PreOpenCheck();
        bool DrawConditions();
        void PreDraw();
        void PostDraw();
        void OnOpen();
        void OnClose();
        void Update();
    */
}