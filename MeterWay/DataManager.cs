using Newtonsoft.Json.Linq;
using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

using MeterWay;
using MeterWay.managers;
using MeterWay.IINACT;

namespace MeterWay;

public class DataManager
{
    // data
    public List<Encounter> encounters;
    private bool lastcombatstate;
    public Encounter current => encounters.Last();

    // constructor
    public DataManager()
    {
        this.encounters = new List<Encounter>();
        encounters.Add(new Encounter());
        this.lastcombatstate = false;
    }

    // metods
    void StartEncounter()
    {
        if (current.active)
        {
            EndEncounter();
        }
        encounters.Add(new Encounter());
        current.StartEncounter();
    }

    void EndEncounter()
    {
        current.EndEncounter();
    }

    public Encounter Current() { return current; }
    
    private bool GetInCombat()
    {
        if (PluginManager.Instance.PartyList.Length == 0)
        {
            return PluginManager.Instance.Condition[ConditionFlag.InCombat];
        }
        foreach (var player in PluginManager.Instance.PartyList)
        {
            if (player.GameObject == null) continue;

            var character = (Dalamud.Game.ClientState.Objects.Types.Character)player.GameObject;
            if ((character.StatusFlags & Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat) == Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat)
            {
                return true;
            }
        }
        return false;
    }

    public void Receiver(JObject json)
    {
        var combatstate = GetInCombat();

        // start combat
        if (combatstate == true && lastcombatstate == false)
        {

            PluginManager.Instance.PluginLog.Info("meterway detected start of combat");
            StartEncounter();
            lastcombatstate = true;
        }

        // end combat
        if (combatstate == false && lastcombatstate == true)
        {

            PluginManager.Instance.PluginLog.Info("meterway detected end of combat");
            EndEncounter();
            lastcombatstate = false;
        }

        // ignore all data when not in combat 
        if (combatstate == false)
        {
            //return;
        }

        // parse data
        try
        {
            IINACT.LoglineParser.Parse(json, current);
        }
        catch (Exception ex)
        {
            PluginManager.Instance.PluginLog.Error(ex.ToString());
            return;
        }
        return;
    }
}