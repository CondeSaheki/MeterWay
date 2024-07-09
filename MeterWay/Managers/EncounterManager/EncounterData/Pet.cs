using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;

using MeterWay.Utils;

namespace MeterWay.Data;

public class Pet : IPet
{
    public IPlayer? Owner {get;set;}

    public uint Id {get;set;}

    public string Name {get;set;}

    public Damage DamageDealt {get;set;}

    public Damage DamageReceived {get;set;}

    public Heal HealDealt {get;set;}

    public Heal HealReceived {get;set;}

    public PerSeconds PerSeconds {get;set;}

    public void Calculate()
    {
        throw new System.NotImplementedException();
    }
}