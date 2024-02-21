using Newtonsoft.Json.Linq;
using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

using MeterWay.Data;
using MeterWay.LogParser;
using MeterWay.Utils;

namespace MeterWay.Managers;

public class EncounterManager : IDisposable
{
    // data
    public readonly List<Encounter> encounters;
    public readonly List<Action> Clients;

    private bool lastCombatState;

    public static EncounterManager Inst { get; private set; } = null!;

    // constructor
    public EncounterManager()
    {
        this.encounters = [];
        encounters.Add(new Encounter());
        this.lastCombatState = false;
        this.Clients = [];

        InterfaceManager.Inst.DutyState.DutyStarted += EncounterManagerOnDutyStart;

        Inst = this;
    }

    private static void EncounterManagerOnDutyStart<ArgType>(Object? sender, ArgType Args)
    {
        Helpers.Log("EncounterManager OnDutyStart Event Trigerred!");
        if(!EndEncounter()) ResetEncounter();
        Inst.encounters.Last().UpdateEncounter();
    }

    public void Dispose()
    {
        InterfaceManager.Inst.DutyState.DutyStarted -= EncounterManagerOnDutyStart;
    }

    // metods
    public static bool StartEncounter()
    {
        if (Inst.encounters.Last().Active) return false;
        if (Inst.encounters.Last().Finished) Inst.encounters.Add(new Encounter());

        Inst.encounters.Last().UpdateEncounter();
        Inst.encounters.Last().RawActions.RemoveAll(r => r.DateTime < Inst.encounters.Last().Start - TimeSpan.FromSeconds(30));
        Inst.encounters.Last().StartEncounter();
        return true;
    }

    public static bool EndEncounter()
    {
        if(Inst.encounters.Last().Finished) return false;
        Inst.encounters.Last().EndEncounter();
        Inst.encounters.Last().UpdateEncounter();
        Inst.encounters.Add(new Encounter());
        return true;
    }

    public static void ResetEncounter()
    {
        Inst.encounters.Add(new Encounter());
        Inst.encounters.Remove(Inst.encounters[Inst.encounters.Count - 1]);
    }

    public static List<Encounter> AllEncounters() { return Inst.encounters; }

    public Encounter CurrentEncounter()
    {
        if (encounters.Last().Active || encounters.Last().Finished) return encounters.Last();
        else if (encounters.Count > 1) return encounters[^2];
        return encounters.Last();
    }

    private static bool GetInCombat()
    {
        var partyList = InterfaceManager.Inst.PartyList;
        if (partyList.Length == 0)
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

        LoglineParser.Parse(json); // parse data
    }

    public static void UpdateClients()
    {
        if (Inst.CurrentEncounter().Finished) return;
        foreach (var client in Inst.Clients) client.Invoke();
    }
}