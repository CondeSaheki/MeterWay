namespace MeterWay.Overlays;

public abstract class IMeterwayOverlay
{
    public static string Name() => string.Empty;
    public virtual string _Name() => Name();
    public abstract void Draw();
    public abstract void Dispose();
    public abstract void DataProcess();
}

public interface IMeterwayOverlayConfig
{
    public void Open();
    public void Close();
}
