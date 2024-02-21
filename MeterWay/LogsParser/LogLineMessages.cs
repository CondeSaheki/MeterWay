using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MeterWay.LogParser;

public class LogLine : Attribute { }

public class LogLineData
{
    public bool Parsed { get; set; }
    public LogLine? Value { get; set; }

    public readonly LogLineType MsgType;
    public readonly DateTime DateTime;
    public readonly string RawLine;

    public Func<List<string>, LogLine>? LogLineTypefn { get; set; }

    public LogLineData(LogLineType msgType, DateTime dateTime, string raw)
    {
        this.Parsed = false;
        this.MsgType = msgType;
        this.DateTime = dateTime;
        this.RawLine = raw;

        this.LogLineTypefn = null;
    }

    public void Parse()
    {
        if (!Parsed && LogLineTypefn != null)
        {
            var data = RawLine.Split('|').ToList();
            this.Value = LogLineTypefn(data);
            this.Parsed = true;
            // return true;
        }
        // return false;
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
    public int? TargetHp { get; }
    public uint? TargetMaxHp { get; }
    public List<KeyValuePair<uint, uint>> ActionAttributes { get; } // index 8 to 23
    public Vector4? TargetPos { get; }
    public uint? SourceMp { get; }
    public Vector4? Pos { get; }
    public uint MultiMessageIndex { get; }
    public uint MultiMessageCount { get; }

    public ActionEffect(List<string> data)
    {
        this.SourceId = Convert.ToUInt32(data[2], 16);
        this.SourceName = data[3].ToString();
        this.Id = Convert.ToUInt32(data[4], 16);
        this.Name = data[5];
        this.TargetId = data[6] == "" ? null : Convert.ToUInt32(data[6], 16);
        this.TargetName = data[7] == "" ? null : data[7];

        this.ActionAttributes = new List<KeyValuePair<uint, uint>>();
        for (var i = 0; i != 8; ++i)
        {
            uint key = Convert.ToUInt32(data[8 + i], 16);
            uint value = Convert.ToUInt32(data[9 + i], 16);
            if (key == 0 && value == 0) break;
            this.ActionAttributes.Add(new KeyValuePair<uint, uint>(key, value));
        }

        // skillatributes = data[9];
        // skillatributes = data[8];

        // skillatributes = data[11];
        // skillatributes = data[10];

        // skillatributes = data[13];
        // skillatributes = data[12];

        // skillatributes = data[15];
        // skillatributes = data[14];

        // skillatributes = data[17];
        // skillatributes = data[16];

        // skillatributes = data[19];
        // skillatributes = data[18];

        // skillatributes = data[21];
        // skillatributes = data[20];

        // skillatributes = data[23];
        // skillatributes = data[22];

        this.TargetHp = data[24] == "" ? null : Convert.ToInt32(data[24]);
        this.TargetMaxHp = data[25] == "" ? null : Convert.ToUInt32(data[25]);
        // int? targetMp = data[26].ToString() == "" ? null : Convert.ToInt32(data[26].ToString());
        // int? targetMaxMp = data[27].ToString() == "" ? null : Convert.ToInt32(data[27].ToString());

        // separator = data[28]; // null
        // separator = data[29]; // null

        if (data[33] != "" || data[32] != "" || data[31] != "" || data[30] != "")
        {
            this.TargetPos = new Vector4(float.Parse(data[30]), float.Parse(data[31]),
                float.Parse(data[32]), float.Parse(data[33]));
        }
        // else null

        this.SourceMp = data[36] == "" ? null : Convert.ToUInt32(data[36]);
        // maxHp = Convert.ToUInt32(data[35].ToString());
        // hp = Convert.ToUInt32(data[34].ToString());
        // maxMp = Convert.ToUInt32(data[37].ToString());

        // separator = data[38]; // null
        // separator = data[39]; // null

        if (data[40] != "" || data[41] != "" || data[42] != "" || data[43] != "")
        {
            this.Pos = new Vector4(float.Parse(data[40]), float.Parse(data[41]),
            float.Parse(data[42]), float.Parse(data[43]));
        }

        // loglinescount = Convert.ToUInt32(data[44].ToString(), 16);
        this.MultiMessageIndex = Convert.ToUInt32(data[45]);
        this.MultiMessageCount = Convert.ToUInt32(data[46]);
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

    public PlayerStats(List<string> data)
    {
        this.JobID = Convert.ToUInt32(data[2]);
        this.Str = Convert.ToUInt32(data[3]);
        this.Dex = Convert.ToUInt32(data[4]);
        this.Vit = Convert.ToUInt32(data[5]);
        this.Intel = Convert.ToUInt32(data[6]);
        this.Mind = Convert.ToUInt32(data[7]);
        this.Piety = Convert.ToUInt32(data[8]);
        this.Attack = Convert.ToUInt32(data[9]);
        this.DirectHit = Convert.ToUInt32(data[10]);
        this.Crit = Convert.ToUInt32(data[11]);
        this.AttackMagicPotency = Convert.ToUInt32(data[12]);
        this.HealMagicPotency = Convert.ToUInt32(data[13]);
        this.Det = Convert.ToUInt32(data[14]);
        this.SkillSpeed = Convert.ToUInt32(data[15]);
        this.SpellSpeed = Convert.ToUInt32(data[16]);
        this.Tenacity = Convert.ToUInt32(data[17]);
        this.LocalContentId = Convert.ToUInt64(data[18]);
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

    public StartsCasting(List<string> data)
    {
        this.SourceId = Convert.ToUInt32(data[2], 16);
        this.SourceName = data[3];
        this.ActionId = Convert.ToUInt32(data[4], 16);
        this.ActionName = data[5];
        this.TargetId = Convert.ToUInt32(data[6], 16);
        this.TargetName = data[7];
        this.CastTime = float.Parse(data[8]);
        if (data[9] != "" || data[10] != "" || data[11] != "" || data[12] != "")
        {
            this.TargetPos = new Vector4(float.Parse(data[9]), float.Parse(data[10]),
                float.Parse(data[11]), float.Parse(data[12]));
        }
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
    public int? TargetMaxHP { get; }
    public int? SourceMaxHP { get; }

    public StatusApply(List<string> data)
    {
        this.Id = Convert.ToUInt32(data[2], 16);
        this.Name = data[3];
        this.Duration = float.Parse(data[4]);
        this.SourceId = Convert.ToUInt32(data[5], 16);
        this.SourceName = data[6];
        this.TargetID = Convert.ToUInt32(data[7], 16);
        this.TargetName = data[8];
        this.Unknown = Convert.ToUInt32(data[9]);
        this.TargetMaxHP = Convert.ToInt32(data[10]);
        this.SourceMaxHP = Convert.ToInt32(data[11]);
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
    public int? TargetMaxHP { get; }
    public int? SourceMaxHP { get; }

    public StatusRemove(List<string> data)
    {
        this.Id = Convert.ToUInt32(data[2], 16);
        this.Name = data[3];
        this.Duration = float.Parse(data[4]); // seems to always be 0.00
        this.SourceId = Convert.ToUInt32(data[5], 16);
        this.SourceName = data[6];
        this.TargetID = Convert.ToUInt32(data[7], 16);
        this.TargetName = data[8];
        this.Unknown = Convert.ToUInt32(data[9]);
        this.TargetMaxHP = Convert.ToInt32(data[10]);
        this.SourceMaxHP = Convert.ToInt32(data[11]);
    }
}

public class Death : LogLine
{
    public uint targetId { get; }
    public string targetName { get; }
    public uint sourceId { get; }
    public string? sourceName { get; }

    public Death(List<string> data)
    {
        this.targetId = Convert.ToUInt32(data[2], 16);
        this.targetName = data[3];
        // if source is nothing id is E0000000
        this.sourceId = Convert.ToUInt32(data[4], 16);
        this.sourceName = data[4] == "" ? null : data[4];
    }
}

public class DoTHoT : LogLine
{
    public uint TargetId { get; }
    public string TargetName { get; }
    public bool IsHeal { get; }
    public uint BuffId { get; }
    public uint Value { get; }
    public int? TargetHp { get; }
    public int? TargetMaxHp { get; }
    public int? TargetMp { get; }
    public int? TargetMaxMp { get; }
    public Vector4? TargetPos { get; }
    public uint SourceId { get; }
    public string SourceName { get; }
    public int DamageType { get; }
    public int? SourceHp { get; }
    public int? SourceMaxHp { get; }
    public int? SourceMp { get; }
    public int? SourceMaxMp { get; }
    public Vector4? SourcePos { get; }

    public DoTHoT(List<string> data)
    {
        // 24|2024-02-19T05:36:56.1640000-03:00|40003EC7|Striking Dummy|DoT|0|466|44|44|0|10000|||-727.13|-810.75|10.02|-0.96|1089ED18|Aruna Rhen|FFFFFFFF|31362|31362|9478|10000|||-723.48|-821.16|10.00|-0.34|2df8dd482da88ed7
        // PluginManager.Instance.PluginLog.Info(raw);
        this.TargetId = Convert.ToUInt32(data[2], 16);
        this.TargetName = data[3];
        this.IsHeal = data[4] == "HoT";
        // data[5] == 0 if from source; BuffId
        this.Value = Convert.ToUInt32(data[6], 16);
        this.SourceHp = Convert.ToInt32(data[7]);
        this.SourceMaxHp = Convert.ToInt32(data[8]);
        this.SourceMp = Convert.ToInt32(data[9]);
        this.SourceMaxMp = Convert.ToInt32(data[10]);
        // data[11] == null
        // data[12] == null
        if (data[13] != "" || data[14] != "" || data[15] != "" || data[16] != "")
        {
            this.SourcePos = new Vector4(float.Parse(data[13]), float.Parse(data[14]),
                float.Parse(data[15]), float.Parse(data[16]));
        }

        // else null


        this.SourceId = Convert.ToUInt32(data[17].ToString(), 16);
        this.SourceName = data[18];

        // this.DamageType = Convert.ToUInt32(data[19]); // seems to be meaningless

        this.TargetHp = data[20] == "" ? 0 : Convert.ToInt32(data[20]);
        this.TargetMaxHp = data[21] == "" ? 0 : Convert.ToInt32(data[21]);
        this.TargetMp = data[22] == "" ? 0 : Convert.ToInt32(data[22]);
        this.TargetMaxMp = data[23] == "" ? 0 : Convert.ToInt32(data[23]);
        // data[24] = null
        // data[25] = null
        if (data[26] != "" || data[27] != "" || data[28] != "" || data[29] != "")
        {
            this.TargetPos = new Vector4(float.Parse(data[26]), float.Parse(data[27]),
                float.Parse(data[28]), float.Parse(data[29]));
        }
        // data[30] = criptoid

    }
}

public class AddCombatant : LogLine
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public string Job { get; set; }
    public int Level { get; set; }
    public uint OwnerId { get; set; }
    public uint WorldId { get; set; }
    public string World { get; set; }
    public uint NpcNameId { get; set; }
    public uint NpcBaseId { get; set; }
    public int CurrentHp { get; set; }
    public int Hp { get; set; }
    public int CurrentMp { get; set; }
    public int Mp { get; set; }
    public string Unknown1 { get; set; }
    public string Unknown2 { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Heading { get; set; }

    public bool IsPet { get; }

    public AddCombatant(List<string> data)
    {
        this.Id = Convert.ToUInt32(data[2], 16);
        this.Name = data[3];
        this.Job = data[4];
        this.Level = Convert.ToInt32(data[5], 16);
        this.OwnerId = Convert.ToUInt32(data[6], 16);
        this.WorldId = Convert.ToUInt32(data[7], 16);
        this.World = data[8];
        this.NpcNameId = Convert.ToUInt32(data[9], 16);
        this.NpcBaseId = Convert.ToUInt32(data[10], 16);
        this.CurrentHp = Convert.ToInt32(data[11], 16);
        this.Hp = Convert.ToInt32(data[12], 16);
        this.CurrentMp = Convert.ToInt32(data[13], 16);
        this.Mp = Convert.ToInt32(data[14], 16);
        this.Unknown1 = data[15];
        this.Unknown2 = data[16];
        this.X = Convert.ToDouble(data[17]);
        this.Y = Convert.ToDouble(data[18]);
        this.Z = Convert.ToDouble(data[19]);
        this.Heading = Convert.ToDouble(data[20]);

        this.IsPet = this.OwnerId != 0 && ((this.Id >> 24) & 0xFF) == 64;
    }
}