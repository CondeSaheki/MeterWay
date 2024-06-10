using MeterWay.Utils;

namespace MeterWay.Data;

public interface IPlayer : IData
{
    public IParty? Party { get; }
    public uint Id { get; }

    public string Name { get; }
    public uint? World { get; }
    public Job Job { get; }
}