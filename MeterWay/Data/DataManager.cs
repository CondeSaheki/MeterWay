using Newtonsoft.Json.Linq;
using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

using Meterway.Managers;

namespace MeterWay.Data;

public class DataManager
{
    // data
    public List<Encounter> encounters;
    private bool lastCombatState;
    public Encounter Current => CurrentEncounter();

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
        if (encounters.Last().Active) return;

        if (encounters.Last().Finished)
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

    public List<Encounter> AllEncounters() { return this.encounters; }

    public Encounter CurrentEncounter()
    {
        if (encounters.Last().Active || encounters.Last().Finished)
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
        var combatState = GetInCombat();

        // Start/End Combat
        if ((combatState == true) && lastCombatState == false)
        {
            if (!encounters.Last().Active) StartEncounter();
            lastCombatState = true;
        }
        if (combatState == false && lastCombatState == true)
        {
            EndEncounter();
            lastCombatState = false;
        }

        LoglineParser.Parse(json, this); // parse data
    }
}