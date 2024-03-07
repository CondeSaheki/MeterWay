using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

using MeterWay.Utils;

namespace MeterWay.LogParser;

public class LogLine : Attribute { }

public class LogLineData
{
    public bool Parsed { get; private set; }
    public LogLine? Value { get; private set; }
    public Type? ValueType { get; init; }
    public LogLineType MsgType { get; init; }
    public DateTime TimePoint { get; init; }
    public string RawLine { get; init; }

    public LogLineData(string rawLine)
    {
        var data = Helpers.SplitStringAsMemory(rawLine, '|', 2);

        Parsed = false;

        MsgType = uint.TryParse(data[0].Span, out uint MsgTypevalue) ? (LogLineType)MsgTypevalue : LogLineType.None;
        TimePoint = DateTime.TryParse(data[1].Span.ToString(), out DateTime TimePointValue) ? TimePoint = TimePointValue : TimePoint = DateTime.Now;
        RawLine = rawLine;

        ValueType = MsgType switch
        {
            LogLineType.ActionEffect => typeof(ActionEffect),
            LogLineType.AOEActionEffect => typeof(ActionEffect),
            LogLineType.StartsCasting => typeof(StartsCasting),
            LogLineType.DoTHoT => typeof(DoTHoT),
            LogLineType.AddCombatant => typeof(AddCombatant),
            LogLineType.PlayerStats => typeof(PlayerStats),
            LogLineType.StatusApply => typeof(StatusApply),
            LogLineType.StatusRemove => typeof(StatusRemove),
            LogLineType.Death => typeof(Death),
            _ => null
        };
    }

    public void Parse()
    {
        if (!Parsed && ValueType != null)
        {
            try
            {
                Value = (LogLine?)Activator.CreateInstance(ValueType, Helpers.SplitStringAsMemory(RawLine, '|'));
                Parsed = true;
            }
            catch (Exception ex)
            {
                Dalamud.Log.Warning($"Failed to parse LogLine {MsgType} - {((uint)MsgType).ToString()} ->\n {RawLine} \n Error -> \n {ex}");
            }
        }
    }
}

public class ActionEffect : LogLine
{
    public uint SourceId { get; }
    public string SourceName { get; }
    public uint Id { get; }
    public string Name { get; }
    public uint? TargetId { get; }
    public string? TargetName { get; }
    public uint? TargetHp { get; }
    public uint? TargetMaxHp { get; }
    public List<KeyValuePair<uint, uint>> ActionAttributes { get; } // index 8 to 23
    public Vector4? TargetPos { get; }
    public uint? SourceMp { get; }
    public Vector4? Pos { get; }
    public uint MultiMessageIndex { get; }
    public uint MultiMessageCount { get; }

    public ActionEffect(List<ReadOnlyMemory<char>> data)
    {
        SourceId = uint.Parse(data[2].Span, NumberStyles.HexNumber);
        SourceName = data[3].Span.ToString();
        Id = uint.Parse(data[4].Span, NumberStyles.HexNumber);
        Name = data[5].Span.ToString();
        TargetId = data[6].IsEmpty ? null : uint.Parse(data[6].Span, NumberStyles.HexNumber);
        TargetName = data[7].IsEmpty ? null : data[7].Span.ToString();

        // ActionAttributes data[8], data[9], data[10], data[11], data[12], data[13], data[14], data[15], data[16], data[17], data[18], data[19], data[20], data[21], data[22], data[23]
        ActionAttributes = [];
        for (var i = 0; i != 8; ++i)
        {
            uint key = uint.Parse(data[8 + i].Span, NumberStyles.HexNumber);
            uint value = uint.Parse(data[9 + i].Span, NumberStyles.HexNumber);
            if (key == 0 && value == 0) break;
            ActionAttributes.Add(new KeyValuePair<uint, uint>(key, value));
        }

        TargetHp = data[24].IsEmpty ? null : uint.Parse(data[24].Span);
        TargetMaxHp = data[25].IsEmpty ? null : uint.Parse(data[25].Span);
        // uint? targetMp = data[26].ToString() == "" ? null : uint.Parse(data[26].ToString());
        // uint? targetMaxMp = data[27].ToString() == "" ? null : uint.Parse(data[27].ToString());
        // separator = data[28]; // null
        // separator = data[29]; // null
        TargetPos = Helpers.Vec4Parse(data, 30, 31, 32, 33);
        // hp = uint.Parse(data[34].ToString());
        // maxHp = uint.Parse(data[35].ToString());
        SourceMp = data[36].IsEmpty ? null : uint.Parse(data[36].Span);
        // maxMp = uint.Parse(data[37].ToString());
        // separator = data[38]; // null
        // separator = data[39]; // null
        Pos = Helpers.Vec4Parse(data, 40, 41, 42, 43);
        // loglinescount = uint.Parse(data[44].ToString(), 16);
        MultiMessageIndex = uint.Parse(data[45].Span);
        MultiMessageCount = uint.Parse(data[46].Span);
        // criptoid = data[47]
    }
}

