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
    public float CritPercent { get; set; } = 0;
    public float DirecHitPercent { get; set; } = 0;
    public float DirectCritHitPercent { get; set; } = 0;
    public float Crithealspercent { get; set; } = 0;
    
    public float Dps { get; set; } = 0;

    public void RecalculateData()
    {
        var seconds = Encounter.Duration.TotalSeconds <= 1 ? 1 : Encounter.Duration.TotalSeconds; // overflow protection

        Dps = (float)(Damage.Total / seconds);
        DamagePercent = Encounter.Damage.Total != 0 ? (Damage.Total * 100 / Encounter.Damage.Total) : 0;

        CritPercent = Encounter.DamageCount.Hit != 0 ? (Encounter.DamageCount.Crit * 100 / Encounter.DamageCount.Hit) : 0;
        DirecHitPercent = Encounter.DamageCount.Hit != 0 ? (Encounter.DamageCount.Dh * 100 / Encounter.DamageCount.Hit) : 0;
        DirectCritHitPercent = Encounter.DamageCount.Hit != 0 ? (Encounter.DamageCount.CritDh * 100 / Encounter.DamageCount.Hit) : 0;
    }
}