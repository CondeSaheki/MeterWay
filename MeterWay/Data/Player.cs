using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using MeterWay.Managers;
using MeterWay.Utils;

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

    public void Calculate()
    {
        var seconds = Encounter.Duration.TotalSeconds <= 1 ? 1 : Encounter.Duration.TotalSeconds;

        Dps = (float)(Damage.Total / seconds);
        Hps = (float)(Healing.Total / seconds);

        static float Protected(uint first, uint second) => second != 0 ? (first * 100 / second) : 0;

        DamagePercent = Protected(Damage.Total, Encounter.Damage.Total);
        HealsPercent = Protected(Healing.Total, Encounter.Healing.Total);

        CritPercent = Protected(Damage.Count.Crit, Encounter.Damage.Count.Hit);
        DirecHitPercent = Protected(Damage.Count.Dh, Encounter.Damage.Count.Hit);
        DirectCritHitPercent = Protected(Damage.Count.CritDh, Encounter.Damage.Count.Hit);
        Crithealspercent = Protected(Healing.Count.Crit, Encounter.Healing.Count.Hit);
    }

    public bool Update()
    {
        if(!IsActive) return false;
        
        var player = InterfaceManager.Inst.PartyList.First(x => x.ObjectId == Id);
        if (player == null) return false;

        var tmpName = player.Name.ToString();
        var tmpWorld = player.World.Id;
        var tmpJob = player.ClassJob.Id;
        if (Name != tmpName || World != tmpWorld || Job != tmpJob)
        {
            Name = player.Name.ToString();
            World = player.World.Id;
            Job = player.ClassJob.Id;
            Id = Helpers.CreateId();

            Helpers.Log("Player Updated");
            return true;
        }
        return false;
    }
}