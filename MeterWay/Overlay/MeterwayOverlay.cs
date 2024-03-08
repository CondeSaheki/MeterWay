using System;

namespace MeterWay.Overlay;

public abstract class MeterWayOverlay
{
    public static string Name => string.Empty;
    public virtual string _Name() => Name;
    public abstract void Draw();
    public abstract void Dispose();
    public abstract void DataProcess();
    public virtual void DrawConfigurationTab() { }
    public virtual bool HasConfigurationTab => false;
}

public class MeterWayOverlayConfiguration : Attribute { }