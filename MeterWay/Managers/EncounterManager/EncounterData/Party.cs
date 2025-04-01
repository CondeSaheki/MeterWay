using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;
using MeterWay.Utils;

namespace MeterWay.Data;

public class Party : IParty
{
    private Encounter Encounter { get; init; }

    public uint Id { get; set; }
    public Dictionary<uint, Player> Players { get; set; }
    public Dictionary<uint, IPet> Pets { get; set; }

    IEncounter? IParty.Encounter => throw new System.NotImplementedException();

    Dictionary<uint, IPlayer> IParty.Players => throw new System.NotImplementedException();

    public Damage DamageDealt => throw new System.NotImplementedException();

    public Damage DamageReceived => throw new System.NotImplementedException();

    public Heal HealDealt => throw new System.NotImplementedException();

    public Heal HealReceived => throw new System.NotImplementedException();

    public PerSeconds PerSeconds => throw new System.NotImplementedException();


    public Party(Encounter encounter)
    {
        Encounter = encounter;
        Id = Helpers.CreateId();

        Players = Dalamud.Framework.RunOnTick(GetPlayers).ConfigureAwait(false).GetAwaiter().GetResult();
        Pets = [];
    }

    private Dictionary<uint, Player> GetPlayers()
    {
        var tmpPlayers = new Dictionary<uint, Player>();
        var partyList = Dalamud.PartyList;
        if (partyList.Count() != 0)
        {
            foreach (var player in partyList)
            {
                if (player.GameObject == null) continue;

                var character = (ICharacter)player.GameObject;
                tmpPlayers.Add(character.EntityId, new Player(character, Encounter));
            }
        }
        else
        {
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer != null)
            {
                var character = (ICharacter)localPlayer;
                tmpPlayers.Add(character.EntityId, new Player(character, Encounter));
            }
        }
        return tmpPlayers;
    }

    public void Update() // vREALVERDADEIROULTIMATE
    {
        if (Encounter.Finished) return;

        var partyList = Dalamud.PartyList;

        // keep only you
        if (partyList.Length == 0)
        {
            Dalamud.Framework.RunOnTick(Disband).ConfigureAwait(false).GetAwaiter().GetResult();
            return; // done
        }

        List<IPartyMember> newPlayers = [];
        List<uint> commonPlayers = [];

        // fill commonPlayers & newPlayers
        foreach (var player in partyList)
        {
            var tmpPlayerId = GetOrRecoverPlayer(player);
            if (tmpPlayerId == null) continue;

            if (!Players.ContainsKey((uint)tmpPlayerId))
            {
                newPlayers.Add(player);
                continue;
            }
            else if (Players[(uint)tmpPlayerId].IsActive == false)
            {
                Players[(uint)tmpPlayerId].IsActive = true;
            }
            commonPlayers.Add((uint)tmpPlayerId);
        }
        if (commonPlayers.Count + newPlayers.Count == 0) return; // all players are null

        // remove players not in common
        foreach (var player in Players)
        {
            if (!commonPlayers.Contains(player.Key))
            {
                if (!Encounter.Active)
                {
                    Players.Remove(player.Key);
                }
                else
                {
                    player.Value.IsActive = false;
                }
            }
            else
            {
                player.Value.Update();
            }
        }

        // add new players 
        foreach (var player in newPlayers)
        {
            var character = player.GameObject as ICharacter;
            if (character == null) continue; // error could not retrieve player

            Players.Add(character.EntityId, new Player(character, Encounter));
        }

        Id = Helpers.CreateId();
        Dalamud.Log.Info("Party Update, Done");
    }

    private uint? GetOrRecoverPlayer(IPartyMember player)
    {
        if (player.GameObject != null) return player.GameObject.EntityId; // no recover

        // attempt recover
        uint? recovered = null;
        foreach (KeyValuePair<uint, Player> cachedPlayer in Players)
        {
            if (cachedPlayer.Value.Name == player.Name.ToString() && cachedPlayer.Value.World == player.World.Value.RowId)
            {
                recovered = cachedPlayer.Key;
                break;
            }
        }
        return recovered;
    }

    private void Disband()
    {
        var you = Dalamud.ClientState.LocalPlayer;
        if (you == null) return; // you are null

        foreach (var player in Players)
        {
            if (player.Key != you.EntityId)
            {
                if (!Encounter.Active) Players.Remove(player.Key);
                else player.Value.IsActive = false;
            }
            else
            {
                player.Value.Update();
            }
        }

        Id = Helpers.CreateId();
        Dalamud.Log.Info("Party Disband, Done");
    }

    public bool HasChanged()
    {
        var partyList = Dalamud.PartyList;

        uint activeCount = 0;
        foreach (var player in Players) if (player.Value.IsActive) ++activeCount;

        // party change is evident
        if (partyList.Length != activeCount)
        {
            if (!(partyList.Length == 0 && activeCount == 1)) return true;
        }

        // party sizes are equal and need to be checked
        foreach (var player in partyList)
        {
            var recoveredId = GetOrRecoverPlayer(player);
            if (recoveredId == null) return true; // id did not got recovered
            if (!Players.ContainsKey((uint)recoveredId)) return true; // new player
        }
        return false;
    }

    public void Calculate()
    {
        foreach (var player in Players.Values) player.Calculate();
    }
}