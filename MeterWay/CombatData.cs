using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Meterway;

public class CombatData
{
    public Encounter encounter { get; set; }
    public List<Combatant> combatants { get; set; }

    public CombatData(Plugin plugin, JObject json)
    {

        // encounter
        JToken? jsonEncounter = json.Value<JToken>("Encounter");
        if (jsonEncounter == null)
        {
            plugin.PluginLog.Warning("Meterway failed to get Encounter");
            this.encounter = new Encounter();
        }
        else
        {
            this.encounter = new Encounter(plugin, jsonEncounter);
        }

        // Combatant
        List<Combatant> combatantList = new List<Combatant>();

        JToken? jsonCombatant = json.Value<JToken>("Combatant");
        if (jsonCombatant == null)
        {
            plugin.PluginLog.Warning("Meterway failed to get Combatants");
            this.combatants = combatantList;
            return;

        }
        var jsonCombatantlist = jsonCombatant.Children().ToList();
        if (jsonCombatantlist.Count() == 0)
        {
            plugin.PluginLog.Warning("Meterway Combatants empty");
            this.combatants = combatantList;
            return;
        }

        // // you are alone in the party create combatents
        // if (plugin.PartyList.Length == 0 && jsonCombatantlist.Count() == 1)
        // {
        //     JToken? key = jsonCombatantlist.Last().First;
        //     if (key != null)
        //         combatantList.Add(new Combatant(plugin, key));

        //     this.combatants = combatantList;
        //     return;
        // }
    
        // get party list names
        var party = new List<string>();
        party.Add("YOU");

        for (var i = 0; i != plugin.PartyList.Length; ++i)
        {
            var player = plugin.PartyList[i]?.Name.ToString();
            if (player == null) continue;

            party.Add(player);
        }

        // compare and parse only party members
        foreach (JToken jsonCombatantKey in jsonCombatantlist)
        {
            JToken? key = jsonCombatantKey.First;
            if (key == null) continue;

            string? value = key["name"]?.Value<string>();
            if (key == null) continue;
        
            if (party.Exists(name => name == value))
            {
                combatantList.Add(new Combatant(plugin, key));
            }
        }
        this.combatants = combatantList;
    }
}

public class Encounter
{
    // general
    public string Name { get; set; }
    public uint Duration { get; set; }
    public uint death { get; set; }
    public uint Kill { get; set; }

    // damage
    public uint Dmg { get; set; }
    public uint Dps { get; set; }
    public uint EncDps { get; set; }
    public uint Hit { get; set; }
    public uint Crt { get; set; }
    public uint CrtPct { get; set; }

    // healing
    public uint heal { get; set; }
    public uint EncHps { get; set; }
    public uint CrtHeal { get; set; }
    public uint CrtHealPct { get; set; }
    public uint HealTaken { get; set; }

    // tanking
    public uint DmgTaken { get; set; }

    // other
    public string MaxHit { get; set; }
    public string MaxHeal { get; set; }
    public uint HitMiss { get; set; }
    public uint HitFail { get; set; }
    public Encounter()
    {
        // general
        this.Name = "";
        this.Duration = 0;
        this.death = 0;
        this.Kill = 0;

        // damage
        this.Dmg = 0;
        this.Dps = 0;
        this.EncDps = 0;
        this.Hit = 0;
        this.Crt = 0;
        this.CrtPct = 0;

        // healing
        this.heal = 0;
        this.EncHps = 0;
        this.CrtHeal = 0;
        this.CrtHealPct = 0;
        this.HealTaken = 0;

        // tanking
        this.DmgTaken = 0;

        // other
        this.MaxHit = "";
        this.MaxHeal = "";
        this.HitMiss = 0;
        this.HitFail = 0;
    }
    public Encounter(Plugin plugin, JToken json)
    {
        string GetStr(string key)
        {
            var value = json[key]?.Value<string>();
            if (value == null) //  || value == ""
            {
                plugin.PluginLog.Warning("Meterway Failed to parse key: \"" + key + "\"");
                return "";
            }
            return value;
        }
        uint GetUint(string key)
        {
            var value = new string(GetStr(key).Where(c => char.IsDigit(c)).ToArray());

            if (value == "")
            {
                return 0;
            }
            try
            {
                uint parsed = uint.Parse(value);
                return parsed;
            }
            catch
            {
                plugin.PluginLog.Warning("Meterway failed to parse: \"" + key + "\" | \"" + value + "\"");
                return 0;
            }
        }

        // general
        this.Name = GetStr("CurrentZoneName");
        this.Duration = GetUint("DURATION");
        this.death = GetUint("deaths");
        this.Kill = GetUint("kills");

        // damage
        this.Dmg = GetUint("damage");
        this.Dps = GetUint("DPS");
        this.EncDps = GetUint("ENCDPS");
        this.Hit = GetUint("hits");
        this.Crt = GetUint("crithits");
        this.CrtPct = GetUint("crithit%");

        // healing
        this.heal = GetUint("healed");
        this.EncHps = GetUint("ENCHPS");
        this.CrtHeal = GetUint("critheals");
        this.CrtHealPct = GetUint("critheal%");
        this.HealTaken = GetUint("healstaken");

        // tanking
        this.DmgTaken = GetUint("damagetaken");

        // other
        this.MaxHit = GetStr("maxhit");
        this.MaxHeal = GetStr("maxheal");
        this.HitMiss = GetUint("misses");
        this.HitFail = GetUint("hitfailed");
    }
}
public class Combatant
{
    // general
    public string Name { get; set; }
    public uint Duration { get; set; }
    public string Job { get; set; }
    public uint Death { get; set; }
    public uint Kill { get; set; }

