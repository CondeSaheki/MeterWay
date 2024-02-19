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
            { MessageType.DoTHoT, MsgDoTHoT }
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
        if (recipient.current.Players.ContainsKey(parsed.ObjectId))
        {
            recipient.current.Players[parsed.ObjectId].RawActions.Add(parsed);
            found = true;
        }
        else if (parsed.TargetId != null && recipient.current.Players.ContainsKey((uint)parsed.TargetId))
        {
            recipient.current.RawActions.Add(parsed);
            found = true;
        }
        if(!found) return;

        if (!recipient.current.active) recipient.StartEncounter();

        uint actionValue = 0;
        uint rawattribute = 0;
        foreach (KeyValuePair<uint, uint> attribute in parsed.ActionAttributes)
        {

            if (ParserAssistant.IsNothing((int)attribute.Key)) break;
            if (ParserAssistant.IsSpecial((int)attribute.Key)) { }; // TODO
            if (ParserAssistant.IsDamage((int)attribute.Key))
            {
                rawattribute = attribute.Value;
                actionValue = (UInt32)((UInt32)(attribute.Value >> 16) | (UInt32)((attribute.Value << 16)) & 0x0FFFFFFF);

                recipient.current.Players[parsed.ObjectId].TotalDamage += actionValue;
                recipient.current.TotalDamage += actionValue;
            }
        }

        PluginManager.Instance.PluginLog.Info($"{parsed.ObjectId} | {parsed.Name} | {(parsed.TargetId == null ? "null" : parsed.TargetId)} | {actionValue} |  {actionValue} ");
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

    }

    public interface INetworkMessage
    {
        public string Id { get; }
        public DateTime DateTime { get; }
    }

    public class action21 : INetworkMessage
    {

        public string Id { get; }
        public DateTime DateTime { get; }

        public action21()
        {

        }
    }



}
