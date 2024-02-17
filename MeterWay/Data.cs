using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.ClientState.Conditions;

using Lumina.Excel.GeneratedSheets;


using MeterWay;
using MeterWay.managers;

namespace MeterWay;

public class IINACTNetworkLog
{
    //data
    public List<string> data;

    public IINACTNetworkLog(JObject json)
    {
        List<string>? dataline = json["line"]?.ToObject<List<string>>();

        if (dataline == null)
        {
            this.data = new List<string>();
            return;
        }
        this.data = dataline;
    }


}

public class Encounter
{

    public string Name { get; set; }

    public DateTime Start { get; set; }
    public DateTime? End { get; set; }

    public bool active { get; set; }
    public TimeSpan duration => (End != null) ? (TimeSpan)(End - Start) : DateTime.Now - Start;

    //public string Data { get; set; }

    public List<Player> Players { get; set; }


    public Encounter()
    {
        this.Name = GetEncounterName();

        // curent timestamp
        this.Start = DateTime.Now;
        this.active = true;

        // generate players

        this.Players = GetPlayers();

    }

    private List<Player> GetPlayers()
    {
        var playerstemp = new List<Player>();
        if (PluginManager.Instance.PartyList.Length != 0)
        {
            foreach (var player in PluginManager.Instance.PartyList)
            {
                if (player.GameObject == null) continue;

                var character = (Dalamud.Game.ClientState.Objects.Types.Character)player.GameObject;
                playerstemp.Add(new Player(character));
            }
        }
        else
        {
            if (PluginManager.Instance.ClientState.LocalPlayer != null)
            {
                var character = (Dalamud.Game.ClientState.Objects.Types.Character)PluginManager.Instance.ClientState.LocalPlayer;
                playerstemp.Add(new Player(character));
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

    public void Update(JObject json)
    {
        List<KeyValuePair<string, System.Action>> handler = new List<KeyValuePair<string, System.Action>>();
        handler.Add(new KeyValuePair<string, System.Action>(
            "21", () =>
            {

            }
        ));
        handler.Add(new KeyValuePair<string, System.Action>(
            "22", () =>
            {

            }
        ));

        var a = json["list"]?.First?.Value<string>();

        // for ()
        // {
        // }
    }

    public void EndEncounter()
    {
        // curent timestamp
        this.End = DateTime.Now;
        this.active = false;

    }
}

public class Player
{
    public string Name { get; set; }
    public uint Job { get; set; }
    public string Data { get; set; }

    public float DPS { get; set; }

    public int TotalDamage { get; set; }

    public int DamagePercentage { get; set; }

    public Player(Dalamud.Game.ClientState.Objects.Types.Character character)
    {
        this.Name = character.Name.ToString();
        this.Job = character.ClassJob.Id;
        this.Data = "";
        this.DPS = 1000;
        this.TotalDamage = 81299;
        this.DamagePercentage = 74;
    }
}


public class susdata
{
    public uint damage { get; set; }
    public uint hitcount { get; set; }
    public uint crtcount { get; set; }
    public uint dhcount { get; set; }
    public uint crtdhcount { get; set; }

    public uint crtpct { get; set; }
    public uint dhpct { get; set; }
    public uint crtdhct { get; set; }

    public string dps { get; set; }


    public susdata()
    {

    }
}

public class damagesus
{
    public string time;
    public bool miss;
    public bool crit;
    public bool directhit;

    public uint Value;

    public damagesus()
    {

    }
}





