using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using MeterWay.Managers;
using MeterWay.Utils;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;

namespace MeterWay.LogParser;

public static class LoglineParser
{
    // we are only getting the messages needed
    private static Dictionary<MessageType, Action<List<string>, string, EncounterManager>> Handlers = new Dictionary<MessageType, Action<List<string>, string, EncounterManager>>
        {
            { MessageType.ActionEffect, MsgActionEffect },
            { MessageType.AOEActionEffect, MsgAOEActionEffect },
            { MessageType.StartsCasting, MsgStartsCasting },
            { MessageType.DoTHoT, MsgDoTHoT },
            { MessageType.PartyList, MsgPartyList },
            { MessageType.AddCombatant, MsgAddCombatant },
            { MessageType.PlayerStats, MsgPlayerStats },
            { MessageType.StatusApply, MsgStatusApply },
            { MessageType.StatusRemove, MsgStatusRemove },
            { MessageType.Death, MsgDeath },

        };

    public static void Parse(JObject json, EncounterManager recipient)
    {
        var data = json["line"];
        if (data == null) return;
        var messageTypeValue = data[0];
        if (messageTypeValue == null) return;
        var messageType = messageTypeValue.ToObject<MessageType>();
        var rawValue = json["rawLine"];
        if (rawValue == null) return;
        string raw = rawValue.ToObject<string>() ?? string.Empty;
        if (raw == "") return;

        List<string> linedata = data.Values<string>().ToList().ConvertAll(x => x ?? string.Empty);

        try
        {
            if (Handlers.ContainsKey(messageType)) Handlers[messageType].Invoke(linedata, raw, recipient);
        }
        catch (Exception ex)
        {
            InterfaceManager.Inst.PluginLog.Warning($"fail -> {rawValue} \n {ex.ToString()}");
        }
    }

    private static void MsgActionEffect(List<string> data, string raw, EncounterManager recipient)
    {
        var parsed = new ActionEffect(data, raw);

        bool found = false;

        bool actionFromPet = false;
        if (recipient.encounters.Last().Players.ContainsKey(parsed.SourceId))
        {
            recipient.encounters.Last().Players[parsed.SourceId].RawActions.Add(parsed);
            found = true;

            if (!recipient.encounters.Last().Players[parsed.SourceId].IsActive && !InterfaceManager.Inst.DutyState.IsDutyStarted)
            {
                return;
            }

            actionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && recipient.encounters.Last().Pets.ContainsKey(parsed.SourceId);
        }
        else if (parsed.TargetId != null && recipient.encounters.Last().Players.ContainsKey((uint)parsed.TargetId))
        {
            recipient.encounters.Last().RawActions.Add(parsed);
            found = true;
        }
        if (!found) return;

        uint actionValue = 0;
        uint rawattribute = 0;
        foreach (KeyValuePair<uint, uint> attribute in parsed.ActionAttributes)
        {

            if (ParserAssistant.IsNothing((int)attribute.Key)) break;
            if (ParserAssistant.IsSpecial((int)attribute.Key)) { }; // TODO
            if (ParserAssistant.IsDamage((int)attribute.Key))
            {
                if (!recipient.encounters.Last().Active) recipient.encounters.Last().StartEncounter();
                rawattribute = attribute.Value;
                actionValue = (UInt32)((UInt32)(attribute.Value >> 16) | (UInt32)((attribute.Value << 16)) & 0x0FFFFFFF);

                if (!recipient.encounters.Last().Players.ContainsKey(parsed.SourceId))
                {
                    Helpers.Log($"Damage from {parsed.Name} not accounted because they are not in the Party.");
                }
                else if (recipient.encounters.Last().Players[parsed.SourceId].IsActive)
                {
                    // pet actions should probably be calculated even if the owner is deactivated #for analyzing the data later
                    // or add a flag to the damaged saying it was ignored idk
                    if (actionFromPet)
                    {
                        if (recipient.encounters.Last().Pets.ContainsKey(parsed.SourceId))
                        {
                            if (recipient.encounters.Last().Players[recipient.encounters.Last().Pets[parsed.SourceId]] != null)
                            {
                                recipient.encounters.Last().Players[recipient.encounters.Last().Pets[parsed.SourceId]].TotalDamage += actionValue;
                                recipient.encounters.Last().TotalDamage += actionValue;
                            }
                        }
                    }
                    else
                    {
                        recipient.encounters.Last().Players[parsed.SourceId].TotalDamage += actionValue;
                        recipient.encounters.Last().TotalDamage += actionValue;
                    }
                }
                else
                {
                    Helpers.Log($"{recipient.encounters.Last().Players[parsed.SourceId].Name} is deactivated, ignoring damage.");
                }
            }
        }

        EncounterManager.UpdateClients();
    }

