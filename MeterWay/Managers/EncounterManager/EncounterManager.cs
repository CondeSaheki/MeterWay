using Newtonsoft.Json.Linq;
using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

using MeterWay.Data;
using MeterWay.Utils;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;

namespace MeterWay.Managers;

public class EncounterManager : IDisposable
{
    // data
    public readonly List<Encounter> encounters;
    private bool lastCombatState;

    public static EncounterManager Inst { get; private set; } = null!;

    public static Encounter LastEncounter => Inst.encounters.Last();

    public Notifier ClientsNotifier { get; init; }
    public static List<KeyValuePair<uint, Action>> Clients => Inst.ClientsNotifier.Clients;

    // constructor
    public EncounterManager()
    {
        encounters = [];
        encounters.Add(new Encounter());
        lastCombatState = false;

        InterfaceManager.Inst.DutyState.DutyStarted += OnDutyStart;

        ClientsNotifier = new Notifier(TimeSpan.FromSeconds(1));
        Inst = this;
    }

    private static void OnDutyStart<ArgType>(object? sender, ArgType Args)
    {
        Helpers.Log("EncounterManager OnDutyStart Event Trigerred!");
        if (!Stop()) Reset();
        LastEncounter.Update();
    }

    public void Dispose()
    {
        InterfaceManager.Inst.DutyState.DutyStarted -= OnDutyStart;
    }

    public static bool Start()
    {
        if (LastEncounter.Active) return false;
        if (LastEncounter.Finished) Inst.encounters.Add(new Encounter());

        LastEncounter.Update();
        LastEncounter.RawActions.RemoveAll(r => r.TimePoint < LastEncounter.Begin - TimeSpan.FromSeconds(30));
        LastEncounter.Start();
        Inst.ClientsNotifier.StartTimer();
        return true;
    }

    public static void Start(Encounter encounter)
    {
        if (encounter.Finished) return;
        if (LastEncounter.Id == encounter.Id) Start(); // gamer
    }

    public static bool Stop()
    {
        if (LastEncounter.Finished) return false;
        LastEncounter.Stop();
        LastEncounter.Update();
        Inst.encounters.Add(new Encounter());
        Inst.ClientsNotifier.StopTimer();
        return true;
    }

    public static void Reset()
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

    private static bool IsInCombat()
    {
        if (InterfaceManager.Inst.Condition[ConditionFlag.InCombat]) return true;
        foreach (var player in InterfaceManager.Inst.PartyList)
        {
            if (player.GameObject == null) continue;
            if ((((Character)player.GameObject).StatusFlags & StatusFlags.InCombat) == 0) return true;
        }
        return false;
    }

    public static void Receiver(ref readonly JObject json)
    {
        var combatState = IsInCombat();

        // Start/End Combat
        if ((combatState == true) && Inst.lastCombatState == false)
        {
            if (!LastEncounter.Active) Start();
            Inst.lastCombatState = true;
        }
        if (combatState == false && Inst.lastCombatState == true)
        {
            Stop();
            Inst.lastCombatState = false;
        }

        LoglineParser.Parse(in json, LastEncounter); // parse data
    }
}