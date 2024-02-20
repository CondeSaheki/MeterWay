using System;
using System.Collections.Generic;
using System.Numerics;

namespace MeterWay.LogParser;

public interface INetworkMessage
{
    public uint MsgType { get; }
    public DateTime DateTime { get; }
    public string RawLine { get; }
}

public class ActionEffect : INetworkMessage
{
    public uint MsgType { get; }
    public DateTime DateTime { get; }
    public uint ObjectId { get; }
    public int Id { get; }
    public string Name { get; }
    public uint? TargetId { get; }
    public string? TargetName { get; }
    public int? TargetHp { get; }
    public uint? TargetMaxHp { get; }
    public List<KeyValuePair<uint, uint>> ActionAttributes { get; } // index 8 to 23
    public Vector4? TargetPos { get; }
    public uint Mp { get; }
    public Vector4 Pos { get; }
    public uint MultiMessageIndex { get; }
    public uint MultiMessageCount { get; }
    public string RawLine { get; }


    public ActionEffect(List<string> data, string raw)
    {
        this.RawLine = raw;

        // int crypto = Convert.ToInt32(data[47].ToString(), 16);
        this.MultiMessageCount = Convert.ToUInt32(data[46].ToString());
        this.MultiMessageIndex = Convert.ToUInt32(data[45].ToString());
        // uint loglinescount = Convert.ToUInt32(data[44].ToString(), 16);

        this.Pos = new Vector4((float)Convert.ToDouble(data[40].ToString()), (float)Convert.ToDouble(data[41].ToString()),
            (float)Convert.ToDouble(data[42].ToString()), (float)Convert.ToDouble(data[43].ToString()));

        // var separator = data[39]; // null
        // var separator = data[38]; // null
        // uint maxMp = Convert.ToUInt32(data[37].ToString());
        this.Mp = Convert.ToUInt32(data[36].ToString());
        // uint maxHp = Convert.ToUInt32(data[35].ToString());
        // uint hp = Convert.ToUInt32(data[34].ToString());

        if (data[33].ToString() != "" || data[32].ToString() != "" || data[31].ToString() != "" || data[30].ToString() != "")
        {
            this.TargetPos = new Vector4((float)Convert.ToDouble(data[30].ToString()), (float)Convert.ToDouble(data[31].ToString()),
                (float)Convert.ToDouble(data[32].ToString()), (float)Convert.ToDouble(data[33].ToString()));
        }
        // else null

        // var separator = data[29]; // null
        // var separator = data[28]; // null
        // int? targetMaxMp = data[27].ToString() == "" ? null : Convert.ToInt32(data[27].ToString());
        // int? targetMp = data[26].ToString() == "" ? null : Convert.ToInt32(data[26].ToString());
        this.TargetMaxHp = data[25].ToString() == "" ? null : Convert.ToUInt32(data[25].ToString());
        this.TargetHp = data[24].ToString() == "" ? null : Convert.ToInt32(data[24].ToString());


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

        // var skillatributes = data[9];
        // var skillatributes = data[8];


        this.ActionAttributes = new List<KeyValuePair<uint, uint>>();
        for (var i = 0; i != 8; ++i)
        {
            uint key = Convert.ToUInt32(data[8 + i].ToString(), 16);
            uint value = Convert.ToUInt32(data[9 + i].ToString(), 16);
            if (key == 0 && value == 0) break;
            this.ActionAttributes.Add(new KeyValuePair<uint, uint>(key, value));
        }



        // UInt32? actionValue = null;
        // if (data[9].ToString() != "")
        // {
        //     int _tempValueHex = Convert.ToInt32(data[9].ToString(), 16);
        //     actionValue = (UInt32)((UInt32)(_tempValueHex >> 16) | (UInt32)((_tempValueHex << 16)) & 0x0FFFFFFF);
        // }

        // int actionTraits = Convert.ToInt32(data[8].ToString(), 16);

        this.TargetName = data[7].ToString() == "" ? null : data[7].ToString();
        this.TargetId = data[6].ToString() == "" ? null : Convert.ToUInt32(data[6].ToString(), 16);
        this.Name = data[5].ToString();
        this.Id = Convert.ToInt32(data[4].ToString(), 16);
        // this.PlayerName = data[3].ToString();
        this.ObjectId = Convert.ToUInt32(data[2].ToString(), 16);

        this.DateTime = DateTime.Parse(data[1].ToString());

        this.RawLine = raw;
    }
}

public class PlayerStatsUpdate : INetworkMessage
{
    public uint MsgType { get; }
    public DateTime DateTime { get; }
    public string RawLine { get; }

