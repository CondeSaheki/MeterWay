namespace MeterWay.Data;

public class HitsCount(uint hit, uint damageCrit, uint damageDh, uint damageCritDh, uint healCrit)
{
    public uint Hit { get; set; } = hit;
    public uint DamageCrit { get; set; } = damageCrit;
    public uint DamageDh { get; set; } = damageDh;
    public uint DamageCritDh { get; set; } = damageCritDh;
    public uint HealCrit { get; set; } = healCrit;
}

public class DamageData(uint total, uint totalCrit, uint totalDirectCrit)
{
    public uint Total { get; set; } = total;
    public uint TotalCrit { get; set; } = totalCrit;
    public uint TotalDirectCrit { get; set; } = totalDirectCrit;
}

public class DamageTakenData(uint total, uint totalCrit, uint totalDirectCrit)
{
    public uint Total { get; set; } = total;
    public uint TotalCrit { get; set; } = totalCrit;
    public uint TotalDirectCrit { get; set; } = totalDirectCrit;
}

public class HealingData(uint total, uint totalCrit)
{
    public uint Total { get; set; } = total;
    public uint TotalCrit { get; set; } = totalCrit;
}
