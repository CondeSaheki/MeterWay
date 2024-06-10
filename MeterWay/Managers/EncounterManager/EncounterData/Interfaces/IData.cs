namespace MeterWay.Data;

public interface IData
{
    public Damage DamageDealt { get; }
    public Damage DamageReceived { get; }

    public Heal HealDealt { get; }
    public Heal HealReceived { get; }

    public PerSeconds PerSeconds { get; }

    public void Calculate();
}