using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

using Meterway.Managers;
using Dalamud.Game.ClientState.Party;
using MeterWay.Utils;
using Dalamud.Plugin.Services;

namespace MeterWay.Data;

public class Encounter
{
    public uint Id { get; set; }
    public string Name { get; set; }

    public bool Active => Start != null && End == null;
    public bool Finished => Start != null && End != null;

    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public TimeSpan Duration => _Duration();

    public List<INetworkMessage> RawActions { get; set; }

    public uint PartyListId { get; set; }
    public Dictionary<uint, Player> Players { get; set; }
    public Dictionary<uint, uint> Pets { get; set; }

    public Int64 TotalDamage { get; set; }
    public float Dps { get; set; }

    // constructor
    public Encounter()
    {
        this.Id = CreateId();
        this.PartyListId = CreateId();
        this.Name = GetEncounterName();
        this.Players = GetPlayers();
        this.Pets = new Dictionary<uint, uint>();
        this.TotalDamage = 0;
        this.Dps = 0;
        this.RawActions = new List<INetworkMessage>();
    }

    private uint CreateId()
    {
        return (uint)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
    }

    private Dictionary<uint, Player> GetPlayers()
    {
        var tmpPlayers = new Dictionary<uint, Player>();
        var partyList = PluginManager.Instance.PartyList;
        if (partyList.Count() != 0)
        {
            foreach (var player in partyList)
            {
                if (player.GameObject == null) continue;

                var character = (Character)player.GameObject;
                tmpPlayers.Add(character.ObjectId, new Player(character, this));
            }
        }
        else
        {
            var localPlayer = PluginManager.Instance.ClientState.LocalPlayer;
            if (localPlayer != null)
            {
                var character = (Character)localPlayer;
                tmpPlayers.Add(character.ObjectId, new Player(character, this));
            }
        }
        return tmpPlayers;
    }

    public void UpdateParty() // vREALVERDADEIROULTIMATE
    {
        Helpers.Log("Party update got triggred");
        var partyList = PluginManager.Instance.PartyList;

        if (!PartyMembersChanged(partyList))
        {
            Helpers.Log("party did not change, player got a DC or changed area");
            return;
        }
        Helpers.Log("Party has changed");

        // keep only you
        if (partyList.Count() == 0)
        {    
            var you = PluginManager.Instance.ClientState.LocalPlayer;
            if (you == null) return; // you are null

            foreach (var player in Players)
            {
                if (player.Key != you.ObjectId)
                {
                    if (!this.Active) Players.Remove(player.Key);
                    else player.Value.IsActive = false;
                }
            }
            this.PartyListId = CreateId();
            Helpers.Log("Party updated");
            return; // done
        }

        List<PartyMember> newPlayers = new List<PartyMember>();
        List<uint> communPlayers = new List<uint>();

        // fill communPlayers & newPlayers
        foreach (var player in partyList)
        {
            var tmpPlayerId = GetPartyMemberOrRecoverId(player);
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
            communPlayers.Add((uint)tmpPlayerId);
        }
        if (communPlayers.Count + newPlayers.Count == 0) return; // all players are null

        // remove players not in common
        foreach (var player in Players)
        {
            if (!communPlayers.Contains(player.Key))
            {
                if (!this.Active)
                {
                    Players.Remove(player.Key);
                }
                else
                {
                    player.Value.IsActive = false;
                }
            }
        }

        // add new players 
        foreach (var player in newPlayers)
        {
            var character = player.GameObject as Character;
            if (character == null) continue; // error could not retrieve player

            Players.Add(character.ObjectId, new Player(character, this));
        }
        Helpers.Log("Party updated");
        this.PartyListId = CreateId();
    }

    public uint? GetPartyMemberOrRecoverId(PartyMember player)
    {
        if (player.GameObject != null) return player.GameObject.ObjectId; // no recover

        // atempt recover
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

    public bool PartyMembersChanged(IPartyList partyList)
    {
        uint activeCount = 0;
        foreach (var player in Players) if (player.Value.IsActive) ++activeCount;

        // party change is evident
        if (partyList.Count() != activeCount)
        {
            if (!(partyList.Count() == 0 && activeCount == 1)) return true;
        }

        // party sizes are equal and need to be checked
        foreach (var player in partyList)
        {
            var recoveredId = GetPartyMemberOrRecoverId(player);
            if (recoveredId == null) return true; // id did not got recovered
            if (!Players.ContainsKey((uint)recoveredId)) return true; // new player
        }
        return false;
    }

    private string GetEncounterName()
    {
        var locationRow = PluginManager.Instance.DataManager.GetExcelSheet<TerritoryType>()?.GetRow(PluginManager.Instance.ClientState.TerritoryType);
        var instanceContentName = locationRow?.ContentFinderCondition.Value?.Name?.ToString();
        var placeName = locationRow?.PlaceName.Value?.Name?.ToString();

        return (string.IsNullOrEmpty(instanceContentName) ? placeName : instanceContentName) ?? "";
    }

    private TimeSpan _Duration()
    {
        if (Start == null) return new TimeSpan(0);
        if (End == null) return (TimeSpan)(DateTime.Now - Start);
        return (TimeSpan)(End - Start);
    }

    public void StartEncounter()
    {
        PluginManager.Instance.PluginLog.Info("Encounter started.");
        if (this.Start == null) this.Start = DateTime.Now;
    }

    public void EndEncounter()
    {
        PluginManager.Instance.PluginLog.Info("Encounter ended.");
        if (this.End == null) this.End = DateTime.Now;
    }

    public void UpdateStats()
    {
        var currentSeconds = this.Duration.TotalSeconds <= 1 ? 1 : this.Duration.TotalSeconds;
        this.Dps = (float)(this.TotalDamage / currentSeconds);
        foreach (var player in this.Players.Values)
        {
            player.UpdateStats();
        }
    }

}

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

    public List<INetworkMessage> RawActions { get; set; }

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
        this.RawActions = new List<INetworkMessage>();
        this.World = (character as PlayerCharacter)?.HomeWorld.Id;
    }
}
