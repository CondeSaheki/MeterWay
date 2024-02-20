using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

using Meterway.Managers;
using Dalamud.Game.ClientState.Party;
using MeterWay.Utils;

namespace MeterWay.Data;

public class Encounter
{
    public uint id { get; set; }
    public string Name { get; set; }

    public bool active => Start != null && End == null;
    public bool finished => Start != null && End != null;

    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public TimeSpan duration => Duration();

    public List<INetworkMessage> RawActions { get; set; }

    public uint partyListId { get; set; }

    public Dictionary<uint, Player> Players { get; set; }

    public Dictionary<uint, uint> Pets { get; set; }

    public Int64 TotalDamage { get; set; }
    public float DPS { get; set; }

    // constructor
    public Encounter()
    {
        this.id = CreateId();
        this.partyListId = CreateId();
        this.Name = GetEncounterName();
        this.Players = GetPlayers();
        this.Pets = new Dictionary<uint, uint>();
        this.TotalDamage = 0;
        this.DPS = 0;
        this.RawActions = new List<INetworkMessage>();
    }

    private uint CreateId()
    {
        return (uint)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
    }

    private Dictionary<uint, Player> GetPlayers()
    {
        var playerstemp = new Dictionary<uint, Player>();
        if (PluginManager.Instance.PartyList.Length != 0)
        {
            foreach (var player in PluginManager.Instance.PartyList)
            {
                if (player.GameObject == null) continue;

                var character = (Character)player.GameObject;
                playerstemp.Add(character.ObjectId, new Player(character, this));
            }
        }
        else
        {
            if (PluginManager.Instance.ClientState.LocalPlayer != null)
            {
                var character = (Character)PluginManager.Instance.ClientState.LocalPlayer;
                playerstemp.Add(character.ObjectId, new Player(character, this));
            }
        }
        return playerstemp;
    }

    public void UpdateParty()
    {
        PluginManager.Instance.PluginLog.Info("Party has changed, triggering update.");

        var partyList = PluginManager.Instance.PartyList;

        List<uint> playersToRemove = this.Players.Keys.ToList().Where(p => !partyList.ToList().Select(p => p.ObjectId).Contains(p)).ToList();
        List<uint> playersToAdd = partyList.ToList().Select(p => p.ObjectId).Where(p => !this.Players.ContainsKey(p)).ToList();

        Helpers.Log("Players to remove: " + String.Join(", ", playersToRemove));
        Helpers.Log("Players to add: " + String.Join(", ", playersToAdd));

        if (PluginManager.Instance.ClientState.LocalPlayer != null && partyList.Length == 0)
        {
            if (!this.Players.ContainsKey((PluginManager.Instance.ClientState.LocalPlayer as Character).ObjectId))
            {
                playersToAdd.Add((PluginManager.Instance.ClientState.LocalPlayer as Character).ObjectId);
            }
            playersToRemove.Remove((PluginManager.Instance.ClientState.LocalPlayer as Character).ObjectId);
        }

        // step 2 - add new players and update existing ones
        foreach (var player in playersToAdd)
        {
            var newPlayer = partyList.Where(p => p.ObjectId == player).Select(p => p).FirstOrDefault();

            Helpers.Log("New player: " + newPlayer);
            if (newPlayer != null)
            {
                var character = newPlayer.GameObject as Character;

                Helpers.Log("Player character: " + character);
                if (character != null)
                {
                    var existingNewPlayer = this.Players.Where(p => p.Value.Name == character.Name.ToString() && p.Value.World == (character as PlayerCharacter)?.HomeWorld.Id).Select(p => p.Value).FirstOrDefault();

                    if (existingNewPlayer != null)
                    {
                        this.Players.Remove(existingNewPlayer.Id);

                        existingNewPlayer.Id = character.ObjectId;
                        existingNewPlayer.InParty = true;
                        this.Players.Add(character.ObjectId, existingNewPlayer);

                        Helpers.Log($"Player {existingNewPlayer.Name} returned the party.");
                    }
                    else
                    {
                        this.Players.Add(player, new Player(character, this));
                        Helpers.Log($"Player {character.Name} joined the party for the first time.");
                    }

                }
            }
        }

        // step 3 - remove players
        foreach (var player in playersToRemove)
        {
            if (this.active)
            {
                this.Players[player].InParty = false;
                Helpers.Log($"Player {this.Players[player].Name} left the party during combat.");
            }
            else
            {
                this.Players.Remove(player);
                Helpers.Log($"Player {player} left the party.");
            }
        }

        foreach (var player in partyList)
        {
            if (player.GameObject == null) continue;
            if (Players.ContainsKey(player.ObjectId) && Players[player.ObjectId].InParty == false)
            {
                Players[player.ObjectId].InParty = true;
            }
        }

        this.partyListId = CreateId();
        PluginManager.Instance.PluginLog.Info("Party update complete: " + this.Players.Count + " players in encounter.");
    }

