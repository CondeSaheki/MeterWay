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
    public Encounter current => Current();

    // constructor
    public DataManager()
    {
        this.encounters = new List<Encounter>();
        encounters.Add(new Encounter());
        this.lastCombatState = false;
    }

    // metods
    public void StartEncounter()
    {
        if (encounters.Last().active) return;

        if (encounters.Last().finished)
        {
            encounters.Add(new Encounter());
            encounters.Last().StartEncounter();
        }

        encounters.Last().RawActions.RemoveAll(r => r.DateTime < encounters.Last().Start - TimeSpan.FromSeconds(30));
    }

    public void EndEncounter()
    {
        encounters.Last().EndEncounter();
        encounters.Add(new Encounter());
    }

    public Encounter Current()
    {
        if (encounters.Last().active || encounters.Last().finished)
        {
            return encounters.Last();
        }
        else if (encounters.Count > 1)
        {
            return encounters[encounters.Count - 2];
        }
        return encounters.Last();
    }


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
        var combatState = GetInCombat(); //  || 

        // start combat
        if ((combatState == true) && lastCombatState == false)
        {
            if (!encounters.Last().active) StartEncounter();
            lastCombatState = true;
        }

        // end combat
        if (combatState == false && lastCombatState == true)
        {
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
            LoglineParser.Parse(json, this);
        }
        catch (Exception ex)
        {
            PluginManager.Instance.PluginLog.Error(ex.ToString());
            return;
        }
        return;
    }
}