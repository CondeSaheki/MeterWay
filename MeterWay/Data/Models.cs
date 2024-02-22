namespace MeterWay.Data;

public class HitsCount(uint hit = 0, uint damageCrit = 0, uint damageDh = 0, uint damageCritDh = 0)
{
    public uint Hit { get; set; } = hit;
    public uint Crit { get; set; } = damageCrit;
    public uint Dh { get; set; } = damageDh;
    public uint CritDh { get; set; } = damageCritDh;
    public uint Miss { get; set; } = 0;
    public uint Parry { get; set; } = 0;
}

public class HealsCount(uint hit = 0, uint crit = 0)
{
    public uint Hit { get; set; } = hit;
    public uint Crit { get; set; } = crit;
}

public class DamageData(uint total = 0, uint totalCrit = 0, uint totalDirectCrit = 0, uint count = 0, uint countCrit = 0)
{
    public uint Total { get; set; } = total;
    public uint TotalCrit { get; set; } = totalCrit;
    public uint TotalDirectCrit { get; set; } = totalDirectCrit;
    public HitsCount Count { get; set; } = new HitsCount(count, countCrit);
}

public class HealingData(uint total = 0, uint totalCrit = 0, uint count = 0, uint countCrit = 0)
{
    public uint Total { get; set; } = total;
    public uint TotalCrit { get; set; } = totalCrit;
    public HealsCount Count { get; set; } = new HealsCount(count, countCrit);
}

