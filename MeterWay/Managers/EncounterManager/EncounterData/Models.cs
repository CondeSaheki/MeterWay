namespace MeterWay.Data;

// Damage

public class Damage(DamageValue? value = default, DamageCount? count = default, DamageSecond? second = default)
{
    public DamageValue Value { get; set; } = value ?? new DamageValue();
    public DamageCount Count { get; set; } = count ?? new DamageCount();
    public DamageSecond Second { get; set; } = second ?? new DamageSecond();

    public override string ToString() => $"Value:\n{Value}\nCount:\n{Count}";

    public void Calculate()
    {
        Value.Calculate();
        Count.Calculate();
        Second.Calculate(this);
    }
}

public class DamageValue(uint total = 0, uint critical = 0, uint direct = 0, uint criticalDirect = 0, DamageValuePercent? percent = default)
{
    public uint Total { get; set; } = total;
    public uint Critical { get; set; } = critical;
    public uint Direct { get; set; } = direct;
    public uint CriticalDirect { get; set; } = criticalDirect;
    public DamageValuePercent Percent { get; set; } = percent ?? new DamageValuePercent();

    public override string ToString() => $"Total={Total}, Critical={Critical}, Direct={Direct}, CriticalDirect={CriticalDirect}\nPercent:\n{Percent}";

    public void Calculate() => Percent.Calculate(this);
}

public class DamageValuePercent(float Neutral = 0f, float critical = 0f, float direct = 0f, float criticalDirect = 0f)
{
    public float Neutral { get; set; } = Neutral;
    public float Critical { get; set; } = critical;
    public float Direct { get; set; } = direct;
    public float CriticalDirect { get; set; } = criticalDirect;

    public override string ToString() => $"Neutral={Neutral} Critical={Critical}, Direct={Direct}, CriticalDirect={CriticalDirect}";

    public void Calculate(DamageValue value)
    {
        if (value.Total == 0) return;
        Neutral = (float)(value.Total - value.Critical - value.Direct - value.CriticalDirect) / value.Total * 100;
        Critical = (float)value.Critical / value.Total * 100;
        Direct = (float)value.Direct / value.Total * 100;
        CriticalDirect = (float)value.CriticalDirect / value.Total * 100;
    }
}

public class DamageCount(uint total = 0, uint critical = 0, uint direct = 0, uint criticalDirect = 0, uint miss = 0, uint parry = 0, DamageCountPercent? percent = default)
{
    public uint Total { get; set; } = total;
    public uint Critical { get; set; } = critical;
    public uint Direct { get; set; } = direct;
    public uint CriticalDirect { get; set; } = criticalDirect;
    public uint Miss { get; set; } = miss;
    public uint Parry { get; set; } = parry;
    public DamageCountPercent Percent { get; set; } = percent ?? new DamageCountPercent();

    public override string ToString() => $"Total={Total}, Critical={Critical}, Direct={Direct}, CriticalDirect={CriticalDirect}, Miss={Miss}, Parry={Parry}\nPercent:\n{Percent}";

    public void Calculate() => Percent.Calculate(this);
}

public class DamageCountPercent(float Neutral = 0f, float critical = 0f, float direct = 0f, float criticalDirect = 0f, float miss = 0f, float parry = 0f)
{
    public float Neutral { get; set; } = Neutral;
    public float Critical { get; set; } = critical;
    public float Direct { get; set; } = direct;
    public float CriticalDirect { get; set; } = criticalDirect;
    public float Miss { get; set; } = miss;
    public float Parry { get; set; } = parry;

    public override string ToString() => $"Neutral={Neutral}, Critical={Critical}, Direct={Direct}, CriticalDirect={CriticalDirect}, Miss={Miss}, Parry={Parry}";

    public void Calculate(DamageCount count)
    {
        if (count.Total != 0)
        {
            Neutral = (float)(count.Total - count.Critical - count.Direct - count.CriticalDirect) / count.Total * 100;
            Critical = (float)count.Critical / count.Total * 100;
            Direct = (float)count.Direct / count.Total * 100;
            CriticalDirect = (float)count.CriticalDirect / count.Total * 100;
        }
        if (count.Miss != 0) Miss = (float)count.Miss / (count.Total + count.Parry + count.Miss);
        if (count.Parry != 0) Parry = (float)count.Parry / (count.Total + count.Parry + count.Miss);
    }
}

public class DamageSecond(float total = 0f) // , float critical = 0f, float direct = 0f, float criticalDirect = 0f
{
    public float Total { get; set; } = total;
    // public float Critical { get; set; } = critical;
    // public float Direct { get; set; } = direct;
    // public float CriticalDirect { get; set; } = criticalDirect;

    public override string ToString() => $"Total={Total}"; // , Critical={Critical}, Direct={Direct}, CriticalDirect={CriticalDirect}

    public void Calculate(Damage damage)
    {
        var seconds = 1f;// Duration.TotalSeconds <= 1 ? 1 : Duration.TotalSeconds; // overflow protection
        Total = damage.Value.Total / seconds;

        // Critical = damage.Value.Critical / seconds;
        // Direct = damage.Value.Direct / seconds;
        // CriticalDirect = damage.Value.CriticalDirect / seconds;
    }
}

// Heal

public class Heal(HealValue? value = default, HealCount? count = default)
{
    public HealValue Value { get; set; } = value ?? new HealValue();
    public HealCount Count { get; set; } = count ?? new HealCount();

    public override string ToString() => $"Value:\n{Value}\nCount:\n{Count}";

    public void Calculate()
    {
        Value.Calculate();
        Count.Calculate();
    }
}

public class HealValue(uint total = 0, uint critical = 0, HealValuePercent? Percent = default)
{
    public uint Total { get; set; } = total;
    public uint Critical { get; set; } = critical;
    public HealValuePercent Percent { get; set; } = Percent ?? new HealValuePercent();

    public override string ToString() => $"Total={Total}, Critical={Critical}\nCount:\n{Percent}";

    public void Calculate() => Percent.Calculate(this);
}

public class HealValuePercent()
{
    public float Neutral { get; set; } = 0f;
    public float Critical { get; set; } = 0f;

    public override string ToString() => $"Neutral={Neutral}, Critical={Critical}";

    public void Calculate(HealValue value)
    {
        if (value.Total == 0) return;
        Neutral = (float)(value.Total - value.Critical) / value.Total * 100;
        Critical = (float)value.Critical / value.Total * 100;
    }
}

public class HealCount(uint total = 0, uint critical = 0, HealCountPercent? percent = default)
{
    public uint Total { get; set; } = total;
    public uint Critical { get; set; } = critical;
    public HealCountPercent Percent { get; set; } = percent ?? new HealCountPercent();

    public override string ToString() => $"Total={Total}, Critical={Critical}\nCount:\n{Percent}";

    public void Calculate() => Percent.Calculate(this);
}

public class HealCountPercent()
{
    public float Neutral { get; set; } = 0f;
    public float Critical { get; set; } = 0f;

    public override string ToString() => $"Neutral={Neutral}, Critical={Critical}";

    public void Calculate(HealCount count)
    {
        if (count.Total == 0) return;
        Neutral = (float)(count.Total - count.Critical) / count.Total * 100;
        Critical = (float)count.Critical / count.Total * 100;
    }
}