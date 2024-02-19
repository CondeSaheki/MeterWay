using System;
using System.Collections.Generic;
using System.Numerics;

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
    // skip player id and name
    public uint ObjectId { get; }
    public int Id { get; }
    public string Name { get; }
    public uint? TargetId { get; }
    public string? TargetName { get; }
    public int? TargetHp { get; }
    public uint? TargetMaxHp { get; }
    public List<KeyValuePair<uint, uint>> ActionAttributes { get; } // index 8 to 23
    public Vector4 TargetPos { get; }
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

        this.TargetPos = new Vector4();
        if (data[33].ToString() != "" || data[32].ToString() != "" || data[31].ToString() != "" || data[30].ToString() != "")
        {
            this.TargetPos = new Vector4((float)Convert.ToDouble(data[30].ToString()), (float)Convert.ToDouble(data[31].ToString()),
                (float)Convert.ToDouble(data[32].ToString()), (float)Convert.ToDouble(data[33].ToString()));
        }

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

    public class StartsCasting : INetworkMessage
    {
        public uint MsgType { get; }
        public DateTime DateTime { get; }
        public string RawLine { get; }

        uint sourceId;
        string sourceName;
        uint targetId;
        string targetName;
        uint skillId;
        string skillName;
        float Duration;
        float? posX;
        float? posY;
        float? posZ;
        float? heading;

        public StartsCasting(List<string> data, string raw)
        {

        }
    }

    public class MsgDoTHoT : INetworkMessage
    {
        public uint MsgType { get; }
        public DateTime DateTime { get; }
        public string RawLine { get; }

        uint targetId;
        string targetName;
        bool IsHeal;
        uint buffId;
        uint amount;
        uint? targetCurrentHp;
        uint? targetMaxHp;
        uint? targetCurrentMp;
        uint? targetMaxMp;
        float? targetPosX;
        float? targetPosY;
        float? targetPosZ;
        float? targetHeading;
        uint? sourceId;
        string sourceName;
        uint damageType;
        uint? sourceCurrentHp;
        uint? sourceMaxHp;
        uint? sourceCurrentMp;
        uint? sourceMaxMp;
        float? sourcePosX;
        float? sourcePosY;
        float? sourcePosZ;
        float? sourceHeading;

        public MsgDoTHoT(List<string> data, string raw)
        {
            this.MsgType = 24;
            this.RawLine = raw;
            this.DateTime = DateTime.Now;

            this.targetId = Convert.ToUInt32(data[0]);
            this.targetName = data[0];
            this.IsHeal = Convert.ToBoolean(data[0]);
            this.buffId = Convert.ToUInt32(data[0]);
            this.amount = Convert.ToUInt32(data[0]);
            this.targetCurrentHp = Convert.ToUInt32(data[0]);
            this.targetMaxHp = Convert.ToUInt32(data[0]);
            this.targetCurrentMp = Convert.ToUInt32(data[0]);
            this.targetMaxMp = Convert.ToUInt32(data[0]);
            this.targetPosX = Convert.ToUInt32(data[0]);
            this.targetPosY = Convert.ToUInt32(data[0]);
            this.targetPosZ = Convert.ToUInt32(data[0]);
            this.targetHeading = Convert.ToUInt32(data[0]);
            this.sourceId = Convert.ToUInt32(data[0]);
            this.sourceName = data[0];
            this.damageType = Convert.ToUInt32(data[0]);
            this.sourceCurrentHp = Convert.ToUInt32(data[0]);
            this.sourceMaxHp = Convert.ToUInt32(data[0]);
            this.sourceCurrentMp = Convert.ToUInt32(data[0]);
            this.sourceMaxMp = Convert.ToUInt32(data[0]);
            this.sourcePosX = Convert.ToUInt32(data[0]);
            this.sourcePosY = Convert.ToUInt32(data[0]);
            this.sourcePosZ = Convert.ToUInt32(data[0]);
            this.sourceHeading = Convert.ToUInt32(data[0]);
        }
    }


}