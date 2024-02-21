using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

using MeterWay.LogParser;

namespace MeterWay.Data;

public class Player
{
    private Encounter Encounter { get; init; }

    public uint Id { get; set; }
    public bool IsActive { get; set; }    

    public string Name { get; set; }
    public uint? World { get; set; }
    public uint Job { get; set; }

    public uint TotalDamage { get; set; }
    public float Dps { get; set; }
    public float DamagePercentage { get; set; }

    public List<LogLine> RawActions { get; set; }

    public void UpdateStats()
    {
        var currentSeconds = this.Encounter.Duration.TotalSeconds <= 1 ? 1 : this.Encounter.Duration.TotalSeconds;
        this.Dps = (float)(this.TotalDamage / currentSeconds);
        this.DamagePercentage = this.Encounter.TotalDamage != 0 ? (float)((this.TotalDamage * 100) / this.Encounter.TotalDamage) : 0;
    }

    public Player(Character character, Encounter encounter)
    {
        this.Encounter = encounter;
        this.Id = character.ObjectId;
        this.Name = character.Name.ToString();
        this.Job = character.ClassJob.Id;
        this.Dps = 0;
        this.TotalDamage = 0;
        this.DamagePercentage = 0;
        this.IsActive = true;
        this.RawActions = new List<LogLine>();
        this.World = (character as PlayerCharacter)?.HomeWorld.Id;
    }
}
