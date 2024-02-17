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
using FFXIVClientStructs.FFXIV.Common.Math;

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










public class message2122
{
    // public int IdentificadorDeLinha { get; set; }
    // public int TamanhoArrayHitTargets { get; set; }
    // public int IndexArrayHitTargets { get; set; }
    // public string HexCounterLogLines { get; set; } // Assuming hex value stored as string
    // public float TargetAngulo { get; set; } // Assuming angle is a floating-point number
    // public float TargetPosZ { get; set; }
    // public float TargetPosY { get; set; }
    // public float TargetPosX { get; set; }
    // // Skipping null placeholders [8] and [9]
    // public int MaxMana { get; set; }
    // public int CurrentMana { get; set; }
    // public int MaxHp { get; set; }
    // public int CurrentHp { get; set; }
    // public float PlayerAngulo { get; set; } // Assuming angle is a floating-point number
    // public float PlayerPosZ { get; set; }
    // public float PlayerPosY { get; set; }
    // public float PlayerPosX { get; set; }
    // // Skipping null placeholders [18] and [19]
    // public int TargetMaxMp { get; set; }
    // public int TargetCurrentMp { get; set; }
    // public int TargetMaxHp { get; set; }
    // public int TargetCurrentHp { get; set; }
    // // Assuming [24-39] SkillAttributes is an array or similar collection
    // public SkillAttribute[] SkillAttributes { get; set; }
    // public string TargetName { get; set; }
    // public string TargetIdHex { get; set; } // Assuming hex value stored as string
    // public string SkillName { get; set; }
    // public string SkillIdHex { get; set; } // Assuming hex value stored as string
    // public string PlayerName { get; set; }
    // public string PlayedIdHex { get; set; } // Assuming hex value stored as string
    // public DateTime Date { get; set; }
    // public int LogId { get; set; }

    public message2122(JToken line)
    {
        // int crypto = Convert.ToInt32(line[47].ToString(), 16);
        // uint multimessagecount = Convert.ToUInt32(line[46].ToString());
        // uint multimessageindex = Convert.ToUInt32(line[45].ToString());
        // uint loglinescount = Convert.ToUInt32(line[44].ToString(), 16);

        Vector4 pos = new Vector4((float)Convert.ToDouble(line[40].ToString()), (float)Convert.ToDouble(line[41].ToString()),
            (float)Convert.ToDouble(line[42].ToString()), (float)Convert.ToDouble(line[43].ToString()));

        // var separator = line[39]; // null
        // var separator = line[38]; // null
        uint maxMp = Convert.ToUInt32(line[37].ToString());
        uint mp = Convert.ToUInt32(line[36].ToString());
        uint maxHp = Convert.ToUInt32(line[35].ToString());
        uint Hp = Convert.ToUInt32(line[34].ToString());

        Vector4 targetPos;
        if (line[33].ToString() != "" || line[32].ToString() != "" || line[31].ToString() != "" || line[30].ToString() != "")
        {
            targetPos = new Vector4((float)Convert.ToDouble(line[30].ToString()), (float)Convert.ToDouble(line[31].ToString()),
                (float)Convert.ToDouble(line[32].ToString()), (float)Convert.ToDouble(line[33].ToString()));
        }

        // var separator = line[29]; // null
        // var separator = line[28]; // null
        int? targetmaxMp = line[27].ToString() == "" ? null : Convert.ToInt32(line[27].ToString());
        int? targetmp = line[26].ToString() == "" ? null : Convert.ToInt32(line[26].ToString());
        int? targetmaxHp = line[25].ToString() == "" ? null : Convert.ToInt32(line[25].ToString());
        int? targetHp = line[24].ToString() == "" ? null : Convert.ToInt32(line[24].ToString());

        // var skillatributes = line[23];
        // var skillatributes = line[22];
        // var skillatributes = line[21];
        // var skillatributes = line[20];
        // var skillatributes = line[19];
        // var skillatributes = line[18];
        // var skillatributes = line[17];
        // var skillatributes = line[16];
        // var skillatributes = line[15];
        // var skillatributes = line[14];
        // var skillatributes = line[13];
        // var skillatributes = line[12];
        // var skillatributes = line[11];
        // var skillatributes = line[10];

        UInt32? value = null;
        if (line[9].ToString() != "")
        {
            int tempvaluehex = Convert.ToInt32(line[9].ToString(), 16);
            value = (UInt32)((UInt32)(tempvaluehex >> 16) | (UInt32)((tempvaluehex << 16)) & 0x0FFFFFFF);
        }

        var type = line[8].ToString();

        string? target = line[7].ToString() == "" ? null : line[7].ToString();
        int? targetid = line[6].ToString() == "" ? null : Convert.ToInt32(line[6].ToString(), 16);
        string action = line[5].ToString();
        int actionid = Convert.ToInt32(line[4].ToString(), 16);
        string player = line[3].ToString();
        int playerid = Convert.ToInt32(line[2].ToString(), 16);

        var datetime = line[1].ToString();

        // int messagetype = Convert.ToInt32(line[0].ToString())

        PluginManager.Instance.PluginLog.Info($"{player} | {action} | {(target == null ? "null" : target.ToString())} | {(value == null ? "null" : value.ToString())} | {type}");
    }
}

public class Tests
{
    public static Int32 FromHex(string str)
    {
        return Convert.ToInt32(str, 16);
    }

    public static void parse(JObject json)
    {
        List<KeyValuePair<uint, System.Action<JToken>>> handlers = new List<KeyValuePair<uint, System.Action<JToken>>>();

        handlers.Add(new KeyValuePair<uint, System.Action<JToken>>(
            21, (JToken line) =>
            {
                var a = new message2122(line);
            }
        ));
        handlers.Add(new KeyValuePair<uint, System.Action<JToken>>(
            22, (JToken line) =>
            {
                var a = new message2122(line);
            }
        ));

        var line = json["line"];
        if (line == null) return;
        var linefirst = line[0];
        if (linefirst == null) return;
        var messagetype = linefirst.ToObject<int>();

        //debug
        //var message = json["rawLine"]?.ToString() ?? "";
        //PluginManager.Instance.PluginLog.Info($"parsed a: \"{messagetype}\" | \"{message}\"");

        try
        {
            foreach (KeyValuePair<uint, System.Action<JToken>> handler in handlers)
            {
                if (handler.Key == messagetype)
                {
                    handler.Value.Invoke(line);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            PluginManager.Instance.PluginLog.Warning("fail -> " + ex.ToString());
        }

        // var log = new IINACTNetworkLog(json);

        //     foreach (string val in log.data)
        //     {
        //         //PluginManager.Instance.PluginLog.Info(val);
        //     }
    }
}