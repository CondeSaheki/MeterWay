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
    private static Dictionary<MessageType, Action<List<string>, Encounter>> Handlers = new Dictionary<MessageType, Action<List<string>, Encounter>>
        {
            { MessageType.ActionEffect, MsgActionEffect },
            { MessageType.AOEActionEffect, MsgAOEActionEffect },
            { MessageType.StartsCasting, MsgStartsCasting },
            { MessageType.DoTHoT, MsgDoTHoT }
        };

    public static void Parse(JObject json, Encounter recipient)
    {
        var data = json["line"];
        if (data == null) return;
        var messageTypeValue = data[0];
        if (messageTypeValue == null) return;
        var messageType = messageTypeValue.ToObject<MessageType>();

        List<string> linedata = data.Values<string>().ToList().ConvertAll(x => x ?? string.Empty);

#if false //debug
            var message = json["rawLine"]?.ToString() ?? "";
            PluginManager.Instance.PluginLog.Info($"parsed a: \"{messagetype}\" | \"{message}\"");
#endif

        try
        {
            if (Handlers.ContainsKey(messageType)) Handlers[messageType].Invoke(linedata, recipient);
        }
        catch (Exception ex)
        {
            PluginManager.Instance.PluginLog.Warning("fail -> " + ex.ToString());
        }
    }

    private static void MsgActionEffect(List<string> data, Encounter recipient)
    {
        // int crypto = Convert.ToInt32(data[47].ToString(), 16);
        // uint multimessagecount = Convert.ToUInt32(data[46].ToString());
        // uint multimessageindex = Convert.ToUInt32(data[45].ToString());
        // uint loglinescount = Convert.ToUInt32(data[44].ToString(), 16);

        Vector4 pos = new Vector4((float)Convert.ToDouble(data[40].ToString()), (float)Convert.ToDouble(data[41].ToString()),
            (float)Convert.ToDouble(data[42].ToString()), (float)Convert.ToDouble(data[43].ToString()));

        // var separator = data[39]; // null
        // var separator = data[38]; // null
        uint maxMp = Convert.ToUInt32(data[37].ToString());
        uint mp = Convert.ToUInt32(data[36].ToString());
        uint maxHp = Convert.ToUInt32(data[35].ToString());
        uint hp = Convert.ToUInt32(data[34].ToString());

        Vector4 targetPos;
        if (data[33].ToString() != "" || data[32].ToString() != "" || data[31].ToString() != "" || data[30].ToString() != "")
        {
            targetPos = new Vector4((float)Convert.ToDouble(data[30].ToString()), (float)Convert.ToDouble(data[31].ToString()),
                (float)Convert.ToDouble(data[32].ToString()), (float)Convert.ToDouble(data[33].ToString()));
        }

        // var separator = data[29]; // null
        // var separator = data[28]; // null
        int? targetMaxMp = data[27].ToString() == "" ? null : Convert.ToInt32(data[27].ToString());
        int? targetMp = data[26].ToString() == "" ? null : Convert.ToInt32(data[26].ToString());
        int? targetMaxHp = data[25].ToString() == "" ? null : Convert.ToInt32(data[25].ToString());
        int? targetHp = data[24].ToString() == "" ? null : Convert.ToInt32(data[24].ToString());

        // var skillatributes = data[23];
        // var skillatributes = data[22];
        // var skillatributes = data[21];
        // var skillatributes = data[20];
        // var skillatributes = data[19];
        // var skillatributes = data[18];
        // var skillatributes = data[17];
        // var skillatributes = data[16];
        // var skillatributes = data[15];
        // var skillatributes = data[14];
        // var skillatributes = data[13];
        // var skillatributes = data[12];
        // var skillatributes = data[11];
        // var skillatributes = data[10];

        UInt32? actionValue = null;
        if (data[9].ToString() != "")
        {
            int _tempValueHex = Convert.ToInt32(data[9].ToString(), 16);
            actionValue = (UInt32)((UInt32)(_tempValueHex >> 16) | (UInt32)((_tempValueHex << 16)) & 0x0FFFFFFF);
        }

        int actionTraits = Convert.ToInt32(data[8].ToString(), 16);

        string? target = data[7].ToString() == "" ? null : data[7].ToString();
        int? targetid = data[6].ToString() == "" ? null : Convert.ToInt32(data[6].ToString(), 16);
        string action = data[5].ToString();
        int actionid = Convert.ToInt32(data[4].ToString(), 16);
        string player = data[3].ToString();
        uint playerid = Convert.ToUInt32(data[2].ToString(), 16);

        var datetime = data[1].ToString();

        if (recipient.Players.ContainsKey(playerid))
        {
            if (ParserAssistant.IsDamage(actionTraits))
            {
                //Total Damage
                recipient.Players[playerid].TotalDamage += actionValue ?? 0;
                recipient.TotalDamage += actionValue ?? 0;

                //DPST
                recipient.Players[playerid].DPS = recipient.duration.TotalSeconds != 0 ? (float)(recipient.Players[playerid].TotalDamage / recipient.duration.TotalSeconds) : 0;

                //DMG PCT
                recipient.Players[playerid].DamagePercentage = recipient.TotalDamage != 0 ? (int)(recipient.Players[playerid].TotalDamage / recipient.TotalDamage) * 100 : 0;
            }
        }

        //PluginManager.Instance.PluginLog.Info($"{player} | {action} | {(target == null ? "null" : target)} | {(value == null ? "null" : data[8].ToString())} ");
    }

    private static void MsgAOEActionEffect(List<string> data, Encounter recipient)
    {
        MsgActionEffect(data, recipient);
    }

    private static void MsgStartsCasting(List<string> data, Encounter recipient)
    {
        // uint sourceId;
        // string sourceName;
        // uint targetId;
        // string targetName;
        // uint skillId;
        // string skillName;
        // float Duration;
        // float? posX;
        // float? posY;
        // float? posZ;
        // float? heading;
    }

    private static void MsgDoTHoT(List<string> data, Encounter recipient)
    {
        // uint targetId;
        // string targetName;
        // bool IsHeal;
        // uint buffId;
        // uint amount;
        // uint? targetCurrentHp;
        // uint? targetMaxHp;
        // uint? targetCurrentMp;
        // uint? targetMaxMp;
        // float? targetPosX;
        // float? targetPosY;
        // float? targetPosZ;
        // float? targetHeading;
        // uint? sourceId;
        // string sourceName;
        // uint damageType;
        // uint? sourceCurrentHp;
        // uint? sourceMaxHp;
        // uint? sourceCurrentMp;
        // uint? sourceMaxMp;
        // float? sourcePosX;
        // float? sourcePosY;
        // float? sourcePosZ;
        // float? sourceHeading;
    }
}
