
using System;
using System.Numerics;

using MeterWay.Utils;

namespace MeterWay.Overlay;

/// <summary>
/// Interface for overlay windows.
/// </summary>
public interface IOverlayWindow : IWindow, IDisposable
{
    /// <summary>
    /// Gets the comment for the overlay window.
    /// </summary>
    public string Comment { get; }

    /// <summary>
    /// Gets the current size of the overlay window. null until window gets draw.
    /// </summary>
    public Vector2? CurrentSize { get; }

    /// <summary>
    /// Gets the current position of the overlay window. null until window gets draw
    /// </summary>
    public Vector2? CurrentPosition { get; }

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