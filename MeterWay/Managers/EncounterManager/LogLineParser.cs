using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using MeterWay.Managers;
using MeterWay.LogParser;

namespace MeterWay.Data;

public static class LoglineParser
{
    public static Action<LogLineData, Encounter> HandlerLogline(LogLineData line)
    {
        return line.MsgType switch
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
            _ => (LogLineData, Encounter) => { }
        };
    }

    // parse encounter real time
    public static void Parse(JObject json, Encounter encounter)
    {
        var line = Store(json, encounter);
        if (line == null) return;

        var handler = HandlerLogline(line);
        handler.Invoke(line, encounter);

        if (line.Parsed) EncounterManager.Inst.ClientsNotifier.Notify();
    }

    // parse encounter later
    public static void Parse(LogLineData line, Encounter encounter)
    {
        var handler = HandlerLogline(line);
        handler.Invoke(line, encounter);
    }

    public static LogLineData? Store(JObject json, Encounter encounter)
    {
        string? rawLine = json.GetValue("rawLine")?.ToObject<string>();
        if (rawLine == null) return null;

        var line = new LogLineData(rawLine);
        if (line.MsgType == LogLineType.None) return null;

        encounter.RawActions.Add(line);
        return line;
    }

    private static void MsgActionEffect(LogLineData logLineData, Encounter encounter)
    {
        if (logLineData.Parsed) return; // todo ignore parsing
        logLineData.Parse();
        ActionEffect parsed = (ActionEffect)logLineData.Value!;

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
                    var actionValue = DamageHealCalc(attribute.Value);

                    player.DamageDealt.Value.Total += actionValue;
                    player.DamageDealt.Count.Total += 1;
                    encounter.DamageDealt.Value.Total += actionValue;
                    encounter.DamageDealt.Count.Total += 1;

                    if(ActionEffectFlag.IsDirectCrit((int)attribute.Key))
                    {
                        player.DamageDealt.Value.CriticalDirect += actionValue;
                        player.DamageDealt.Count.CriticalDirect += 1;
                        encounter.DamageDealt.Value.CriticalDirect += actionValue;
                        encounter.DamageDealt.Count.CriticalDirect += 1;
                    }
                    else
                    {
                        if(ActionEffectFlag.IsCrit((int)attribute.Key))
                        {
                            player.DamageDealt.Value.Critical += actionValue;
                            player.DamageDealt.Count.Critical += 1;
                            encounter.DamageDealt.Value.Critical += actionValue;
                            encounter.DamageDealt.Count.Critical += 1;
                        }
                        if(ActionEffectFlag.IsDirect((int)attribute.Key))
                        {
                            player.DamageDealt.Value.Direct += actionValue;
                            player.DamageDealt.Count.Direct += 1;
                            encounter.DamageDealt.Value.Direct += actionValue;
                            encounter.DamageDealt.Count.Direct += 1;
                        }
                    }
                }
                else if (ActionEffectFlag.IsHeal((int)attribute.Key))
                {
                    var actionValue = DamageHealCalc(attribute.Value);

                    player.HealDealt.Value.Total += actionValue;
                    player.HealDealt.Count.Total += 1;
                    encounter.HealDealt.Value.Total += actionValue;
                    encounter.HealDealt.Count.Total += 1;

                    if (ActionEffectFlag.IsCritHeal((int)attribute.Key))
                    {
                        player.HealDealt.Count.Critical += 1;
                        player.HealDealt.Value.Critical += actionValue;
                        encounter.HealDealt.Value.Critical += actionValue;
                        encounter.HealDealt.Count.Critical += 1;
                    }
                }
            }

            if (targetIsPlayer)
            {
                var player = encounter.Players[(uint)parsed.TargetId!];

                if (ActionEffectFlag.IsDamage((int)attribute.Key))
                {
                    var actionValue = DamageHealCalc(attribute.Value);

                    player.DamageReceived.Value.Total += actionValue;
                    player.DamageReceived.Count.Total += 1;
                    encounter.DamageReceived.Value.Total += actionValue;
                    encounter.DamageReceived.Count.Total += 1;

                    if(ActionEffectFlag.IsDirectCrit((int)attribute.Key))
                    {
                        player.DamageReceived.Value.CriticalDirect += actionValue;
                        player.DamageReceived.Count.CriticalDirect += 1;
                        encounter.DamageReceived.Value.CriticalDirect += actionValue;
                        encounter.DamageReceived.Count.CriticalDirect += 1;
                    }
                    else
                    {
                        if(ActionEffectFlag.IsCrit((int)attribute.Key))
                        {
                            player.DamageReceived.Value.Critical += actionValue;
                            player.DamageReceived.Count.Critical += 1;
                            encounter.DamageReceived.Value.Critical += actionValue;
                            encounter.DamageReceived.Count.Critical += 1;
                        }
                        if(ActionEffectFlag.IsDirect((int)attribute.Key))
                        {
                            player.DamageReceived.Value.Direct += actionValue;
                            player.DamageReceived.Count.Direct += 1;
                            encounter.DamageReceived.Value.Direct += actionValue;
                            encounter.DamageReceived.Count.Direct += 1;
                        }
                    }

                    if(ActionEffectFlag.IsParriedDamage((int)attribute.Key))
                    {
                        player.DamageReceived.Count.Parry += 1;
                        encounter.DamageReceived.Count.Parry += 1;
                    }

                    if(ActionEffectFlag.IsMiss((int)attribute.Key))
                    {
                        player.DamageReceived.Count.Miss += 1;
                        encounter.DamageReceived.Count.Miss += 1;
                    }

                }
                else if (ActionEffectFlag.IsHeal((int)attribute.Key))
                {
                    var actionValue = DamageHealCalc(attribute.Value);

                    player.HealReceived.Value.Total += actionValue;
                    player.HealReceived.Count.Total += 1;
                    encounter.HealReceived.Value.Total += actionValue;
                    encounter.HealReceived.Count.Total += 1;

                    if (ActionEffectFlag.IsCritHeal((int)attribute.Key))
                    {
                        player.HealReceived.Value.Critical += actionValue;
                        player.HealReceived.Count.Critical += 1;
                        encounter.HealReceived.Value.Critical += actionValue;
                        encounter.HealReceived.Count.Critical += 1;
                    }
                }
            }

            // TODO Pets
            if (ActionFromPet)
            {
                Dalamud.Log.Info("Detected Action From Pet");
            }

            break;
        }
    }

    private static void MsgAOEActionEffect(LogLineData logLineData, Encounter encounter)
    {
        MsgActionEffect(logLineData, encounter); // have same format
    }

    private static void MsgStartsCasting(LogLineData logLineData, Encounter encounter)
    {
        if (!encounter.Finished || logLineData.Parsed) return;
        logLineData.Parse();
        var parsed = (StartsCasting)logLineData.Value!;

        Dalamud.Log.Info("MsgStartsCasting not implemented");
    }

    private static void MsgDoTHoT(LogLineData logLineData, Encounter encounter)
    {
        // dots are calculated even if the player is deactivated, pet actions should probably do the same #for analyzing the data later
        // or add a flag to the damaged saying it was ignored idk

        if (!encounter.Finished || logLineData.Parsed) return;
        logLineData.Parse();
        var parsed = (DoTHoT)logLineData.Value!;

        bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        bool ActionFromPet = false; // ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        var player = encounter.Players[(uint)parsed.TargetId!];
        if (sourceIsPlayer)
        {
            //if (!thisEnconter.Players[parsed.SourceId].IsActive) return;
            if (!parsed.IsHeal) // IsDamage
            {
                player.DamageDealt.Value.Total += parsed.Value;
                player.DamageDealt.Count.Total += 1;
                encounter.DamageDealt.Value.Total += parsed.Value;
                encounter.DamageDealt.Count.Total += 1;
            }
            else
            {
                player.HealDealt.Value.Total += parsed.Value;
                player.HealDealt.Count.Total += 1;
                encounter.HealDealt.Value.Total += parsed.Value;
                encounter.HealDealt.Count.Total += 1;

            }
        }

        if (targetIsPlayer)
        {
            if (!parsed.IsHeal) // IsDamage
            {
                player.DamageReceived.Value.Total += parsed.Value;
                player.DamageReceived.Count.Total += 1;
                encounter.DamageReceived.Value.Total += parsed.Value;
                encounter.DamageReceived.Count.Total += 1;
            }
            else
            {
                player.HealReceived.Value.Total += parsed.Value;
                player.HealReceived.Count.Total += 1;
                encounter.HealReceived.Value.Total += parsed.Value;
                encounter.HealReceived.Count.Total += 1;
            }
        }

        // TODO Pets
        if (ActionFromPet)
        {
            Dalamud.Log.Info("Detected Action From Pet");
        }
    }

    private static void MsgPartyList(LogLineData logLineData, Encounter encounter)
    {
        if (encounter.Finished) return;
        encounter.Update();
    }

    // this will be used to make sure summoners have their pets damage accounted
    private static void MsgAddCombatant(LogLineData logLineData, Encounter encounter)
    {
        if (!encounter.Finished || logLineData.Parsed) return;
        logLineData.Parse();
        var parsed = (AddCombatant)logLineData.Value!;

        // bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        // bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        // bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        // if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        // todo

        if (parsed.IsPet)
        {
            Dalamud.Log.Info("Detected Action From Pet");
        }
        Dalamud.Log.Info("MsgAddCombatant not implemented");
    }

    private static void MsgPlayerStats(LogLineData logLineData, Encounter encounter)
    {
        if (!encounter.Finished || logLineData.Parsed) return;
        logLineData.Parse();
        var parsed = (PlayerStats)logLineData.Value!;

        // bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        // bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        // bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        // if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        Dalamud.Log.Info("MsgPlayerStats not implemented");
    }

    private static void MsgStatusApply(LogLineData logLineData, Encounter encounter)
    {
        if (!encounter.Finished || logLineData.Parsed) return;
        logLineData.Parse();
        var parsed = (StatusApply)logLineData.Value!;

        // bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        // bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        // bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        // if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        Dalamud.Log.Info("MsgStatusApply not implemented");
    }

    private static void MsgStatusRemove(LogLineData logLineData, Encounter encounter)
    {
        if (!encounter.Finished || logLineData.Parsed) return;
        logLineData.Parse();
        var parsed = (StatusRemove)logLineData.Value!;

        // bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        // bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        // bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        // if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        Dalamud.Log.Info("MsgStatusRemove not implemented");
    }

    private static void MsgDeath(LogLineData logLineData, Encounter encounter)
    {
        if (!encounter.Finished || logLineData.Parsed) return;
        logLineData.Parse();
        var parsed = (Death)logLineData.Value!;

        // bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        // bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        // bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        // if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        Dalamud.Log.Info("MsgDeath not implemented");
    }
}
