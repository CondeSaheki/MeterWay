using System;
using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;

using MeterWay;
using MeterWay.managers;

namespace MeterWay;

public class Encounter
{
    // data
    public string Name { get; set; }

    public bool active { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public TimeSpan duration => Duration();

    // all other data here

    public Dictionary<uint, Player> Players { get; set; }

    // constructor
    public Encounter()
    {
        this.Name = GetEncounterName();
        this.active = false;
        this.Players = GetPlayers();
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
                playerstemp.Add(character.ObjectId, new Player(character));
            }
        }
        else
        {
            if (PluginManager.Instance.ClientState.LocalPlayer != null)
            {
                var character = (Dalamud.Game.ClientState.Objects.Types.Character)PluginManager.Instance.ClientState.LocalPlayer;
                playerstemp.Add(character.ObjectId, new Player(character));
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
}

public class Player
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public uint Job { get; set; }

    public float DPS { get; set; }
    public uint TotalDamage { get; set; }
    public int DamagePercentage { get; set; }

    public Player(Dalamud.Game.ClientState.Objects.Types.Character character)
    {
        this.Id = character.ObjectId;
        this.Name = character.Name.ToString();
        this.Job = character.ClassJob.Id;
        this.DPS = 1000;
        this.TotalDamage = 0;
        this.DamagePercentage = 74;
    }
}

// public class susdata
// {
//     public uint damage { get; set; }
//     public uint hitcount { get; set; }
//     public uint crtcount { get; set; }
//     public uint dhcount { get; set; }
//     public uint crtdhcount { get; set; }

//     public uint crtpct { get; set; }
//     public uint dhpct { get; set; }
//     public uint crtdhct { get; set; }

//     public string dps { get; set; }


//     public susdata()
//     {

//     }
// }

// public class damagesus
// {
//     public string time;
//     public bool miss;
//     public bool crit;
//     public bool directhit;

//     public uint Value;

//     public damagesus()
//     {

//     }
// }