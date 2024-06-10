namespace MeterWay.Data;

public interface IPet : IData
{
    public IPlayer? Owner { get; }
    public uint Id { get; }
    
    public string Name { get; }
}