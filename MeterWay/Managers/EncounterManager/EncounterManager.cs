using Newtonsoft.Json.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

using MeterWay.Data;

namespace MeterWay.Managers;

public class EncounterManager : IDisposable
{
    public static EncounterManager Inst { get; private set; } = null!;

    public readonly List<Encounter> encounters;
    private bool lastCombatState;
    public static Encounter LastEncounter => Inst.encounters.Last();

    public Notifier ClientsNotifier { get; init; }
    
    public event EventHandler EncounterBegin  = delegate { };
    public event EventHandler EncounterEnd = delegate { };
    
    public EncounterManager()
    {
        encounters = [];
        encounters.Add(new Encounter());
        lastCombatState = false;

        Dalamud.Duty.DutyStarted += OnDutyStart;

        ClientsNotifier = new Notifier(ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate);
        Inst = this;
    }

    private static void OnDutyStart<ArgType>(object? _, ArgType __)
    {
        Dalamud.Log.Debug("EncounterManager OnDutyStart, got trigerred");
        if (!Stop()) Reset();
        LastEncounter.Update();
    }

    public void Dispose()
    {
        Dalamud.Duty.DutyStarted -= OnDutyStart;
    }

    public static bool Start()
    {
        if (LastEncounter.Active) return false;
        if (LastEncounter.Finished) Inst.encounters.Add(new Encounter());

        LastEncounter.Update();
        LastEncounter.RawActions.RemoveAll(r => r.TimePoint < LastEncounter.Begin - TimeSpan.FromSeconds(30));
        LastEncounter.Start();
        Inst.ClientsNotifier.StartTimer();
        Inst.EncounterBegin?.Invoke(null, EventArgs.Empty);
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
        Inst.EncounterEnd?.Invoke(null, EventArgs.Empty);
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
        if (Dalamud.Conditions[ConditionFlag.InCombat]) return true;
        foreach (var player in Dalamud.PartyList)
        {
            if (player.GameObject == null) continue;
            if ((((ICharacter)player.GameObject).StatusFlags & StatusFlags.InCombat) !=  StatusFlags.None) return true;
        }
        return false;
    }

    public static void Receiver(object? _, JObject json)
    {
        var combatState = Dalamud.Framework.RunOnTick(IsInCombat).ConfigureAwait(false).GetAwaiter().GetResult();

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

        LoglineParser.Parse(json, LastEncounter); // parse data
    }
}