using System;
using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;

using MeterWay.managers;

namespace MeterWay;

public class Encounter
{
    // data
    public uint id { get; set; }
    public string Name { get; set; }

    public bool active { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public TimeSpan duration => Duration();

    public List<INetworkMessage> RawActions { get; set; }
    public Dictionary<uint, Player> Players { get; set; }
    
    
    public Int64 TotalDamage { get; set; }
    public float Dps { get; set; }
    
    // constructor
    public Encounter()
    {
        this.id = CreateId();
        this.Name = GetEncounterName();
        this.active = false;
        this.Players = GetPlayers();
        this.TotalDamage = 0;
        this.Dps = 0;
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

                var character = (Dalamud.Game.ClientState.Objects.Types.Character)player.GameObject;
                playerstemp.Add(character.ObjectId, new Player(character, this));
            }
        }
        else
        {
            if (PluginManager.Instance.ClientState.LocalPlayer != null)
            {
                var character = (Dalamud.Game.ClientState.Objects.Types.Character)PluginManager.Instance.ClientState.LocalPlayer;
                playerstemp.Add(character.ObjectId, new Player(character, this));
            }
        }
        return playerstemp;
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
        this.Start = DateTime.Now;
        this.active = true;
    }

    public void EndEncounter()
    {
        this.End = DateTime.Now;
        this.active = false;
    }

    public void Update()
    {
        this.Dps = this.duration.TotalSeconds != 0 ? (float)(this.TotalDamage / this.duration.TotalSeconds) : 0;
        //this.Dps = TotalDamage / duration;
        foreach (var player in this.Players.Values)
        {
            player.Update();
        }
    }
}

public class Player
{
    public uint Id { get; set; }

    private Encounter Encounter { get; init; }

    public string Name { get; set; }
    public uint Job { get; set; }

    public float DPS { get; set; }
    public uint TotalDamage { get; set; }
    
    public float DamagePercentage { get; set; }

    public List<INetworkMessage> RawActions { get; set; }

    public void Update()
    {
        this.DPS = this.Encounter.duration.TotalSeconds != 0 ? (float)(this.TotalDamage / this.Encounter.duration.TotalSeconds) : 0;
        this.DamagePercentage = this.Encounter.TotalDamage != 0 ? (float)((this.TotalDamage * 100) / this.Encounter.TotalDamage) : 0;
    }

    public Player(Dalamud.Game.ClientState.Objects.Types.Character character, Encounter encounter)
    {
        this.Encounter = encounter;
        this.Id = character.ObjectId;
        this.Name = character.Name.ToString();
        this.Job = character.ClassJob.Id;
        this.DPS = 0;
        this.TotalDamage = 0;
        this.DamagePercentage = 0;
        
        this.RawActions = new List<INetworkMessage>();
    }
}