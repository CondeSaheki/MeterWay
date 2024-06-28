using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace MeterWay.Utils;

public static class Helpers
{
    public static uint CreateId() => (uint)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

    public static string HumanizeNumber(double number, int decimals = 1)
    {
        string[] suffixes = ["", "k", "M", "B", "T"];
        int suffixIndex = 0;

        while (number >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            number /= 1000;
            suffixIndex++;
        }
        // return with no decimais if below 1k
        if (suffixIndex == 0)
        {
            return $"{number.ToString("0")}";
        }

        // use . for decimals
        return $"{number.ToString($"0.{new string('0', decimals)}", CultureInfo.InvariantCulture)}{suffixes[suffixIndex]}";
    }

    // /(type first, type second) => {return first > second;}
    public static List<Id> CreateDictionarySortCache<Id, Type>(Dictionary<Id, Type> data, Func<KeyValuePair<Id, Type>, bool> ignore) where Id : notnull
    {
        List<Id> sorted = [];

        foreach (KeyValuePair<Id, Type> i in data)
        {
            if (ignore(i)) sorted.Add(i.Key);
        }

        return sorted;
    }

    public static List<ReadOnlyMemory<char>> SplitStringAsMemory(string input, char delimiter)
    {
        List<ReadOnlyMemory<char>> result = [];
        int startIndex = 0;
        for (int index = 0; index != input.Length; ++index)
        {
            if (input[index] == delimiter)
            {
                result.Add(input.AsMemory(startIndex, index - startIndex));
                startIndex = index + 1;
            }
        }
        if (input.Length != startIndex) result.Add(input.AsMemory(startIndex, input.Length - startIndex));
        return result;
    }

    public static List<ReadOnlyMemory<char>> SplitStringAsMemory(string input, char delimiter, int splits)
    {
        List<ReadOnlyMemory<char>> result = [];
        int startIndex = 0;
        int splitsCount = 0;
        for (int index = 0; index != input.Length && splitsCount != splits; ++index)
        {
            if (input[index] == delimiter)
            {
                result.Add(input.AsMemory(startIndex, index - startIndex));
                startIndex = index + 1;
                ++splitsCount;
            }
        }
        if (input.Length != startIndex && splitsCount != splits) result.Add(input.AsMemory(startIndex, input.Length - startIndex));
        return result;
    }

    public static Vector4? Vec4Parse(List<ReadOnlyMemory<char>> data, int x, int y, int z, int h)
    {
        if (data[x].IsEmpty || data[y].IsEmpty || data[z].IsEmpty || data[h].IsEmpty) return null;
        return new Vector4(float.Parse(data[x].Span), float.Parse(data[y].Span), float.Parse(data[z].Span), float.Parse(data[h].Span));
    }

    public static CancellationTokenSource DelayedAction(TimeSpan delay, Action action)
    {
        async Task Delay(CancellationTokenSource cancelSource)
        {
            try
            {
                await Task.Delay((int)delay.TotalMilliseconds, cancelSource.Token);
                action.Invoke();
            }
            catch (TaskCanceledException) { }
        }

        CancellationTokenSource cancelSource = new();
        _ = Delay(cancelSource);
        return cancelSource;
    }

    /// <summary>
    /// Overrides the alpha value of a given color with a new alpha value.
    /// </summary>
    /// <param name="color">The original color with alpha value.</param>
    /// <param name="newAlpha">The new alpha value to be applied.</param>
    /// <returns>The new color with the overridden alpha value.</returns>
    public static uint ColorU32AlphaOverride(uint color, float newAlpha)
    {
        newAlpha = Math.Clamp(newAlpha, 0.0f, 1.0f); // unneeded ? 
        return (color & 0x00FFFFFF) | ((uint)(newAlpha * 255.0f) << 24);
    }
}