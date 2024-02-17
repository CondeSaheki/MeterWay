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
using System.Text;

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

    public List<Player> Players { get; set; }

    // constructor
    public Encounter()
    {
        this.Name = GetEncounterName();
        this.active = false;
        this.Players = GetPlayers();
    }

    // metods
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
    public string Name { get; set; }
    public uint Job { get; set; }

    public float DPS { get; set; }
    public int TotalDamage { get; set; }
    public int DamagePercentage { get; set; }

    public Player(Dalamud.Game.ClientState.Objects.Types.Character character)
    {
        this.Name = character.Name.ToString();
        this.Job = character.ClassJob.Id;
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

public class Tests
{
    public static void parse(JObject json)
    {
        List<KeyValuePair<uint, System.Action<JToken>>> handlers = new List<KeyValuePair<uint, System.Action<JToken>>>();

        handlers.Add(new KeyValuePair<uint, System.Action<JToken>>(
            21, (JToken line) =>
            {
                // var time = line[1];
                // var playerid = Convert.ToInt32(line[2].ToString(), 16);
                // var player = line[3].ToString();
                // var actionhexcode = Convert.ToInt32(line[4].ToString(), 16);
                // var action = line[5].ToString();
                // var targetid = Convert.ToInt32(line[6].ToString(), 16);
                // var target = line[7].ToString();

                // var type = line[8];

                // int valuehex = Convert.ToInt32(line[9].ToString(), 16);
                // UInt32 value = (UInt32)((UInt32)(valuehex >> 16) | (UInt32)((valuehex << 16) ) & 0x0FFFFFFF);

                // PluginManager.Instance.PluginLog.Info($"22: {player} | {value.ToString()} | {type.ToString()}");
            }
        ));
        handlers.Add(new KeyValuePair<uint, System.Action<JToken>>(
            22, (JToken line) =>
            {

            }
        ));

        var line = json["line"];
        if (line == null) return;
        var linefirst = line[0];
        if (linefirst == null) return;
        var messagetype = linefirst.ToObject<int>();

        //debug
        var message = json["rawLine"]?.ToString() ?? "";
        PluginManager.Instance.PluginLog.Info($"parsed a: \"{messagetype}\" | \"{message}\"");

        try
        {
            foreach (KeyValuePair<uint, System.Action<JToken>> handler in handlers)
            {
                if (handler.Key == messagetype)
                {
                    
                    //handler.Value.Invoke(line);
                    break;
                }
            }
        }
        catch
        {
            PluginManager.Instance.PluginLog.Warning("fail");
        }

        // var log = new IINACTNetworkLog(json);

        //     foreach (string val in log.data)
        //     {
        //         //PluginManager.Instance.PluginLog.Info(val);
        //     }
    }
}