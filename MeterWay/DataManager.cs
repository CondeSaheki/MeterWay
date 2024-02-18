using Newtonsoft.Json.Linq;
using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

using MeterWay.managers;
using MeterWay.IINACT;

namespace MeterWay;

public class DataManager
{
    // data
    public List<Encounter> encounters;
    private bool lastCombatState;
    public Encounter current => encounters.Last();

    // constructor
    public DataManager()
    {
        this.encounters = new List<Encounter>();
        encounters.Add(new Encounter());
        this.lastCombatState = false;
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
        var combatState = GetInCombat();

        // start combat
        if (combatState == true && lastCombatState == false)
        {

            PluginManager.Instance.PluginLog.Info("meterway detected start of combat");
            StartEncounter();
            lastCombatState = true;
        }

        // end combat
        if (combatState == false && lastCombatState == true)
        {

            PluginManager.Instance.PluginLog.Info("meterway detected end of combat");
            EndEncounter();
            lastCombatState = false;
        }

        // ignore all data when not in combat 
        if (combatState == false)
        {
            //return;
        }

        // parse data
        try
        {
            LoglineParser.Parse(json, current);
        }
        catch (Exception ex)
        {
            PluginManager.Instance.PluginLog.Error(ex.ToString());
            return;
        }
        return;
    }
}