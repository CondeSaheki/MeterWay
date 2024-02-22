using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

using MeterWay.LogParser;

namespace MeterWay.Data;

public class Player(Character character, Encounter encounter)
{
    private Encounter Encounter { get; init; } = encounter;

    public uint Id { get; set; } = character.ObjectId;
    public bool IsActive { get; set; } = true;

    public string Name { get; set; } = character.Name.ToString();
    public uint? World { get; set; } = (character as PlayerCharacter)?.HomeWorld.Id;
    public uint Job { get; set; } = character.ClassJob.Id;

    public DamageData Damage { get; set; } = new DamageData();
    public DamageData DamageTaken { get; set; } = new DamageData();

    public HealingData Healing { get; set; } = new HealingData();
    public HealingData HealingTaken { get; set; } = new HealingData();

    // calculated
    public float DamagePercent { get; set; } = 0;
    public float HealsPercent { get; set; } = 0;
    public float CritPercent { get; set; } = 0;
    public float DirecHitPercent { get; set; } = 0;
    public float DirectCritHitPercent { get; set; } = 0;
    public float Crithealspercent { get; set; } = 0;
    
    public float Dps { get; set; } = 0;
    public float Hps { get; set; } = 0;

    public void RecalculateData()
    {
        var seconds = Encounter.Duration.TotalSeconds <= 1 ? 1 : Encounter.Duration.TotalSeconds; // overflow protection

        this.Dps = (float)(Damage.Total / seconds);
        this.Hps = (float)(Healing.Total / seconds);

        this.DamagePercent = Encounter.Damage.Total != 0 ? (this.Damage.Total * 100 / Encounter.Damage.Total) : 0;
        this.HealsPercent = Encounter.Healing.Total != 0 ? (this.Healing.Total * 100 / Encounter.Healing.Total) : 0;

        this.CritPercent = Encounter.Damage.Count.Hit != 0 ? (this.Damage.Count.Crit * 100 / Encounter.Damage.Count.Hit) : 0;
        this.DirecHitPercent = Encounter.Damage.Count.Hit != 0 ? (this.Damage.Count.Dh * 100 / Encounter.Damage.Count.Hit) : 0;
        this.DirectCritHitPercent = Encounter.Damage.Count.Hit != 0 ? (this.Damage.Count.CritDh * 100 / Encounter.Damage.Count.Hit) : 0;
        this.Crithealspercent = Encounter.Healing.Count.Hit != 0 ? (this.Healing.Count.Crit * 100 / Encounter.Healing.Count.Hit) : 0;
    }
}