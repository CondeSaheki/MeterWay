using System.Collections.Generic;

namespace MeterWay.Utils;

public enum MessageType : uint
{
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
}

public static class ParserAssistant
{
    public enum DamageType
    {
        Magic = 0x50000,
        Physical = 0x10000,
        MagicMelee = 0x30000,
        Ranged = 0x20000,
    }
    public enum EffectEntryType
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
        Unknown_3F = 0x3F,

    }

    public static bool IsNothing(int flag) => (flag & 0xF) == (int)EffectEntryType.Nothing;
    public static bool IsMiss(int flag) => (flag & 0xF) == (int)EffectEntryType.Miss;
    public static bool IsFullResist(int flag) => (flag & 0xF) == (int)EffectEntryType.FullResist;
    public static bool IsDamage(int flag) => (flag & 0xF) == (int)EffectEntryType.Damage;
    public static bool IsCrit(int flag) => (flag & 0xF000) == (int)EffectEntryType.CritHit;
    public static bool IsDirect(int flag) => (flag & 0xF000) == (int)EffectEntryType.DirectHit;
    public static bool IsDirectCrit(int flag) => (flag & 0xF000) == (int)EffectEntryType.DirectCritHit;

    public static bool IsHeal(int flag) => (flag & 0xF) == (int)EffectEntryType.Heal;
    public static bool IsCritHeal(int flag) => (flag & 0xF0000F) == (int)EffectEntryType.CritHeal;
    public static bool IsBlockedDamage(int flag) => (flag & 0xF) == (int)EffectEntryType.BlockedDamage;
    public static bool IsParriedDamage(int flag) => (flag & 0xF) == (int)EffectEntryType.ParriedDamage;
    public static bool IsInvulnerable(int flag) => (flag & 0xF) == (int)EffectEntryType.Invulnerable;
    public static bool IsApplyStatusEffectTarget(int flag) => (flag & 0xF) == (int)EffectEntryType.ApplyStatusEffectTarget;
    public static bool IsApplyStatusEffectSource(int flag) => (flag & 0xF) == (int)EffectEntryType.ApplyStatusEffectSource;
    public static bool IsRecoveredFromStatusEffect(int flag) => (flag & 0xF) == (int)EffectEntryType.RecoveredFromStatusEffect;
    public static bool IsLoseStatusEffectTarget(int flag) => (flag & 0xF) == (int)EffectEntryType.LoseStatusEffectTarget;
    public static bool IsLoseStatusEffectSource(int flag) => (flag & 0xF) == (int)EffectEntryType.LoseStatusEffectSource;
    public static bool IsFullResistStatus(int flag) => (flag & 0xF) == (int)EffectEntryType.FullResistStatus;
    public static bool IsInterrupt(int flag) => (flag & 0xF) == (int)EffectEntryType.Interrupt;
    public static bool IsSpecial(int flag)
    {
        if ((flag & 0xFFF) == (int)EffectEntryType.Unknown_3F)
        {
            return true;
        }
        return false;
    }


    public static List<EffectEntryType> ParseFlag(int flag)
    {
        List<EffectEntryType> parsedflag = new List<EffectEntryType>();

        if (IsNothing(flag))
        {
            parsedflag.Add(EffectEntryType.Nothing);
            return parsedflag;
        };
        if (IsMiss(flag))
        {
            parsedflag.Add(EffectEntryType.Miss);
        };
        if (IsFullResist(flag))
        {
            parsedflag.Add(EffectEntryType.FullResist);
        };
        // HITS AND STUFF
        if (IsDamage(flag))
        {
            if (IsCrit(flag))
            {
                parsedflag.Add(EffectEntryType.CritHit);
            }
            else if (IsDirect(flag))
            {
                parsedflag.Add(EffectEntryType.DirectHit);
            }
            else if (IsDirectCrit(flag))
            {
                parsedflag.Add(EffectEntryType.DirectCritHit);
            }
            parsedflag.Add(EffectEntryType.Damage);
        };
        // HEALS AND STUFF
        if (IsHeal(flag) && !IsCritHeal(flag))
        {
            if (IsCritHeal(flag))
            {
                parsedflag.Add(EffectEntryType.CritHeal);
            }
            else
            {
                parsedflag.Add(EffectEntryType.Heal);
            }
        };
        if (IsBlockedDamage(flag))
        {
            parsedflag.Add(EffectEntryType.BlockedDamage);
        };
        if (IsParriedDamage(flag))
        {
            parsedflag.Add(EffectEntryType.ParriedDamage);
        }
        if (IsInvulnerable(flag))
        {
            parsedflag.Add(EffectEntryType.Invulnerable);
        };
        // APLLY EFFECTS FROM TARGET
        if (IsApplyStatusEffectTarget(flag))
        {
            parsedflag.Add(EffectEntryType.ApplyStatusEffectTarget);
        };
        // APPLY EFFECTS FROM SOURCE
        if (IsApplyStatusEffectSource(flag))
        {
            parsedflag.Add(EffectEntryType.ApplyStatusEffectSource);
        };
        if (IsRecoveredFromStatusEffect(flag))
        {
            parsedflag.Add(EffectEntryType.RecoveredFromStatusEffect);
        };
        // CLEAR AFFLICTED STATUS FROM TARGET
        if (IsLoseStatusEffectTarget(flag))
        {
            parsedflag.Add(EffectEntryType.LoseStatusEffectTarget);
        };
        // CLEAR AFFLICTED STATUS FROM SOURCE
        if (IsLoseStatusEffectSource(flag))
        {
            parsedflag.Add(EffectEntryType.LoseStatusEffectSource);
        };
        // RESIST STATUS
        if (IsFullResistStatus(flag))
        {
            parsedflag.Add(EffectEntryType.FullResistStatus);
        };
        if (IsFullResistStatus(flag))
        {
            parsedflag.Add(EffectEntryType.Interrupt);
        };

        return parsedflag;
    }


    public static bool IsMagic(int flag) => (flag & 0xF0000) == (int)DamageType.Magic;
    public static bool IsPhysical(int flag) => (flag & 0xF0000) == (int)DamageType.Physical;
    public static bool IsRanged(int flag) => (flag & 0xF0000) == (int)DamageType.Ranged;
    public static bool IsMagicMelee(int flag) => (flag & 0xF0000) == (int)DamageType.MagicMelee;

    public static List<DamageType> ParseTypes(int flag)
    {
        List<DamageType> parsedTypes = new List<DamageType>();
        if (IsMagic(flag))
        {
            parsedTypes.Add(DamageType.Magic);
        }
        else if (IsPhysical(flag))
        {
            parsedTypes.Add(DamageType.Physical);
        }
        else if (IsRanged(flag))
        {
            parsedTypes.Add(DamageType.Ranged);
        }
        else if (IsMagicMelee(flag))
        {
            parsedTypes.Add(DamageType.MagicMelee);
        }
        return parsedTypes;
    }
}