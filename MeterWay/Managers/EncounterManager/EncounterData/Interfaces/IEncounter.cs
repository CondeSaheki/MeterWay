using System;
using System.Collections.Generic;

namespace MeterWay.Data;

public interface IEncounter : IData
{
    public uint Id { get; }
    public string Name { get; }

    public bool Active { get; }
    public bool Finished { get; }

    public DateTime? Begin { get; }
    public DateTime? End { get; }
    public TimeSpan Duration { get; }

    public Dictionary<uint, IParty> Partys { get; }
    public Dictionary<uint, IPlayer> Players { get; }
    public Dictionary<uint, IPet> Pets { get; set; }
}