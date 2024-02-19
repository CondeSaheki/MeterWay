using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

using Meterway.Managers;

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

        List<uint> newPlayerList = new List<uint>();
        var partyList = PluginManager.Instance.PartyList;

        if (PluginManager.Instance.PartyList.Length != 0)
        {
            foreach (var player in partyList)
            {
                if (player.GameObject != null)
                    newPlayerList.Add((player.GameObject as GameObject).ObjectId);
            }
        }
        else if (PluginManager.Instance.ClientState.LocalPlayer != null)
        {
            newPlayerList.Add((PluginManager.Instance.ClientState.LocalPlayer as Character).ObjectId);
        }

        foreach (var player in this.Players.Keys)
        {
            if (newPlayerList.Contains(player))
            {
                this.Players[player].InParty = true;
            }
            else if (this.active)
            {
                this.Players[player].InParty = false;
            }
            else
            {
                var playerToCheck = this.Players[player];
                var existingNewPlayer = partyList.FirstOrDefault(p => p.GameObject != null && p.GameObject.Name.ToString() == playerToCheck.Name && (p.GameObject as PlayerCharacter)?.HomeWorld.Id == playerToCheck.World);

                if (existingNewPlayer != null && existingNewPlayer.GameObject != null)
                {
                    playerToCheck.Id = existingNewPlayer.GameObject.ObjectId;
                    playerToCheck.InParty = true;

                    this.Players.Add(playerToCheck.Id, playerToCheck);
                }

                this.Players.Remove(player);
            }
        }

        PluginManager.Instance.PluginLog.Info("Party update complete: " + this.Players.Count + " players in encounter.");

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
