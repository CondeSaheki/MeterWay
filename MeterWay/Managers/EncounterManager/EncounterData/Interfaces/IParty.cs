using System.Collections.Generic;

namespace MeterWay.Data;

public interface IParty : IData
{
    public IEncounter? Encounter { get; }
    public uint Id { get; }

    public Dictionary<uint, IPlayer> Players { get; }
    public Dictionary<uint, IPet> Pets { get; }
}