public class PlayerStats : LogLine
{
    public uint JobID { get; }
    public uint Str { get; }
    public uint Dex { get; }
    public uint Vit { get; }
    public uint Intel { get; }
    public uint Mind { get; }
    public uint Piety { get; }
    public uint Attack { get; }
    public uint DirectHit { get; }
    public uint Crit { get; }
    public uint AttackMagicPotency { get; }
    public uint HealMagicPotency { get; }
    public uint Det { get; }
    public uint SkillSpeed { get; }
    public uint SpellSpeed { get; }
    public uint Tenacity { get; }
    public ulong LocalContentId { get; }

    public PlayerStats(List<ReadOnlyMemory<char>> data)
    {
        JobID = uint.Parse(data[2].Span);
        Str = uint.Parse(data[3].Span);
        Dex = uint.Parse(data[4].Span);
        Vit = uint.Parse(data[5].Span);
        Intel = uint.Parse(data[6].Span);
        Mind = uint.Parse(data[7].Span);
        Piety = uint.Parse(data[8].Span);
        Attack = uint.Parse(data[9].Span);
        DirectHit = uint.Parse(data[10].Span);
        Crit = uint.Parse(data[11].Span);
        AttackMagicPotency = uint.Parse(data[12].Span);
        HealMagicPotency = uint.Parse(data[13].Span);
        Det = uint.Parse(data[14].Span);
        SkillSpeed = uint.Parse(data[15].Span);
        SpellSpeed = uint.Parse(data[16].Span);
        Tenacity = uint.Parse(data[17].Span);
        LocalContentId = UInt64.Parse(data[18].Span);
        // criptoid = data[19]
    }
}

public class StartsCasting : LogLine
{
    public uint SourceId { get; }
    public string SourceName { get; }
    public uint ActionId { get; }
    public string ActionName { get; }
    public uint? TargetId { get; }
    public string? TargetName { get; }
    public float CastTime { get; }
    public Vector4? TargetPos { get; }

    public StartsCasting(List<ReadOnlyMemory<char>> data)
    {
        SourceId = uint.Parse(data[2].Span, NumberStyles.HexNumber);
        SourceName = data[3].Span.ToString();
        ActionId = uint.Parse(data[4].Span, NumberStyles.HexNumber);
        ActionName = data[5].Span.ToString();
        TargetId = uint.Parse(data[6].Span, NumberStyles.HexNumber);
        TargetName = data[7].Span.ToString();
        CastTime = float.Parse(data[8].Span);
        TargetPos = Helpers.Vec4Parse(data, 9, 10, 11, 12);
        // criptoid = data[13]
    }
}

public class StatusApply : LogLine
{
    public uint Id { get; }
    public string Name { get; }
    public float Duration { get; }
    public uint SourceId { get; }
    public string SourceName { get; }
    public uint TargetID { get; }
    public string TargetName { get; }
    public uint Unknown { get; }
    public uint? TargetMaxHP { get; }
    public uint? SourceMaxHP { get; }

    public StatusApply(List<ReadOnlyMemory<char>> data)
    {
        Id = uint.Parse(data[2].Span, NumberStyles.HexNumber);
        Name = data[3].Span.ToString();
        Duration = float.Parse(data[4].Span);
        SourceId = uint.Parse(data[5].Span, NumberStyles.HexNumber);
        SourceName = data[6].Span.ToString();
        TargetID = uint.Parse(data[7].Span, NumberStyles.HexNumber);
        TargetName = data[8].Span.ToString();
        Unknown = uint.Parse(data[9].Span);
        TargetMaxHP = uint.Parse(data[10].Span);
        SourceMaxHP = uint.Parse(data[11].Span);
    }
}

public class StatusRemove : LogLine
{
    public uint Id { get; }
    public string Name { get; }
    public float Duration { get; }
    public uint SourceId { get; }
    public string SourceName { get; }
    public uint TargetID { get; }
    public string TargetName { get; }
    public uint Unknown { get; }
    public uint? TargetMaxHP { get; }
    public uint? SourceMaxHP { get; }

    public StatusRemove(List<ReadOnlyMemory<char>> data)
    {
        Id = uint.Parse(data[2].Span, NumberStyles.HexNumber);
        Name = data[3].Span.ToString();
        Duration = float.Parse(data[4].Span); // seems to always be 0.00
        SourceId = uint.Parse(data[5].Span, NumberStyles.HexNumber);
        SourceName = data[6].Span.ToString();
        TargetID = uint.Parse(data[7].Span, NumberStyles.HexNumber);
        TargetName = data[8].Span.ToString();
        Unknown = uint.Parse(data[9].Span);
        TargetMaxHP = uint.Parse(data[10].Span);
        SourceMaxHP = uint.Parse(data[11].Span);
    }
}

