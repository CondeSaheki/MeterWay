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



    void a(JObject json)
    {
        List<KeyValuePair<string, Action>> handler = new List<KeyValuePair<string, Action>>();
        handler.Add(new KeyValuePair<string, Action>(
            "21", () =>
            {

            }
        ));
        handler.Add(new KeyValuePair<string, Action>(
            "22", () =>
            {

            }
        ));

        var a = json["list"]?.First?.Value<string>();

        // for ()
        // {
        // }
    }
}

public class Encounter
{

    public string Name { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public bool active { get; set; }

    public string Data { get; set; }

    public List<Player> Players { get; set; }

    public Encounter()
    {

        this.Name = "";

        var asdsa = PluginManager.Instance.ClientState.LocalContentId;

        // curent timestamp
        this.Start = DateTime.Now;
        this.End = DateTime.Now;
        this.active = true;


        // generate players
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
        this.Players = playerstemp;

    }


    public TimeSpan duration => End - Start;

    void EndEncounter()
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
        this.DPS = 0;
        this.TotalDamage = 0;
        this.DamagePercentage = 0;
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
