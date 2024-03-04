using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using MeterWay.Managers;
using MeterWay.Utils;

namespace MeterWay.Data;

public class EncounterParty
{
    private Encounter Encounter { get; init; }

    public uint Id { get; set; }
    public Dictionary<uint, Player> Players { get; set; }
    public Dictionary<uint, uint> Pets { get; set; }

    public EncounterParty(Encounter encounter)
    {
        Encounter = encounter;
        Id = Helpers.CreateId();

        Players = GetPlayers();
        Pets = [];
    }

    private Dictionary<uint, Player> GetPlayers()
    {
        var tmpPlayers = new Dictionary<uint, Player>();
        var partyList = InterfaceManager.Inst.PartyList;
        if (partyList.Count() != 0)
        {
            foreach (var player in partyList)
            {
                if (player.GameObject == null) continue;

                var character = (Character)player.GameObject;
                tmpPlayers.Add(character.ObjectId, new Player(character, Encounter));
            }
        }
        else
        {
            var localPlayer = InterfaceManager.Inst.ClientState.LocalPlayer;
            if (localPlayer != null)
            {
                var character = (Character)localPlayer;
                tmpPlayers.Add(character.ObjectId, new Player(character, Encounter));
            }
        }
        return tmpPlayers;
    }

    public void Update() // vREALVERDADEIROULTIMATE
    {
        if (Encounter.Finished) return;
        
        var partyList = InterfaceManager.Inst.PartyList;

        // keep only you
        if (partyList.Length == 0)
        {
            Disband();
            return; // done
        }

        List<PartyMember> newPlayers = [];
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
            var character = player.GameObject as Character;
            if (character == null) continue; // error could not retrieve player

            Players.Add(character.ObjectId, new Player(character, Encounter));
        }

        Id = Helpers.CreateId();
        Helpers.Log("Party updated");
    }

    private uint? GetOrRecoverPlayer(PartyMember player)
    {
        if (player.GameObject != null) return player.GameObject.ObjectId; // no recover

        // attempt recover
        uint? recovered = null;
        foreach (KeyValuePair<uint, Player> cachedPlayer in Players)
        {
            if (cachedPlayer.Value.Name == player.Name.ToString() && cachedPlayer.Value.World == player.World.Id)
            {
                recovered = cachedPlayer.Key;
                break;
            }
        }
        return recovered;
    }

    private void Disband()
    {
        var you = InterfaceManager.Inst.ClientState.LocalPlayer;
        if (you == null) return; // you are null

        foreach (var player in Players)
        {
            if (player.Key != you.ObjectId)
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
        Helpers.Log("Party updated");
    }

    public bool HasChanged()
    {
        var partyList = InterfaceManager.Inst.PartyList;

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