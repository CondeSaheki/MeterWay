using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

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

    public PerSecounds PerSecounds { get; set; } = new PerSecounds();
    
    public void Calculate()
    {
        DamageDealt.Calculate();
        DamageReceived.Calculate();
        HealDealt.Calculate();
        HealReceived.Calculate();
        
        PerSecounds.Calculate(DamageDealt, DamageReceived, HealDealt, HealReceived, Encounter);
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

            Dalamud.Log.Info("Player Updated");
            return true;
        }
        return false;
    }

    private PlayerCharacter? GetPlayer()
    {
        var partyList = Dalamud.PartyList;
        if (partyList.Length == 0)
        {
            var you = Dalamud.ClientState.LocalPlayer;
            if (you?.ObjectId == Id) return you;
        }

        var player = Dalamud.PartyList.FirstOrDefault(x => x?.ObjectId == Id, null)?.GameObject as PlayerCharacter;
        return player;
    }

}