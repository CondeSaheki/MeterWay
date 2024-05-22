using System;
using System.Threading;

namespace MeterWay.Utils;

public class ThreadSafeEnum<T>(T initialValue) where T : Enum
{
    private long _value = Convert.ToInt64(initialValue);

    public T Value
    {
        get
        {
            return (T)Enum.ToObject(typeof(T), Interlocked.Read(ref _value));
        }
        set
        {
            Interlocked.Exchange(ref _value, Convert.ToInt64(value));
        }
    }
}