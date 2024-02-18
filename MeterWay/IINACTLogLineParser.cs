using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using FFXIVClientStructs.FFXIV.Common.Math;

using MeterWay;
using MeterWay.managers;

namespace MeterWay.IINACT;

// all message types:

/*
ChatLog = 0,
Territory = 1,
ChangePrimaryPlayer = 2,
AddCombatant = 3,
RemoveCombatant = 4,
PartyList = 11, // 0x0000000B
PlayerStats = 12, // 0x0000000C
StartsCasting = 20, // 0x00000014
ActionEffect = 21, // 0x00000015
AOEActionEffect = 22, // 0x00000016
CancelAction = 23, // 0x00000017
DoTHoT = 24, // 0x00000018
Death = 25, // 0x00000019
StatusAdd = 26, // 0x0000001A
TargetIcon = 27, // 0x0000001B
WaymarkMarker = 28, // 0x0000001C
SignMarker = 29, // 0x0000001D
StatusRemove = 30, // 0x0000001E
Gauge = 31, // 0x0000001F
World = 32, // 0x00000020
Director = 33, // 0x00000021
NameToggle = 34, // 0x00000022
Tether = 35, // 0x00000023
LimitBreak = 36, // 0x00000024
EffectResult = 37, // 0x00000025
StatusList = 38, // 0x00000026
UpdateHp = 39, // 0x00000027
ChangeMap = 40, // 0x00000028
SystemLogMessage = 41, // 0x00000029
StatusList3 = 42, // 0x0000002A
Settings = 249, // 0x000000F9
Process = 250, // 0x000000FA
Debug = 251, // 0x000000FB
PacketDump = 252, // 0x000000FC
Version = 253, // 0x000000FD
Error = 254, // 0x000000FE
*/

public static class LoglineParser
{
    // we are only getting the messages needed:
    private static Dictionary<uint, Action<List<string>, Encounter>> Handlers = new Dictionary<uint, Action<List<string>, Encounter>>
        {
            { 21, MsgActionEffect },
            { 22, MsgAOEActionEffect },
            { 20, MsgStartsCasting },
            { 24, MsgDoTHoT }
        };

