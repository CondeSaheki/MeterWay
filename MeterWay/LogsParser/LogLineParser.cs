using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using MeterWay.Managers;
using MeterWay.Data;
using MeterWay.Utils;

namespace MeterWay.LogParser;

public static class LoglineParser
{
    public static void Parse(ref readonly JObject json)
    {
        string? rawLine = json.GetValue("rawLine")?.ToObject<string>();
        if (rawLine == null) return;

        var line = new LogLineData(rawLine);

        Action<LogLineData> handler = line.MsgType switch
        {
            LogLineType.ActionEffect => MsgActionEffect,
            LogLineType.AOEActionEffect => MsgAOEActionEffect,
            LogLineType.StartsCasting => MsgStartsCasting,
            LogLineType.DoTHoT => MsgDoTHoT,
            LogLineType.PartyList => MsgPartyList,
            LogLineType.AddCombatant => MsgAddCombatant,
            LogLineType.PlayerStats => MsgPlayerStats,
            LogLineType.StatusApply => MsgStatusApply,
            LogLineType.StatusRemove => MsgStatusRemove,
            LogLineType.Death => MsgDeath,
            _ => (LogLineData) => { }
        };
        handler.Invoke(line);
        
        if (line.Parsed) EncounterManager.UpdateClients();
    }

    private static void MsgActionEffect(LogLineData logLineData)
    {
        if (false) return; // todo ignore parsing

        logLineData.Parse();
        ActionEffect parsed = (ActionEffect)logLineData.Value!;

        Encounter encounter = EncounterManager.Inst.encounters.Last();

        bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        bool targetIsPlayer = parsed.TargetId == null ? false : encounter.Players.ContainsKey((uint)parsed.TargetId);
        bool ActionFromPet = false; // ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        static uint DamageHealCalc(uint val) => (val >> 16 | val << 16) & 0x0FFFFFFF;

        foreach (KeyValuePair<uint, uint> attribute in parsed.ActionAttributes)
        {
            if (ActionEffectFlag.IsNothing((int)attribute.Key)) break;
            if (ActionEffectFlag.IsSpecial((int)attribute.Key)) { }; // TODO

            if (sourceIsPlayer && encounter.Players[parsed.SourceId].IsActive)
            {
                var player = encounter.Players[parsed.SourceId];
                if (ActionEffectFlag.IsDamage((int)attribute.Key))
                {
                    EncounterManager.Start(); // gamer

                    var actionValue = DamageHealCalc(attribute.Value);
                    player.Damage.Total += actionValue;
                    encounter.Damage.Total += actionValue;
                }
                else if (ActionEffectFlag.IsHeal((int)attribute.Key))
                {
                    if (ActionEffectFlag.IsCritHeal((int)attribute.Key))
                    {
                        player.Healing.Count.Crit += 1;
                        player.Healing.TotalCrit += DamageHealCalc(attribute.Value);
                    }
                    player.Healing.Total += DamageHealCalc(attribute.Value);
                    player.Healing.Count.Hit += 1;
                }
            }

            if (targetIsPlayer)
            {
                var player = encounter.Players[(uint)parsed.TargetId!];
                if (ActionEffectFlag.IsDamage((int)attribute.Key))
                {
                    player.DamageTaken.Total = DamageHealCalc(attribute.Value);
                    player.DamageTaken.Count.Hit += 1;
                }
                else if (ActionEffectFlag.IsHeal((int)attribute.Key))
                {
                    if (ActionEffectFlag.IsCritHeal((int)attribute.Key >> 16))
                    {
                        player.HealingTaken.TotalCrit = DamageHealCalc(attribute.Value);
                        player.HealingTaken.Count.Crit += 1;
                    }
                    player.HealingTaken.Total = DamageHealCalc(attribute.Value);
                    player.HealingTaken.Count.Hit += 1;
                }
            }

            // TODO Pets
            if (ActionFromPet)
            {
                Helpers.Log("Detected Action From Pet");
            }

            break;
        }
    }

    private static void MsgAOEActionEffect(LogLineData logLineData)
    {
        MsgActionEffect(logLineData); // have same format
    }

    private static void MsgStartsCasting(LogLineData logLineData)
    {
        if (true) return; // todo

        logLineData.Parse();
        var parsed = (StartsCasting)logLineData.Value!;

        // todo
    }

    private static void MsgDoTHoT(LogLineData logLineData)
    {
        // dots are calculated even if the player is deactivated, pet actions should probably do the same #for analyzing the data later
        // or add a flag to the damaged saying it was ignored idk

        if (false) return; // todo ignore parsing

        logLineData.Parse();
        var parsed = (DoTHoT)logLineData.Value!;

        Encounter encounter = EncounterManager.Inst.encounters.Last();

        bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        bool ActionFromPet = false; // ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        if (sourceIsPlayer)
        {
            //if (!thisEnconter.Players[parsed.SourceId].IsActive) return;
            if (!parsed.IsHeal) // IsDamage
            {
                encounter.Players[parsed.SourceId].Damage.Total += parsed.Value;
                encounter.Damage.Total += parsed.Value;
            }
            else
            {
                encounter.Players[parsed.SourceId].Healing.Total += parsed.Value;
                encounter.Healing.Total += parsed.Value;
            }
        }

        if (targetIsPlayer)
        {
            if (!parsed.IsHeal) // IsDamage
            {
                encounter.Players[parsed.TargetId].DamageTaken.Total += parsed.Value;
                encounter.DamageTaken.Total += parsed.Value;
            }
            else
            {
                encounter.Players[parsed.TargetId].HealingTaken.Total += parsed.Value;
                encounter.HealingTaken.Total += parsed.Value;
            }
        }

        // TODO Pets
        if (ActionFromPet)
        {
            Helpers.Log("Detected Action From Pet");
        }

    }

    private static void MsgPartyList(LogLineData logLineData)
    {
        EncounterManager.Inst.encounters.Last().Update();
        return;
    }

    // this will be used to make sure summoners have their pets damage accounted
    private static void MsgAddCombatant(LogLineData logLineData)
    {
        if (true) return; // todo
        logLineData.Parse();
        var parsed = (AddCombatant)logLineData.Value!;

        Encounter encounter = EncounterManager.Inst.encounters.Last();

        // bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        // bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        // bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        // if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        // todo

        if (parsed.IsPet)
        {
            Helpers.Log("Detected Action From Pet");
        }
    }

    private static void MsgPlayerStats(LogLineData logLineData)
    {
        if (true) return; // todo
        logLineData.Parse();
        var parsed = (PlayerStats)logLineData.Value!;

        Encounter encounter = EncounterManager.Inst.encounters.Last();

        // bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        // bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        // bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        // if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        // todo

    }

    private static void MsgStatusApply(LogLineData logLineData)
    {
        if (true) return; // todo
        logLineData.Parse();
        var parsed = (StatusApply)logLineData.Value!;

        Encounter encounter = EncounterManager.Inst.encounters.Last();

        // bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        // bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        // bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        // if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        // todo

    }

    private static void MsgStatusRemove(LogLineData logLineData)
    {
        if (true) return; // todo
        logLineData.Parse();
        var parsed = (StatusRemove)logLineData.Value!;

        Encounter encounter = EncounterManager.Inst.encounters.Last();

        // bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        // bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        // bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        // if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        // todo

    }

    private static void MsgDeath(LogLineData logLineData)
    {
        if (true) return; // todo
        logLineData.Parse();
        var parsed = (Death)logLineData.Value!;

        Encounter encounter = EncounterManager.Inst.encounters.Last();

        // bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        // bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        // bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        // if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        // todo

    }
}