public class Death : LogLine
{
    public uint targetId { get; }
    public string targetName { get; }
    public uint sourceId { get; }
    public string? sourceName { get; }

    public Death(List<ReadOnlyMemory<char>> data)
    {
        targetId = uint.Parse(data[2].Span, NumberStyles.HexNumber);
        targetName = data[3].Span.ToString();
        // if source is nothing id is E0000000
        sourceId = uint.Parse(data[4].Span, NumberStyles.HexNumber);
        sourceName = data[4].IsEmpty ? null : data[4].Span.ToString();
    }
}

public class DoTHoT : LogLine
{
    public uint TargetId { get; }
    public string TargetName { get; }
    public bool IsHeal { get; }
    public uint BuffId { get; }
    public uint Value { get; }
    public uint? TargetHp { get; }
    public uint? TargetMaxHp { get; }
    public uint? TargetMp { get; }
    public uint? TargetMaxMp { get; }
    public Vector4? TargetPos { get; }
    public uint SourceId { get; }
    public string SourceName { get; }
    public uint DamageType { get; }
    public uint? SourceHp { get; }
    public uint? SourceMaxHp { get; }
    public uint? SourceMp { get; }
    public uint? SourceMaxMp { get; }
    public Vector4? SourcePos { get; }

    public DoTHoT(List<ReadOnlyMemory<char>> data)
    {
        TargetId = uint.Parse(data[2].Span, NumberStyles.HexNumber);
        TargetName = data[3].Span.ToString();
        IsHeal = data[4].Span.ToString() == "HoT";
        // data[5] == 0 if from source; BuffId
        Value = uint.Parse(data[6].Span, NumberStyles.HexNumber);
        SourceHp = uint.Parse(data[7].Span);
        SourceMaxHp = uint.Parse(data[8].Span);
        SourceMp = uint.Parse(data[9].Span);
        SourceMaxMp = uint.Parse(data[10].Span);
        // data[11] == null
        // data[12] == null
        SourcePos = Helpers.Vec4Parse(data, 13, 14, 15, 16);
        SourceId = uint.Parse(data[17].Span, NumberStyles.HexNumber);
        SourceName = data[18].Span.ToString();
        // DamageType = uint.Parse(data[19]); // seems to be meaningless
        TargetHp = data[20].IsEmpty ? 0 : uint.Parse(data[20].Span);
        TargetMaxHp = data[21].IsEmpty ? 0 : uint.Parse(data[21].Span);
        TargetMp = data[22].IsEmpty ? 0 : uint.Parse(data[22].Span);
        TargetMaxMp = data[23].IsEmpty ? 0 : uint.Parse(data[23].Span);
        // data[24] = null
        // data[25] = null
        TargetPos = Helpers.Vec4Parse(data, 26, 27, 28, 29);
        // data[30] = criptoid
    }
}

public class AddCombatant : LogLine
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public string Job { get; set; }
    public uint Level { get; set; }
    public uint OwnerId { get; set; }
    public uint WorldId { get; set; }
    public string World { get; set; }
    public uint NpcNameId { get; set; }
    public uint NpcBaseId { get; set; }
    public uint CurrentHp { get; set; }
    public uint Hp { get; set; }
    public uint CurrentMp { get; set; }
    public uint Mp { get; set; }
    public string Unknown1 { get; set; }
    public string Unknown2 { get; set; }
    public Vector4? Pos { get; set; }

    public bool IsPet { get; }

    public AddCombatant(List<ReadOnlyMemory<char>> data)
    {
        Id = uint.Parse(data[2].Span, NumberStyles.HexNumber);
        Name = data[3].Span.ToString();
        Job = data[4].Span.ToString();
        Level = uint.Parse(data[5].Span, NumberStyles.HexNumber);
        OwnerId = uint.Parse(data[6].Span, NumberStyles.HexNumber);
        WorldId = uint.Parse(data[7].Span, NumberStyles.HexNumber);
        World = data[8].Span.ToString();
        NpcNameId = uint.Parse(data[9].Span, NumberStyles.HexNumber);
        NpcBaseId = uint.Parse(data[10].Span, NumberStyles.HexNumber);
        CurrentHp = uint.Parse(data[11].Span, NumberStyles.HexNumber);
        Hp = uint.Parse(data[12].Span, NumberStyles.HexNumber);
        CurrentMp = uint.Parse(data[13].Span, NumberStyles.HexNumber);
        Mp = uint.Parse(data[14].Span, NumberStyles.HexNumber);
        Unknown1 = data[15].Span.ToString();
        Unknown2 = data[16].Span.ToString();
        Pos = Helpers.Vec4Parse(data, 17, 18, 19, 20);
        IsPet = OwnerId != 0 && ((Id >> 24) & 0xFF) == 64;
    }
}