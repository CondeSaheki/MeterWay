using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using FFXIVClientStructs.FFXIV.Common.Math;

using MeterWay.managers;
using MeterWay.Utils;

namespace MeterWay.IINACT;

public static class LoglineParser
{
    // we are only getting the messages needed:
    private static Dictionary<MessageType, Action<List<string>, string, DataManager>> Handlers = new Dictionary<MessageType, Action<List<string>, string, DataManager>>
        {
            { MessageType.ActionEffect, MsgActionEffect },
            { MessageType.AOEActionEffect, MsgAOEActionEffect },
            { MessageType.StartsCasting, MsgStartsCasting },
            { MessageType.DoTHoT, MsgDoTHoT },
            { MessageType.PartyList, MsgPartyList }
                };

    public static void Parse(JObject json, DataManager recipient)
    {
        var data = json["line"];
        if (data == null) return;
        var messageTypeValue = data[0];
        if (messageTypeValue == null) return;
        var messageType = messageTypeValue.ToObject<MessageType>();
        var rawValue = json["rawLine"];
        if (rawValue == null) return;
        string raw = rawValue.ToObject<string>() ?? "";

        List<string> linedata = data.Values<string>().ToList().ConvertAll(x => x ?? string.Empty);

#if false //debug
            var message = json["rawLine"]?.ToString() ?? "";
            PluginManager.Instance.PluginLog.Info($"parsed a: \"{messagetype}\" | \"{message}\"");
#endif

        try
        {
            if (Handlers.ContainsKey(messageType)) Handlers[messageType].Invoke(linedata, raw, recipient);
        }
        catch (Exception ex)
        {
            PluginManager.Instance.PluginLog.Warning("fail -> " + ex.ToString());
        }
    }

    private static void MsgActionEffect(List<string> data, string raw, DataManager recipient)
    {
        var parsed = new ActionEffect(data, raw);

        bool found = false;
        if (recipient.encounters.Last().Players.ContainsKey(parsed.ObjectId))
        {
            recipient.encounters.Last().Players[parsed.ObjectId].RawActions.Add(parsed);
            found = true;

            if (!recipient.encounters.Last().Players[parsed.ObjectId].InParty && !(PluginManager.Instance.DutyState.IsDutyStarted))
            {
              return;
            }
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
                if (!recipient.encounters.Last().active) recipient.encounters.Last().StartEncounter();

                rawattribute = attribute.Value;
                actionValue = (UInt32)((UInt32)(attribute.Value >> 16) | (UInt32)((attribute.Value << 16)) & 0x0FFFFFFF);

                recipient.encounters.Last().Players[parsed.ObjectId].TotalDamage += actionValue;
                recipient.encounters.Last().TotalDamage += actionValue;
            }
        }

        //PluginManager.Instance.PluginLog.Info($"{parsed.ObjectId} | {parsed.Name} | {(parsed.TargetId == null ? "null" : parsed.TargetId)} | {actionValue} |  {actionValue} ");
    }

    private static void MsgAOEActionEffect(List<string> data, string raw, DataManager recipient)
    {
        MsgActionEffect(data, raw, recipient);
    }

    private static void MsgStartsCasting(List<string> data, string raw, DataManager recipient)
    {

    }

    private static void MsgDoTHoT(List<string> data, string raw, DataManager recipient)
    {
        var parsed = new DoTHoT(data, raw);

        bool found = false;
        if (recipient.encounters.Last().Players.ContainsKey(parsed.SourceId))
        {
            recipient.encounters.Last().Players[parsed.SourceId].RawActions.Add(parsed);
            found = true;

            if (!recipient.encounters.Last().Players[parsed.SourceId].InParty && !(PluginManager.Instance.DutyState.IsDutyStarted))
            {
              return;
            }
        }
        else if (recipient.encounters.Last().Players.ContainsKey((uint)parsed.TargetId))
        {
            recipient.encounters.Last().RawActions.Add(parsed);
            found = true;
        }
        if (!found) return;

        if (!parsed.IsHeal)
        {
            if (!recipient.encounters.Last().active) recipient.encounters.Last().StartEncounter();

            recipient.encounters.Last().Players[parsed.SourceId].TotalDamage += parsed.Value;
            recipient.encounters.Last().TotalDamage += parsed.Value;

            PluginManager.Instance.PluginLog.Info($"DOT: {parsed.RawLine}");
        }

    }

    private static void MsgPartyList(List<string> data, string raw, DataManager recipient)
    {
        recipient.encounters.Last().UpdateParty();
    }

}