using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;

using MeterWay.Utils;

namespace MeterWay.Data;

public class Pet : IPet
{
    public IPlayer? Owner => throw new System.NotImplementedException();

    public uint Id => throw new System.NotImplementedException();

    public string Name => throw new System.NotImplementedException();

    public Damage DamageDealt => throw new System.NotImplementedException();

    public Damage DamageReceived => throw new System.NotImplementedException();

    public Heal HealDealt => throw new System.NotImplementedException();

    public Heal HealReceived => throw new System.NotImplementedException();

    public PerSeconds PerSeconds => throw new System.NotImplementedException();

    public void Calculate()
    {
        throw new System.NotImplementedException();
    }
}