    // damage
    public uint Hit { get; set; }
    public uint Dmg { get; set; }
    public uint DmgPct { get; set; }
    public uint Dps { get; set; }
    public uint EncDps { get; set; }
    public uint Crt { get; set; }
    public uint Dh { get; set; }
    public uint CrtDh { get; set; }
    public uint CrtPct { get; set; }
    public uint DhPct { get; set; }
    public uint CrtDhPct { get; set; }

    // healing
    public uint Heal { get; set; }
    public uint Shield { get; set; }
    public uint HealPct { get; set; }
    public uint EncHps { get; set; }
    public uint CrtHeal { get; set; }
    public uint OverHeal { get; set; }
    public uint OverHealPct { get; set; }
    public uint AbsorbHeal { get; set; }
    public uint HealTaken { get; set; }

    // tanking
    public uint DmgTaken { get; set; }
    public uint ParryPct { get; set; }
    public uint BlockPct { get; set; }

    // others
    public string MaxHitname { get; set; }
    public string MaxHit { get; set; }
    public string MaxHealName { get; set; }
    public string MaxHeal { get; set; }
    public uint HitMiss { get; set; }
    public uint HitFail { get; set; }

    public Combatant(Plugin plugin, JToken json)
    {
        string GetStr(string key)
        {
            var value = json[key]?.Value<string>();
            if (value == null) //  || value == ""
            {
                plugin.PluginLog.Warning("Meterway Failed get key: \"" + key + "\"");
                return "";
            }
            return value;
        }
        uint GetUint(string key)
        {
            var value = new string(GetStr(key).Where(c => char.IsDigit(c)).ToArray());

            if (value == "")
            {
                return 0;
            }
            try
            {
                uint parsed = uint.Parse(value);
                return parsed;
            }
            catch
            {
                plugin.PluginLog.Warning("Meterway failed to parse: \"" + key + "\" | \"" + value + "\"");
                return 0;
            }
        }


        // general
        this.Name = GetStr("name");
        this.Duration = GetUint("DURATION");
        this.Job = GetStr("Job");
        this.Death = GetUint("deaths");
        this.Kill = GetUint("kills");

        // damage
        this.Hit = GetUint("hits");
        this.Dmg = GetUint("damage");
        this.DmgPct = GetUint("damage%");
        this.Dps = GetUint("DPS");
        this.EncDps = GetUint("ENCDPS");
        this.Crt = GetUint("crithits");
        this.Dh = GetUint("DirectHitCount");
        this.CrtDh = GetUint("CritDirectHitCount");
        this.CrtPct = GetUint("crithit%");
        this.DhPct = GetUint("DirectHitPct");
        this.CrtDhPct = GetUint("CritDirectHitPct");

        // healing
        this.Heal = GetUint("healed");
        this.Shield = GetUint("damageShield");
        this.HealPct = GetUint("healed%");
        this.EncHps = GetUint("ENCDPS");
        this.CrtHeal = GetUint("critheal%");
        this.OverHeal = GetUint("overHeal");
        this.OverHealPct = GetUint("OverHealPct");
        this.AbsorbHeal = GetUint("absorbHeal");
        this.HealTaken = GetUint("healstaken");

        // tanking
        this.DmgTaken = GetUint("damagetaken");
        this.ParryPct = GetUint("ParryPct");
        this.BlockPct = GetUint("BlockPct");

        // others
        this.MaxHitname = GetStr("maxhit");
        this.MaxHit = GetStr("MAXHIT");
        this.MaxHealName = GetStr("maxheal");
        this.MaxHeal = GetStr("MAXHEAL");
        this.HitMiss = GetUint("misses");
        this.HitFail = GetUint("hitfailed");
    }
}



/* Combatant default constructor
public Combatant()
{
    // general
    this.Name = "";
    this.Duration = 0;
    this.Job = "";
    this.Death = 0;
    this.Kill = 0;

    // damage
    this.Hit = 0;
    this.Dmg = 0;
    this.DmgPct = 0;
    this.Dps = 0;
    this.EncDps = 0;
    this.Crt = 0;
    this.Dh = 0;
    this.CrtDh = 0;
    this.CrtPct = 0;
    this.DhPct = 0;
    this.CrtDhPct = 0;

    // healing
    this.Heal = 0;
    this.Shield = 0;
    this.HealPct = 0;
    this.EncHps = 0;
    this.CrtHeal = 0;
    this.OverHeal = 0;
    this.OverHealPct = 0;
    this.AbsorbHeal = 0;
    this.HealTaken = 0;

    // tanking
    this.DmgTaken = 0;
    this.ParryPct = 0;
    this.BlockPct = 0;

    // others
    this.MaxHitname = "";
    this.MaxHit = 0;
    this.MaxHealName = "";
    this.MaxHeal = 0;
    this.HitMiss = 0;
    this.HitFail = 0;
}
*/