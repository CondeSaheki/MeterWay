using System.Collections.Generic;

namespace MeterWay.LogParser;

public enum LogLineType : uint
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
    StatusApply = 26, // 0x0000001A
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
    None = 999, // used in LogsParser 
}

public static class ActionEffectFlag
{
    public enum Damage
    {
        Magic = 0x50000,
        Physical = 0x10000,
        MagicMelee = 0x30000,
        Ranged = 0x20000,
    }

    public enum EffectEntry
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

    private static int Masker(int data)
    {
        int mask = ((0xF000000 & data) != 0x0000000) ? 0xF000000 : 0x0000000;
        mask |= ((0x0F00000 & data) != 0x0000000) ? 0x0F00000 : 0x0000000;
        mask |= ((0x00F0000 & data) != 0x0000000) ? 0x00F0000 : 0x0000000;
        mask |= ((0x000F000 & data) != 0x0000000) ? 0x000F000 : 0x0000000;
        mask |= ((0x0000F00 & data) != 0x0000000) ? 0x0000F00 : 0x0000000;
        mask |= ((0x00000F0 & data) != 0x0000000) ? 0x00000F0 : 0x0000000;
        mask |= ((0x000000F & data) != 0x0000000) ? 0x000000F : 0x0000000;
        return mask;
    }

    private static bool Compare(int data, EffectEntry flag) => (data & Masker((int)flag)) == (int)flag;

    public static bool IsNothing(int data) => data == (int)EffectEntry.Nothing; // especial case
    public static bool IsMiss(int data) => Compare(data, EffectEntry.Miss);
    public static bool IsFullResist(int data) => Compare(data, EffectEntry.FullResist);
    public static bool IsDamage(int data) => Compare(data, EffectEntry.Damage);
    public static bool IsCrit(int data) => Compare(data, EffectEntry.CritHit);
    public static bool IsDirect(int data) => Compare(data, EffectEntry.DirectHit);
    public static bool IsDirectCrit(int data) => Compare(data, EffectEntry.DirectCritHit);

    public static bool IsHeal(int data) => Compare(data, EffectEntry.Heal);
    public static bool IsCritHeal(int data) => Compare(data, EffectEntry.CritHeal);
    public static bool IsBlockedDamage(int data) => Compare(data, EffectEntry.BlockedDamage);
    public static bool IsParriedDamage(int data) => Compare(data, EffectEntry.ParriedDamage);
    public static bool IsInvulnerable(int data) => Compare(data, EffectEntry.Invulnerable);
    public static bool IsApplyStatusEffectTarget(int data) => Compare(data, EffectEntry.ApplyStatusEffectTarget);
    public static bool IsApplyStatusEffectSource(int data) => Compare(data, EffectEntry.ApplyStatusEffectSource);
    public static bool IsRecoveredFromStatusEffect(int data) => Compare(data, EffectEntry.RecoveredFromStatusEffect);
    public static bool IsLoseStatusEffectTarget(int data) => Compare(data, EffectEntry.LoseStatusEffectTarget);
    public static bool IsLoseStatusEffectSource(int data) => Compare(data, EffectEntry.LoseStatusEffectSource);
    public static bool IsFullResistStatus(int data) => Compare(data, EffectEntry.FullResistStatus);
    public static bool IsInterrupt(int data) => Compare(data, EffectEntry.Interrupt);
    public static bool IsSpecial(int data)
    {
        if ((data & 0xFFF) == (int)EffectEntry.Unknown_3F)
        {
            return true;
        }
        return false;
    }

    public static List<EffectEntry> EffectEntryFlags(int data)
    {
        List<EffectEntry> flags = [];

        if (IsNothing(data))
        {
            flags.Add(EffectEntry.Nothing);
            return flags;
        }
        if (IsMiss(data)) flags.Add(EffectEntry.Miss);
        if (IsFullResist(data)) flags.Add(EffectEntry.FullResist);

        // HITS AND STUFF
        if (IsDamage(data))
        {
            if (IsCrit(data)) flags.Add(EffectEntry.CritHit);
            else if (IsDirect(data)) flags.Add(EffectEntry.DirectHit);
            else if (IsDirectCrit(data)) flags.Add(EffectEntry.DirectCritHit);
            flags.Add(EffectEntry.Damage);
        }

        // HEALS AND STUFF
        if (IsHeal(data) && !IsCritHeal(data))
        {
            if (IsCritHeal(data)) flags.Add(EffectEntry.CritHeal);
            else flags.Add(EffectEntry.Heal);
        }

        if (IsBlockedDamage(data)) flags.Add(EffectEntry.BlockedDamage);
        if (IsParriedDamage(data)) flags.Add(EffectEntry.ParriedDamage);
        if (IsInvulnerable(data)) flags.Add(EffectEntry.Invulnerable);

        // APLLY EFFECTS FROM TARGET
        if (IsApplyStatusEffectTarget(data)) flags.Add(EffectEntry.ApplyStatusEffectTarget);

        // APPLY EFFECTS FROM SOURCE
        if (IsApplyStatusEffectSource(data)) flags.Add(EffectEntry.ApplyStatusEffectSource);
        if (IsRecoveredFromStatusEffect(data)) flags.Add(EffectEntry.RecoveredFromStatusEffect);

        // CLEAR AFFLICTED STATUS FROM TARGET
        if (IsLoseStatusEffectTarget(data)) flags.Add(EffectEntry.LoseStatusEffectTarget);

        // CLEAR AFFLICTED STATUS FROM SOURCE
        if (IsLoseStatusEffectSource(data)) flags.Add(EffectEntry.LoseStatusEffectSource);

        // RESIST STATUS
        if (IsFullResistStatus(data)) flags.Add(EffectEntry.FullResistStatus);

        if (IsInterrupt(data)) flags.Add(EffectEntry.Interrupt);

        return flags;
    }

    private static bool Compare(int data, Damage flag) => (data & Masker((int)flag)) == (int)flag;

    public static bool IsMagic(int data) => Compare(data, Damage.Magic);
    public static bool IsPhysical(int data) => Compare(data, Damage.Physical);
    public static bool IsRanged(int data) => Compare(data, Damage.Ranged);
    public static bool IsMagicMelee(int data) => Compare(data, Damage.MagicMelee);

    public static List<Damage> DamageFlags(int data)
    {
        List<Damage> flags = [];

        if (IsMagic(data)) flags.Add(Damage.Magic);
        else if (IsPhysical(data)) flags.Add(Damage.Physical);
        else if (IsRanged(data)) flags.Add(Damage.Ranged);
        else if (IsMagicMelee(data)) flags.Add(Damage.MagicMelee);

        return flags;
    }
}