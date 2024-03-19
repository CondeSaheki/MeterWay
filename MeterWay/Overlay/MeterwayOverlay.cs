using System;

namespace MeterWay.Overlay;

public interface IOverlay : IDisposable
{
    public void Draw();
    public void DataUpdate();
}

public interface IOverlayTab
{
    public void DrawTab();
}

public interface IConfiguration
{
    int Version { get; set; }
}