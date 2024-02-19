using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using MeterWay.managers;

namespace MeterWay;

public class Encounter
{
    // data
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


    public Int64 TotalDamage { get; set; }
    public float DPS { get; set; }

    // constructor
    public Encounter()
    {
        this.id = CreateId();
        this.partyListId = CreateId();
        this.Name = GetEncounterName();
        this.Players = GetPlayers();
        this.TotalDamage = 0;
        this.DPS = 0;
        this.RawActions = new List<INetworkMessage>();
    }

    private uint CreateId()
    {
        return (uint)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
    }

    // metods
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

        List<uint> newPlayerList = new List<uint>();
        if (PluginManager.Instance.PartyList.Length != 0)
        {
            foreach (var player in PluginManager.Instance.PartyList)
            {
                if (player.GameObject == null) continue;
                var character = (Character)player.GameObject;

                if (!this.Players.ContainsKey(character.ObjectId))
                {  
                    var existingPlayer = this.Players.Values.First(p => p.Name == character.Name.ToString() && p.World == (character as PlayerCharacter)?.HomeWorld.Id);
                    
                    if (existingPlayer != null)
                    {
                        this.Players[existingPlayer.Id].Id = character.ObjectId;
                        this.Players[player.ObjectId] = this.Players[existingPlayer.Id];
                    } else {
                        this.Players.Add(character.ObjectId, new Player(character, this));
                    }
                }

                newPlayerList.Add(character.ObjectId);
            }

            PluginManager.Instance.PluginLog.Info("Party player count: " + PluginManager.Instance.PartyList.Length);
        }
        else if (PluginManager.Instance.ClientState.LocalPlayer != null)
        {
            var character = (Character)PluginManager.Instance.ClientState.LocalPlayer;

            if (!this.Players.ContainsKey(character.ObjectId))
            {  
                var existingPlayer = this.Players.Values.First(p => p.Name == character.Name.ToString() && p.World == (character as PlayerCharacter)?.HomeWorld.Id);
                
                if (existingPlayer != null)
                {
                    this.Players[existingPlayer.Id].Id = character.ObjectId;
                    this.Players[character.ObjectId] = this.Players[existingPlayer.Id];
                } else {
                    this.Players.Add(character.ObjectId, new Player(character, this));
                }
            }
            newPlayerList.Add(character.ObjectId);
        }

        PluginManager.Instance.PluginLog.Info("PartyList: " + String.Join(", ", this.Players.Keys.ToArray()));

        PluginManager.Instance.PluginLog.Info("newPartyList: " + String.Join(", ", newPlayerList.ToArray()));

        foreach (var player in this.Players.Keys)
        {
            if (newPlayerList.Contains(player))
            {
                this.Players[player].InParty = true;
                continue;
            }

            if (this.active)
            {
                this.Players[player].InParty = false;
                continue;
            }

            PluginManager.Instance.PluginLog.Info("Tried to remove: " + player + " from encounter.");

            this.Players.Remove(player);
        }

        PluginManager.Instance.PluginLog.Info("Party update complete: " + this.Players.Count + " players in encounter.");

        this.partyListId = CreateId(); // update id
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
        //this.DPS = TotalDamage / duration;
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