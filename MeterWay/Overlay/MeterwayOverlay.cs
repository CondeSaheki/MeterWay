using System;
using System.Collections.Generic;

namespace MeterWay.Overlay;

public interface IOverlay : IDisposable
{
    public void Draw();
    public void DataUpdate();
    public void Remove();
}

public interface IOverlayConfig
{
    public void DrawConfig();
}

public interface IOverlayCommandHandler
{
    public string CommandHelpMessage(string? command);
    public Action? OnCommand(List<string> args);
}

public interface IConfiguration
{
    int Version { get; set; }
}