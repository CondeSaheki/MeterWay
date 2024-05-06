using System;

namespace MeterWay.Utils;

public class Lerp<T>(Func<T> interpolation, Func<T, T, bool> compare) where T : notnull
{
    public Func<T> Interpolation { get; init; } = interpolation;
    public Func<T, T, bool> Compare { get; init; } = compare;

    public (DateTime Time, T Data)? Begin { get; private set; } = null;
    public (DateTime Time, T Data)? End { get; private set; } = null;

    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);

    public T? Now()
    {
        if (End == null) return default;
        if (Begin != null && End.Value.Time > DateTime.Now) return Interpolation.Invoke();
        return End.Value.Data;
    }

    public void Update(T data)
    {
        DateTime now = DateTime.Now;
        T? current = Now();

        if (End != null && current != null && !Compare.Invoke(current, data))
        {
            End = (now + Interval, data);
            Begin = (now, (T)current);
            return;
        }
        End = (now, data);
        Begin = null;
    }

    public void Reset()
    {
        Begin = null;
        End = null;
    }
}