    public void UpdatePartyFallBack()
    {
        PluginManager.Instance.PluginLog.Info("Party has changed, triggering update.");

        List<PartyMember> PlayerListadd = new List<PartyMember>();
        List<uint> PlayerListremove = new List<uint>();

        var partyList = PluginManager.Instance.PartyList;

        if (PluginManager.Instance.PartyList.Length != 0)
        {
            // fill lists to compare
            foreach (var player in partyList)
            {
                if (player.GameObject == null) continue;
                if (!Players.ContainsKey(player.ObjectId))
                {
                    PlayerListadd.Add(player); // new players to be added later
                }
                PlayerListremove.Add(player.ObjectId); // players in common
            }
            if (PlayerListremove.Count + PlayerListadd.Count == 0) return; // error all players are null
        }
        else
        {
            // keep only you
            if (PluginManager.Instance.ClientState.LocalPlayer == null) return; // error you are null

            var you = (PluginManager.Instance.ClientState.LocalPlayer as Character).ObjectId;
            foreach (var player in Players)
            {
                if (player.Key != you) Players.Remove(player.Key); // remove copy
            }

            return; // ggs
        }

        // remove players
        foreach (var player in Players)
        {
            if (!PlayerListremove.Contains(player.Key)) Players.Remove(player.Key); // remove players not in common
        }

        foreach (var player in PlayerListadd)
        {
            var character = (player.GameObject as Character);
            if (character == null) continue; // error could not retrieve player
            Players.Add(character.ObjectId, new Player(character, this)); // add all new players to the player list
        }
        this.partyListId = CreateId();
    }



    public void UpdateParty2()
    {
        PluginManager.Instance.PluginLog.Info("Party has changed, triggering update.");

        List<PartyMember> PlayerListadd = new List<PartyMember>();
        List<uint> PlayerListremove = new List<uint>();

        var partyList = PluginManager.Instance.PartyList;

        if (PluginManager.Instance.PartyList.Length != 0)
        {
            // fill lists to compare
            foreach (var player in partyList)
            {
                if (player.GameObject == null) continue;

                // var a = player;
                if (!Players.ContainsKey(player.ObjectId))
                {
                    PlayerListadd.Add(player); // new players to be added later
                }
                else
                {
                    Players[player.ObjectId].InParty = true; // re activate player
                }
                PlayerListremove.Add(player.ObjectId); // players in common
            }
            if (PlayerListremove.Count + PlayerListadd.Count == 0) return; // error all players are null
        }
        else
        {
            // keep only you
            if (PluginManager.Instance.ClientState.LocalPlayer == null) return; // error you are null

            var you = (PluginManager.Instance.ClientState.LocalPlayer).ObjectId;
            foreach (var player in Players)
            {
                if (player.Key != you)
                {
                    if (!this.active)
                    {
                        Players.Remove(player.Key); // true remove copy
                    }
                    else
                    {
                        player.Value.InParty = false; // disable player
                    }
                }

                return; // ggs
            }
        }

        // remove players
        foreach (var player in Players)
        {
            if (!PlayerListremove.Contains(player.Key))
            {
                if (!this.active)
                {
                    Players.Remove(player.Key); // remove players not in common
                }
                else
                {
                    // we need to check and update for id changes here and not add them again down there
                    player.Value.InParty = false; // disable player
                }
            }
        }

        foreach (var player in PlayerListadd)
        {
            var character = (player.GameObject as Character);
            if (character == null) continue; // error could not retrieve player
            Players.Add(character.ObjectId, new Player(character, this)); // add all new players to the player list

        }
        this.partyListId = CreateId();
    }




    private string GetEncounterName()
    {
        var locationRow = PluginManager.Instance.DataManager.GetExcelSheet<TerritoryType>()?.GetRow(PluginManager.Instance.ClientState.TerritoryType);
        var instanceContentName = locationRow?.ContentFinderCondition.Value?.Name?.ToString();
        var placeName = locationRow?.PlaceName.Value?.Name?.ToString();

        return (string.IsNullOrEmpty(instanceContentName) ? placeName : instanceContentName) ?? "";
    }

    private TimeSpan Duration()
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
        var currentSeconds = this.duration.TotalSeconds <= 1 ? 1 : this.duration.TotalSeconds;
        this.DPS = (float)(this.TotalDamage / currentSeconds);
        foreach (var player in this.Players.Values)
        {
            player.UpdateStats();
        }
    }

}

public class Player
{
    public uint Id { get; set; }

    private Encounter Encounter { get; init; }

    public string Name { get; set; }
    public uint Job { get; set; }

    public uint? World { get; set; }

    public float DPS { get; set; }
    public uint TotalDamage { get; set; }

    public float DamagePercentage { get; set; }

    public bool InParty { get; set; }

    public List<INetworkMessage> RawActions { get; set; }

    public void UpdateStats()
    {
        var currentSeconds = this.Encounter.duration.TotalSeconds <= 1 ? 1 : this.Encounter.duration.TotalSeconds;
        this.DPS = (float)(this.TotalDamage / currentSeconds);
        this.DamagePercentage = this.Encounter.TotalDamage != 0 ? (float)((this.TotalDamage * 100) / this.Encounter.TotalDamage) : 0;
    }

    public Player(Character character, Encounter encounter)
    {
        this.Encounter = encounter;
        this.Id = character.ObjectId;
        this.Name = character.Name.ToString();
        this.Job = character.ClassJob.Id;
        this.DPS = 0;
        this.TotalDamage = 0;
        this.DamagePercentage = 0;
        this.InParty = true;
        this.RawActions = new List<INetworkMessage>();

        this.World = (character as PlayerCharacter)?.HomeWorld.Id;
    }
}