    public static void Parse(JObject json, Encounter recipient)
    {
        var data = json["line"];
        if (data == null) return;
        var msgidvalue = data[0];
        if (msgidvalue == null) return;
        var msgid = msgidvalue.ToObject<uint>();

        List<string> linedata = data.Values<string>().ToList();

#if false //debug
            var message = json["rawLine"]?.ToString() ?? "";
            PluginManager.Instance.PluginLog.Info($"parsed a: \"{messagetype}\" | \"{message}\"");
#endif

        try
        {
            if (Handlers.ContainsKey(msgid)) Handlers[msgid].Invoke(linedata, recipient);
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
        uint Hp = Convert.ToUInt32(data[34].ToString());

        Vector4 targetPos;
        if (data[33].ToString() != "" || data[32].ToString() != "" || data[31].ToString() != "" || data[30].ToString() != "")
        {
            targetPos = new Vector4((float)Convert.ToDouble(data[30].ToString()), (float)Convert.ToDouble(data[31].ToString()),
                (float)Convert.ToDouble(data[32].ToString()), (float)Convert.ToDouble(data[33].ToString()));
        }

        // var separator = data[29]; // null
        // var separator = data[28]; // null
        int? targetmaxMp = data[27].ToString() == "" ? null : Convert.ToInt32(data[27].ToString());
        int? targetmp = data[26].ToString() == "" ? null : Convert.ToInt32(data[26].ToString());
        int? targetmaxHp = data[25].ToString() == "" ? null : Convert.ToInt32(data[25].ToString());
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

        UInt32? value = null;
        if (data[9].ToString() != "")
        {
            int tempvaluehex = Convert.ToInt32(data[9].ToString(), 16);
            value = (UInt32)((UInt32)(tempvaluehex >> 16) | (UInt32)((tempvaluehex << 16)) & 0x0FFFFFFF);
        }

        int type = Convert.ToInt32(data[8].ToString(), 16);

        string? target = data[7].ToString() == "" ? null : data[7].ToString();
        int? targetid = data[6].ToString() == "" ? null : Convert.ToInt32(data[6].ToString(), 16);
        string action = data[5].ToString();
        int actionid = Convert.ToInt32(data[4].ToString(), 16);
        string player = data[3].ToString();
        uint playerid = Convert.ToUInt32(data[2].ToString(), 16);

        var datetime = data[1].ToString();

        // int messagetype = Convert.ToInt32(data[0].ToString())



        if (recipient.Players.ContainsKey(playerid))
        {
            if (EffectEntryType.IsDamage(type))
            {
                recipient.Players[playerid].TotalDamage += value ?? 0;
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


    public static class EffectEntryType
    {
        public enum Flag
        {
            Nothing = 0,
            Miss = 1,
            FullResist = 2,
            Damage = 3,
            CritHit = 0x2000,
            DirectHit = 0x4000,
            DirectCritHit = 0x6000,
            Heal = 4,
            CritHeal = 0x200004,
            BlockedDamage = 5,
            ParriedDamage = 6,
            Invulnerable = 7,
            ApplyStatusEffectTarget = 0x0E,
            ApplyStatusEffectSource = 0x0F,
            RecoveredFromStatusEffect = 0x10,
            LoseStatusEffectTarget = 0x11,
            LoseStatusEffectSource = 0x12,
            FullResistStatus = 0x37,
            Interrupt = 0x4B,

        }

        public static bool IsNothing(int value) => (value & 0xF) == (int)Flag.Nothing;
        public static bool IsMiss(int value) => (value & 0xF) == (int)Flag.Miss;
        public static bool IsFullResist(int value) => (value & 0xF) == (int)Flag.FullResist;
        public static bool IsDamage(int value) => (value & 0xF) == (int)Flag.Damage;
        public static bool IsCrit(int value) => (value & 0xFFFF) == (int)Flag.CritHit;
        public static bool IsDirect(int value) => (value & 0xFFFF) == (int)Flag.DirectHit;
        public static bool IsDirectCrit(int value) => (value & 0xFFFF) == (int)Flag.DirectCritHit;

        public static bool IsHeal(int value) => (value & 0xF) == (int)Flag.Heal;
        public static bool IsCritHeal(int value) => (value & 0xFFFFFF) == (int)Flag.CritHeal;
        public static bool IsBlockedDamage(int value) => (value & 0xF) == (int)Flag.BlockedDamage;
        public static bool IsParriedDamage(int value) => (value & 0xF) == (int)Flag.ParriedDamage;
        public static bool IsInvulnerable(int value) => (value & 0xF) == (int)Flag.Invulnerable;
        public static bool IsApplyStatusEffectTarget(int value) => (value & 0xF) == (int)Flag.ApplyStatusEffectTarget;
        public static bool IsApplyStatusEffectSource(int value) => (value & 0xF) == (int)Flag.ApplyStatusEffectSource;
        public static bool IsRecoveredFromStatusEffect(int value) => (value & 0xF) == (int)Flag.RecoveredFromStatusEffect;
        public static bool IsLoseStatusEffectTarget(int value) => (value & 0xF) == (int)Flag.LoseStatusEffectTarget;
        public static bool IsLoseStatusEffectSource(int value) => (value & 0xF) == (int)Flag.LoseStatusEffectSource;
        public static bool IsFullResistStatus(int value) => (value & 0xF) == (int)Flag.FullResistStatus;
        public static bool IsInterrupt(int value) => (value & 0xF) == (int)Flag.Interrupt;


        public static List<Flag> ParseFlags(int value)
        {
            List<Flag> parsedFlags = new List<Flag>();

            if (IsNothing(value))
            {
                parsedFlags.Add(Flag.Nothing);
                return parsedFlags;
            };
            if (IsMiss(value))
            {
                parsedFlags.Add(Flag.Miss);
            };
            if (IsFullResist(value))
            {
                parsedFlags.Add(Flag.FullResist);
            };
            // HITS AND STUFF
            if (IsDamage(value))
            {
                if (IsCrit(value))
                {
                    parsedFlags.Add(Flag.CritHit);
                }
                else if (IsDirect(value))
                {
                    parsedFlags.Add(Flag.DirectHit);
                }
                else if (IsDirectCrit(value))
                {
                    parsedFlags.Add(Flag.DirectCritHit);
                }
                parsedFlags.Add(Flag.Damage);
            };
            // HEALS AND STUFF
            if (IsHeal(value) && !IsCritHeal(value))
            {
                if (IsCritHeal(value))
                {
                    parsedFlags.Add(Flag.CritHeal);
                }
                else
                {
                    parsedFlags.Add(Flag.Heal);
                }
            };
            if (IsBlockedDamage(value))
            {
                parsedFlags.Add(Flag.BlockedDamage);
            };
            if (IsParriedDamage(value))
            {
                parsedFlags.Add(Flag.ParriedDamage);
            }
            if (IsInvulnerable(value))
            {
                parsedFlags.Add(Flag.Invulnerable);
            };
            // APLLY EFFECTS FROM TARGET
            if (IsApplyStatusEffectTarget(value))
            {
                parsedFlags.Add(Flag.ApplyStatusEffectTarget);
            };
            // APPLY EFFECTS FROM SOURCE
            if (IsApplyStatusEffectSource(value))
            {
                parsedFlags.Add(Flag.ApplyStatusEffectSource);
            };
            if (IsRecoveredFromStatusEffect(value))
            {
                parsedFlags.Add(Flag.RecoveredFromStatusEffect);
            };
            // CLEAR AFFLICTED STATUS FROM TARGET
            if (IsLoseStatusEffectTarget(value))
            {
                parsedFlags.Add(Flag.LoseStatusEffectTarget);
            };
            // CLEAR AFFLICTED STATUS FROM SOURCE
            if (IsLoseStatusEffectSource(value))
            {
                parsedFlags.Add(Flag.LoseStatusEffectSource);
            };
            // RESIST STATUS
            if (IsFullResistStatus(value))
            {
                parsedFlags.Add(Flag.FullResistStatus);
            };
            if (IsFullResistStatus(value))
            {
                parsedFlags.Add(Flag.Interrupt);
            };

            return parsedFlags;
        }
    }
}
