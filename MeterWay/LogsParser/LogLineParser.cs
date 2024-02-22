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

    private static void MsgActionEffect(LogLineData loglinedata)
    {
        loglinedata.LogLineTypefn = (List<string> data) => { return new ActionEffect(data); };

        if (false) return; // use return here if you dont want to parse this message

        loglinedata.Parse();
        ActionEffect parsed = (ActionEffect)loglinedata.Value!;

        Encounter encounter = EncounterManager.Inst.encounters.Last();

        bool sourceIsPlayer = encounter.Players.ContainsKey(parsed.SourceId);
        bool targetIsPlayer = parsed.TargetId == null ? false : encounter.Players.ContainsKey((uint)parsed.TargetId);
        bool ActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && encounter.Pets.ContainsKey(parsed.SourceId);
        if (!sourceIsPlayer && !targetIsPlayer && !ActionFromPet) return;

        uint actionValue = 0;
        uint rawattribute = 0;
        foreach (KeyValuePair<uint, uint> attribute in parsed.ActionAttributes)
        {
            if (ActionEffectFlag.IsNothing((int)attribute.Key)) break;
            if (ActionEffectFlag.IsSpecial((int)attribute.Key)) { }; // TODO

            if (sourceIsPlayer && encounter.Players[parsed.SourceId].IsActive)
            {
                if (ActionEffectFlag.IsDamage((int)attribute.Key))
                {
                    EncounterManager.StartEncounter();
                    Helpers.Log("is damage");
                    rawattribute = attribute.Value;
                    actionValue = rawattribute >> 16 | rawattribute << 16 & 0x0FFFFFFF;
                    

                    encounter.Players[parsed.SourceId].TotalDamage += actionValue;
                    encounter.TotalDamage += actionValue;
                    encounter.Players[parsed.SourceId].UpdateStats();
                }
                else if (ActionEffectFlag.IsHeal((int)attribute.Key))
                {
                    if (ActionEffectFlag.IsCritHeal((int)attribute.Key))
                    {
                        
                    }
                    // total healing done
                }
            }

            // TODO
            if (targetIsPlayer)
            {
                if (ActionEffectFlag.IsDamage((int)attribute.Key))
                {
                    // total damage taken
                }
                else
                {
                    // total Healing taken
                }
            }

            // TODO
            // if (actionFromPet)
            // {
            //     if (EncounterManager.Inst.encounters.Last().Pets.ContainsKey(parsed.SourceId))
            //     {
            //         if (EncounterManager.Inst.encounters.Last().Players[EncounterManager.Inst.encounters.Last().Pets[parsed.SourceId]] != null)
            //         {
            //             EncounterManager.Inst.encounters.Last().Players[EncounterManager.Inst.encounters.Last().Pets[parsed.SourceId]].TotalDamage += actionValue;
            //             EncounterManager.Inst.encounters.Last().TotalDamage += actionValue;
            //         }
            //     }
            // }
            // else
            // {
            //     EncounterManager.Inst.encounters.Last().Players[parsed.SourceId].TotalDamage += actionValue;
            //     EncounterManager.Inst.encounters.Last().TotalDamage += actionValue;
            // }

            break;
        }
    }

    private static void MsgAOEActionEffect(LogLineData loglinedata)
    {
        MsgActionEffect(loglinedata); // have same format
    }

    private static void MsgStartsCasting(LogLineData loglinedata)
    {
        loglinedata.LogLineTypefn = (List<string> data) => { return new StartsCasting(data); };

        if (true) return; // todo

        loglinedata.Parse();
        var parsed = (StartsCasting)loglinedata.Value!;

        // todo
    }

    private static void MsgDoTHoT(LogLineData loglinedata)
    {
        // dots are calculated even if the player is deactivated, pet actions should probably do the same #for analyzing the data later
        // or add a flag to the damaged saying it was ignored idk

        loglinedata.LogLineTypefn = (List<string> data) => { return new DoTHoT(data); };

        if (false) return; // todo

        loglinedata.Parse();
        var parsed = (DoTHoT)loglinedata.Value!;


        Encounter thisEnconter = EncounterManager.Inst.encounters.Last();

        bool sourceIsPlayer = thisEnconter.Players.ContainsKey(parsed.SourceId);
        bool targetIsPlayer = thisEnconter.Players.ContainsKey(parsed.TargetId);
        bool IsActionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && thisEnconter.Pets.ContainsKey(parsed.SourceId);

        if (!sourceIsPlayer && !targetIsPlayer && !IsActionFromPet) return;



        if (sourceIsPlayer)
        {
            //if (!thisEnconter.Players[parsed.SourceId].IsActive) return;
            if (!parsed.IsHeal) // IsDamage
            {
                // total damage
                thisEnconter.Players[parsed.SourceId].TotalDamage += parsed.Value;
                thisEnconter.TotalDamage += parsed.Value;
            }
            else
            {
                // total healing done

            }
        }

        if (targetIsPlayer)
        {
            if (!parsed.IsHeal) // IsDamage
            {
                // total damage taken
            }
            else
            {
                // total Healing taken
            }
        }

        // TODO Pets

        // if (IsActionFromPet)
        // {
        //     if (!parsed.IsHeal) // IsDamage
        //     {


        //         if (current.Pets.ContainsKey(parsed.SourceId))
        //         {
        //             if (current.Players[current.Pets[parsed.SourceId]] != null)
        //             {
        //                 current.Players[current.Pets[parsed.SourceId]].TotalDamage += parsed.Value;
        //                 current.TotalDamage += parsed.Value;
        //             }
        //         }
        //     }
        //     else
        //     {

        //     }
        // }
    }

    private static void MsgPartyList(LogLineData loglinedata)
    {
        EncounterManager.Inst.encounters.Last().UpdateParty();
        return;
    }

    private static void MsgAddCombatant(LogLineData loglinedata)
    {
        loglinedata.LogLineTypefn = (List<string> data) => { return new AddCombatant(data); };

        if (true) return; // todo

        loglinedata.Parse();
        var parsed = (AddCombatant)loglinedata.Value!;

        // todo

        // this will be used to make sure summoners have their pet

        if (parsed.IsPet)
            if (EncounterManager.Inst.encounters.Last().Players.ContainsKey(parsed.OwnerId) && EncounterManager.Inst.encounters.Last().Players[parsed.OwnerId] != null)
                if (!EncounterManager.Inst.encounters.Last().Pets.ContainsKey(parsed.Id))
                    EncounterManager.Inst.encounters.Last().Pets.Add(parsed.Id, parsed.OwnerId);


    }

    private static void MsgPlayerStats(LogLineData loglinedata)
    {
        loglinedata.LogLineTypefn = (List<string> data) => { return new PlayerStats(data); };

        if (true) return; // todo

        loglinedata.Parse();
        var parsed = (PlayerStats)loglinedata.Value!;

        // todo

    }

    private static void MsgStatusApply(LogLineData loglinedata)
    {
        loglinedata.LogLineTypefn = (List<string> data) => { return new StatusApply(data); };

        if (true) return; // todo

        loglinedata.Parse();
        var parsed = (StatusApply)loglinedata.Value!;

        // todo

    }

    private static void MsgStatusRemove(LogLineData loglinedata)
    {
        loglinedata.LogLineTypefn = (List<string> data) => { return new StatusRemove(data); };

        if (true) return; // todo

        loglinedata.Parse();
        var parsed = (StatusRemove)loglinedata.Value!;

        // todo

    }

    private static void MsgDeath(LogLineData loglinedata)
    {
        loglinedata.LogLineTypefn = (List<string> data) => { return new Death(data); };

        if (true) return; // todo

        loglinedata.Parse();
        var parsed = (Death)loglinedata.Value!;

        // todo

    }
}
