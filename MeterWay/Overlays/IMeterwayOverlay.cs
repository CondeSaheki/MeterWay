namespace MeterWay.Overlays;

public abstract class MeterwayOverlay
{
    public static string Name() => string.Empty;
    public abstract void Draw();
    public abstract void Dispose();
    public abstract void DataProcess();
}

public interface IMeterwayOverlayConfig
{
    public void Open();
    public void Close();
}