using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using MeterWay.Managers;
using MeterWay.Data;
using MeterWay.Utils;

namespace MeterWay.LogParser;

public static class LoglineParser
{
    // we are only getting the messages needed
    private static readonly Dictionary<LogLineType, Action<LogLineData>> Handlers = new()
    {
        { LogLineType.ActionEffect, MsgActionEffect },
        { LogLineType.AOEActionEffect, MsgAOEActionEffect },
        { LogLineType.StartsCasting, MsgStartsCasting },
        { LogLineType.DoTHoT, MsgDoTHoT },
        { LogLineType.PartyList, MsgPartyList },
        { LogLineType.AddCombatant, MsgAddCombatant },
        { LogLineType.PlayerStats, MsgPlayerStats },
        { LogLineType.StatusApply, MsgStatusApply },
        { LogLineType.StatusRemove, MsgStatusRemove },
        { LogLineType.Death, MsgDeath }
    };

    public static void Parse(JObject json)
    {
        string? rawValue = json.GetValue("rawLine")?.ToObject<string>();
        if (rawValue == null) return;

        JToken? item0 = json.GetValue("line")?[0], item1 = json.GetValue("line")?[1];
        if (item0 == null || item1 == null) return;

        LogLineType messageType = item0.ToObject<LogLineType>();
        DateTime messageDateTime = DateTime.Parse(item1.ToString());

        if (Handlers.ContainsKey(messageType))
        {
            LogLineData commonData = new(messageType, messageDateTime, rawValue);
            try
            {
                Handlers[messageType].Invoke(commonData);
            }
            catch (Exception ex)
            {
                InterfaceManager.Inst.PluginLog.Warning($"Failed to parse LogLine {messageType} - {((uint)messageType).ToString()} ->\n {rawValue} \n Error -> \n {ex.ToString()}");
            }
            if (commonData.Parsed) EncounterManager.UpdateClients();
        }
    }

    private static void MsgActionEffect(LogLineData logLineData)
    {
        logLineData.LogLineTypefn = (List<string> data) => { return new ActionEffect(data); };
        
        if (false) return; // todo ignore parsing

        logLineData.Parse();
        ActionEffect parsed = (ActionEffect)logLineData.Value!;

        Encounter encounter = EncounterManager.Inst.encounters.Last();

        bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        bool targetIsPlayer = parsed.TargetId == null ? false : encounter.Players.ContainsKey((uint)parsed.TargetId);
        bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        foreach (KeyValuePair<uint, uint> attribute in parsed.ActionAttributes)
        {
            if (ActionEffectFlag.IsNothing((int)attribute.Key)) break;
            if (ActionEffectFlag.IsSpecial((int)attribute.Key)) { }; // TODO

            if (sourceIsPlayer && encounter.Players[parsed.SourceId].IsActive)
            {
                var player = encounter.Players[parsed.SourceId];
                if (ActionEffectFlag.IsDamage((int)attribute.Key))
                {
                    EncounterManager.StartEncounter(); // gamer

                    var actionValue = attribute.Value >> 16 | attribute.Value << 16 & 0x0FFFFFFF;
                    player.Damage.Total += actionValue;
                    encounter.Damage.Total += actionValue;
                }
                else if (ActionEffectFlag.IsHeal((int)attribute.Key))
                {
                    if (ActionEffectFlag.IsCritHeal((int)attribute.Key))
                    {
                        player.Healing.Count.Crit += 1;
                        player.Healing.TotalCrit += attribute.Value;
                    }
                    player.Healing.Total += attribute.Value;
                }
            }

            if (targetIsPlayer)
            {
                var player = encounter.Players[(uint)parsed.TargetId!];
                if (ActionEffectFlag.IsDamage((int)attribute.Key))
                {
                    player.DamageTaken.Total = attribute.Value >> 16 | attribute.Value << 16 & 0x0FFFFFFF;
                }
                else if (ActionEffectFlag.IsHeal((int)attribute.Key))
                {
                    player.HealingTaken.Total = attribute.Value >> 16 | attribute.Value << 16 & 0x0FFFFFFF;
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
        logLineData.LogLineTypefn = (List<string> data) => { return new StartsCasting(data); };

        if (true) return; // todo

        logLineData.Parse();
        var parsed = (StartsCasting)logLineData.Value!;

        // todo
    }

    private static void MsgDoTHoT(LogLineData logLineData)
    {
        // dots are calculated even if the player is deactivated, pet actions should probably do the same #for analyzing the data later
        // or add a flag to the damaged saying it was ignored idk

        logLineData.LogLineTypefn = (List<string> data) => { return new DoTHoT(data); };

        if (false) return; // todo ignore parsing
        logLineData.Parse();
        var parsed = (DoTHoT)logLineData.Value!;

        Encounter encounter = EncounterManager.Inst.encounters.Last();

        bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        bool targetIsPlayer = encounter.Players.ContainsKey((uint)parsed.TargetId);
        bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
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
                encounter.Players[parsed.SourceId].HealingTaken.Total += parsed.Value;
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
        EncounterManager.Inst.encounters.Last().UpdateParty();
        return;
    }

    // this will be used to make sure summoners have their pets damage accounted
    private static void MsgAddCombatant(LogLineData logLineData)
    {
        logLineData.LogLineTypefn = (List<string> data) => { return new AddCombatant(data); };

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
            if (EncounterManager.Inst.encounters.Last().Players.ContainsKey(parsed.OwnerId) && EncounterManager.Inst.encounters.Last().Players[parsed.OwnerId] != null)
                if (!EncounterManager.Inst.encounters.Last().Pets.ContainsKey(parsed.Id))
                    EncounterManager.Inst.encounters.Last().Pets.Add(parsed.Id, parsed.OwnerId);
    }

    private static void MsgPlayerStats(LogLineData logLineData)
    {
        logLineData.LogLineTypefn = (List<string> data) => { return new PlayerStats(data); };

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
        logLineData.LogLineTypefn = (List<string> data) => { return new StatusApply(data); };

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
        logLineData.LogLineTypefn = (List<string> data) => { return new StatusRemove(data); };

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
        logLineData.LogLineTypefn = (List<string> data) => { return new Death(data); };

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