    public uint JobID { get; }
    public uint Str { get ; }
    public uint Dex { get ; }
    public uint Vit { get ; }
    public uint Intel { get ; }
    public uint Mind { get ; }
    public uint Piety { get ; }
    public uint Attack { get ; }
    public uint DirectHit { get ; }
    public uint Crit { get ; }
    public uint AttackMagicPotency { get ; }
    public uint HealMagicPotency { get ; }
    public uint Det { get ; }
    public uint SkillSpeed { get ; }
    public uint SpellSpeed { get ; }
    public uint Tenacity { get ; }
    public ulong LocalContentId { get ; }

    public PlayerStatsUpdate(List<string> data, string raw)
    {
        this.MsgType = 12;
        this.RawLine = raw;
        this.DateTime = DateTime.Parse(data[1].ToString());

        // TODO        
    }
}

public class StartsCasting : INetworkMessage
{
    public uint MsgType { get; }
    public DateTime DateTime { get; }
    public string RawLine { get; }

    uint sourceId { get; }
    string sourceName { get; }
    uint skillId { get; }
    string skillName { get; }
    uint? targetId { get; }
    string? targetName { get; }
    float Duration { get; }
    float? posX { get; }
    float? posY { get; }
    float? posZ { get; }
    float? heading { get; }

    public StartsCasting(List<string> data, string raw)
    {
        this.MsgType = 20;
        this.RawLine = raw;
        this.DateTime = DateTime.Parse(data[1].ToString());

        // TODO
    }
}

public class DoTHoT : INetworkMessage
{
    public uint MsgType { get; }
    public DateTime DateTime { get; }
    public string RawLine { get; }

    public int TargetId { get; }
    public string TargetName { get; }
    public bool IsHeal { get; }
    public int BuffId { get; }
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
    public Vector4 SourcePos { get; }

    public DoTHoT(List<string> data, string raw)
    {
        // 24|2024-02-19T05:36:56.1640000-03:00|40003EC7|Striking Dummy|DoT|0|466|44|44|0|10000|||-727.13|-810.75|10.02|-0.96|1089ED18|Aruna Rhen|FFFFFFFF|31362|31362|9478|10000|||-723.48|-821.16|10.00|-0.34|2df8dd482da88ed7
        // PluginManager.Instance.PluginLog.Info(raw);
        this.MsgType = 24;
        this.RawLine = raw;
        this.DateTime = DateTime.Parse(data[1].ToString());

        this.TargetId = Convert.ToInt32(data[2].ToString(), 16);
        this.TargetName = data[3];
        this.IsHeal = data[4].ToString() == "HoT";
        // data[5] == 0 if from source;
        this.Value = Convert.ToUInt32(data[6].ToString(), 16);
        this.SourceHp = Convert.ToInt32(data[7]);
        this.SourceMaxHp = Convert.ToInt32(data[8]);
        this.SourceMp = Convert.ToInt32(data[9]);
        this.SourceMaxMp = Convert.ToInt32(data[10]);
        // data[11] == null
        // data[12] == null
        this.SourcePos = new Vector4((float)Convert.ToDouble(data[13].ToString()), (float)Convert.ToDouble(data[14].ToString()),
            (float)Convert.ToDouble(data[15].ToString()), (float)Convert.ToDouble(data[16].ToString()));
        // else null


        this.SourceId = Convert.ToUInt32(data[17].ToString(), 16);
        this.SourceName = data[18].ToString();

        // this.DamageType = Convert.ToUInt32(data[19]); // seems to be meaningless

        this.TargetHp = data[20].ToString() == "" ? 0 : Convert.ToInt32(data[20].ToString());
        this.TargetMaxHp = data[21].ToString() == "" ? 0 : Convert.ToInt32(data[21].ToString());
        this.TargetMp = data[22].ToString() == "" ? 0 : Convert.ToInt32(data[22].ToString());
        this.TargetMaxMp = data[23].ToString() == "" ? 0 : Convert.ToInt32(data[23].ToString());
        // data[24] = null
        // data[25] = null
        if (data[26].ToString() != "" || data[27].ToString() != "" || data[28].ToString() != "" || data[29].ToString() != "")
        {
            this.TargetPos = new Vector4((float)Convert.ToDouble(data[26].ToString()), (float)Convert.ToDouble(data[27].ToString()),
                (float)Convert.ToDouble(data[28].ToString()), (float)Convert.ToDouble(data[29].ToString()));
        }
        // data[30] = criptoid

    }
}
public class AddCombatant : INetworkMessage
{
    public uint MsgType { get; }
    public DateTime DateTime { get; set; }
    public string RawLine { get; }
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

    public AddCombatant(List<string> data, string raw)
    {
        this.RawLine = raw;

        this.MsgType = Convert.ToUInt32(data[0], 16);
        this.DateTime = DateTime.Parse(data[1]);
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


