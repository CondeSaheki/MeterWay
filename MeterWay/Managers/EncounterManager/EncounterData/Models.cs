namespace MeterWay.Data;

public class Damage(uint total = 0, uint critical = 0, uint direct = 0, uint criticalDirect = 0, DamageCount? count = default)
{
    public uint Total { get; set; } = total;
    public uint Critical { get; set; } = critical;
    public uint Direct { get; set; } = direct;
    public uint CriticalDirect { get; set; } = criticalDirect;
    public DamageCount Count { get; set; } = count ?? new DamageCount();

    public override string ToString() => $"Total={Total}, Critical={Critical}, Direct={Direct}, CriticalDirect={CriticalDirect}, Count: {Count}";
}

public class DamageCount(uint total = 0, uint critical = 0, uint direct = 0, uint criticalDirect = 0, uint miss = 0, uint parry = 0)
{
    public uint Total { get; set; } = total;
    public uint Critical { get; set; } = critical;
    public uint Direct { get; set; } = direct;
    public uint CriticalDirect { get; set; } = criticalDirect;
    public uint Miss { get; set; } = miss;
    public uint Parry { get; set; } = parry;

    public override string ToString() => $"Total={Total}, Critical={Critical}, Direct={Direct}, CriticalDirect={CriticalDirect}, Miss={Miss}, Parry={Parry}";
}

public class Heal(uint total = 0, uint critical = 0, HealCount? count = default)
{
    public uint Total { get; set; } = total;
    public uint Critical { get; set; } = critical;
    public HealCount Count { get; set; } = count ?? new HealCount();

    public override string ToString() => $"Total={Total}, Critical={Critical}, Count: {Count}";
}

public class HealCount(uint total = 0, uint critical = 0)
{
    public uint Total { get; set; } = total;
    public uint Critical { get; set; } = critical;

    public override string ToString() => $"Total={Total}, Critical={Critical}";
}