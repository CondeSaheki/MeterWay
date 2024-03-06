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

    public Damage DamageDealt { get; set; } = new Damage();
    public Damage DamageReceived { get; set; } = new Damage();

    public Heal HealDealt { get; set; } = new Heal();
    public Heal HealReceived { get; set; } = new Heal();

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

        Dps = (float)(DamageDealt.Total / seconds);
        Hps = (float)(HealDealt.Total / seconds);

        static float Protected(uint first, uint second) => second != 0 ? (first * 100 / second) : 0;

        DamagePercent = Protected(DamageDealt.Total, Encounter.DamageDealt.Total);
        HealsPercent = Protected(HealDealt.Total, Encounter.HealDealt.Total);

        CritPercent = Protected(DamageDealt.Count.Critical, Encounter.DamageDealt.Count.Total);
        DirecHitPercent = Protected(DamageDealt.Count.Direct, Encounter.DamageDealt.Count.Total);
        DirectCritHitPercent = Protected(DamageDealt.Count.CriticalDirect, Encounter.DamageDealt.Count.Total);
        Crithealspercent = Protected(HealDealt.Count.Critical, Encounter.HealDealt.Count.Total);
    }

    public bool Update()
    {
        if (!IsActive) return false;

        var player = GetPlayer();
        if (player == null) return false;

        var tmpName = player.Name.ToString();
        var tmpWorld = player.HomeWorld.Id;
        var tmpJob = player.ClassJob.Id;
        if (Name != tmpName || World != tmpWorld || Job != tmpJob)
        {
            Name = player.Name.ToString();
            World = player.HomeWorld.Id;
            Job = player.ClassJob.Id;
            Id = Helpers.CreateId();

            Helpers.Log("Player Updated");
            return true;
        }
        return false;
    }

    private PlayerCharacter? GetPlayer()
    {
        var partyList = InterfaceManager.Inst.PartyList;
        if (partyList.Length == 0)
        {
            var you = InterfaceManager.Inst.ClientState.LocalPlayer;
            if (you == null || you.ObjectId != Id) return null;
            return you;
        }
        else
        {
            var obj = InterfaceManager.Inst.PartyList.First(x => x.ObjectId == Id).GameObject;
            if (obj == null) return null;
            return (PlayerCharacter)obj;
        }
    }

}