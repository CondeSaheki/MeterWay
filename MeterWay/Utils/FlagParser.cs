using System.Collections.Generic;
using MeterWay.managers;

public static class FlagParser
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
            else if(IsDirectCrit(flag))
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
            } else
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