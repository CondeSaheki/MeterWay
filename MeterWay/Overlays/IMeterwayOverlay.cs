namespace MeterWay.Overlays;

public interface IMeterwayOverlay
{
    public string Name { get; }
    public void Draw();
    public void Dispose();
    public void DataProcess();
}
