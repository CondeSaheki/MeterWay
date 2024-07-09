using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;

using MeterWay.Utils;

namespace MeterWay.Data;

public class Pet : IPet
{
    public IPlayer? Owner { get; set; } = null;

    public uint Id {get;set;}

    public string Name {get;set;} = string.Empty;

    public Damage DamageDealt { get; set; } = new();

    public Damage DamageReceived { get; set; } = new();

    public Heal HealDealt { get; set; } = new();

    public Heal HealReceived { get; set; } = new();

    public PerSeconds PerSeconds { get; set; } = new();

    public void Calculate()
    {
        throw new System.NotImplementedException();
    }
}