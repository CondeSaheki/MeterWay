using Newtonsoft.Json.Linq;
using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

using MeterWay.Data;
using MeterWay.LogParser;

namespace MeterWay.Managers;

public class EncounterManager
{
    // data
    public readonly List<Encounter> encounters;
    public readonly List<Action> Clients;
    
    private bool lastCombatState;
    
    public static EncounterManager Inst { get; private set; } = null!;

    // constructor
    public EncounterManager()
    {
        this.encounters = new List<Encounter>();
        encounters.Add(new Encounter());
        this.lastCombatState = false;
        this.Clients = new List<Action>();

        Inst = this;

    }

    // metods
    public static void StartEncounter()
    {
        if (Inst.encounters.Last().Active) return;

        if (Inst.encounters.Last().Finished)
        {
            Inst.encounters.Add(new Encounter());
            Inst.encounters.Last().StartEncounter();
        }

        Inst.encounters.Last().RawActions.RemoveAll(r => r.DateTime < Inst.encounters.Last().Start - TimeSpan.FromSeconds(30));
    }

    public static void EndEncounter()
    {
        Inst.encounters.Last().EndEncounter();
        Inst.encounters.Add(new Encounter());
    }

    public static List<Encounter> AllEncounters() { return Inst.encounters; }

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

    private static bool GetInCombat()
    {
        var partyList = InterfaceManager.Inst.PartyList;
        if (partyList.Count() == 0)
        {
            return InterfaceManager.Inst.Condition[ConditionFlag.InCombat];
        }
        foreach (var player in partyList)
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

    public static void Receiver(JObject json)
    {
        var combatState = GetInCombat();

        // Start/End Combat
        if ((combatState == true) && Inst.lastCombatState == false)
        {
            if (!Inst.encounters.Last().Active) StartEncounter();
            Inst.lastCombatState = true;
        }
        if (combatState == false && Inst.lastCombatState == true)
        {
            EndEncounter();
            Inst.lastCombatState = false;
        }

        LoglineParser.Parse(json, Inst); // parse data
    }
 
    public static void UpdateClients()
    {
        foreach (var client in Inst.Clients) client.Invoke();
    }
}