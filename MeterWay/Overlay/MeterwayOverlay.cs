using System;
using System.Collections.Generic;

namespace MeterWay.Overlay;

/// <summary>
/// Interface representing an overlay with basic methods.
/// </summary>
public interface IOverlay : IDisposable
{
    /// <summary>
    /// Method to render the overlay.
    /// This method is responsible for drawing the overlay's content using ImGui.
    /// </summary>
    void Draw();

    /// <summary>
    /// Method to update the encounter data, called periodically.
    /// This method fetch and update any data that the overlay depends on.
    /// </summary>
    void DataUpdate();

    /// <summary>
    /// Method to remove the configuration file.
    /// This method handle the cleanup of any persistent data like configuration associated with the overlay.
    /// </summary>
    void Remove();
}

/// <summary>
/// Interface representing the configuration UI for an overlay.
/// </summary>
public interface IOverlayConfig
{
    /// <summary>
    /// Method to render the configuration window.
    /// This method is responsible for drawing the configuration UI using ImGui.
    /// </summary>
    void DrawConfig();
}

public interface IOverlayCommandHandler
{
    public string CommandHelpMessage(string? command);
    public Action? OnCommand(List<string> args);
}

/// <summary>
/// Interface representing the configuration for an overlay.
/// </summary>
public interface IConfiguration
{
    /// <summary>
    /// Gets or sets the version of the configuration.
    /// This property is used to keep track of the configuration schema version,
    /// allowing for proper handling of updates and migrations.
    /// </summary>
    int Version { get; set; }
}