    private static void MsgAOEActionEffect(List<string> data, string raw, EncounterManager recipient)
    {
        MsgActionEffect(data, raw, recipient); // have same format
    }

    private static void MsgStartsCasting(List<string> data, string raw, EncounterManager recipient)
    {
        // todo
    }

    private static void MsgDoTHoT(List<string> data, string raw, EncounterManager recipient)
    {
        var parsed = new DoTHoT(data, raw);

        bool found = false;

        bool actionFromPet = false;
        if (recipient.encounters.Last().Players.ContainsKey(parsed.SourceId))
        {
            recipient.encounters.Last().Players[parsed.SourceId].RawActions.Add(parsed);
            found = true;

            if (!recipient.encounters.Last().Players[parsed.SourceId].IsActive && !(InterfaceManager.Inst.DutyState.IsDutyStarted))
            {
                return;
            }

            actionFromPet = ((parsed.SourceId >> 24) & 0xFF) == 64 && recipient.encounters.Last().Pets.ContainsKey(parsed.SourceId);
        }
        else if (recipient.encounters.Last().Players.ContainsKey((uint)parsed.TargetId))
        {
            recipient.encounters.Last().RawActions.Add(parsed);
            found = true;
        }
        if (!found) return;

        if (!parsed.IsHeal)
        {
            if (!recipient.encounters.Last().Active) recipient.encounters.Last().StartEncounter();
            // dots are calculated even if the player is deactivated, pet actions should probably do the same #for analyzing the data later
            // or add a flag to the damaged saying it was ignored idk
            // if (recipient.encounters.Last().Players[parsed.SourceId].IsActive)
            // {
            if (actionFromPet)
            {
                if (recipient.encounters.Last().Pets.ContainsKey(parsed.SourceId))
                {
                    if (recipient.encounters.Last().Players[recipient.encounters.Last().Pets[parsed.SourceId]] != null)
                    {
                        recipient.encounters.Last().Players[recipient.encounters.Last().Pets[parsed.SourceId]].TotalDamage += parsed.Value;
                        recipient.encounters.Last().TotalDamage += parsed.Value;
                    }
                }
            }
            else
            {
                if (!recipient.encounters.Last().Players.ContainsKey(parsed.SourceId))
                {
                    Helpers.Log($"Damage from {parsed.SourceName} not accounted because they are not in the Party.");
                }
                else
                {
                    recipient.encounters.Last().Players[parsed.SourceId].TotalDamage += parsed.Value;
                    recipient.encounters.Last().TotalDamage += parsed.Value;
                }
            }
            // }

        }
        EncounterManager.UpdateClients();
    }

    private static void MsgPartyList(List<string> data, string raw, EncounterManager recipient)
    {
        recipient.encounters.Last().UpdateParty();
        EncounterManager.UpdateClients();
    }

    private static void MsgAddCombatant(List<string> data, string raw, EncounterManager recipient)
    {
        var parsed = new AddCombatant(data, raw);
        // this will be used to make sure summoners have their pet

        if (parsed.IsPet)
            if (recipient.encounters.Last().Players.ContainsKey(parsed.OwnerId) && recipient.encounters.Last().Players[parsed.OwnerId] != null)
                if (!recipient.encounters.Last().Pets.ContainsKey(parsed.Id))
                    recipient.encounters.Last().Pets.Add(parsed.Id, parsed.OwnerId);

        EncounterManager.UpdateClients();
    }

    private static void MsgPlayerStats(List<string> data, string raw, EncounterManager recipient)
    { 
        // todo
    }
    private static void MsgStatusApply(List<string> data, string raw, EncounterManager recipient)
    {
        // todo
    }

    private static void MsgStatusRemove(List<string> data, string raw, EncounterManager recipient)
    {
        // todo
    }

    private static void MsgDeath(List<string> data, string raw, EncounterManager recipient)
    {
        //var parsed = new Message(false, data, raw, Death);
        // todo
    }

}
