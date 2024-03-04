using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using MeterWay.Managers;

namespace MeterWay.Utils;

public static class Helpers
{
    public static uint CreateId()
    {
        return (uint)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
    }

    public static double Lerp(double firstFloat, double secondFloat, double by)
    {
        return firstFloat + (secondFloat - firstFloat) * by;
    }

    public static string HumanizeNumber(double number, int decimals = 1)
    {
        string[] suffixes = { "", "k", "M", "B", "T" };
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

    public static uint Color(byte r, byte g, byte b, byte a)
    {
        uint ret = a;
        ret <<= 8;
        ret += b;
        ret <<= 8;
        ret += g;
        ret <<= 8;
        ret += r;
        return ret;
    }

    public static uint Color(Vector4 color)
    {
        return Color(Convert.ToByte(Math.Min(Math.Round(color.X * 255), 255)), Convert.ToByte(Math.Min(Math.Round(color.Y * 255), 255)),
            Convert.ToByte(Math.Min(Math.Round(color.Z * 255), 255)), Convert.ToByte(Math.Round(color.W * 255)));
    }


    // /(type first, type second) => {return first > second;}
    public static List<Id> CreateDictionarySortCache<Id, Type>(Dictionary<Id, Type> data, Func<KeyValuePair<Id, Type>, bool> ignore) where Id : notnull
    {
        List<Id> sorted = new List<Id>();

        foreach (KeyValuePair<Id, Type> i in data)
        {
            if (ignore(i)) sorted.Add(i.Key);
        }

        return sorted;
    }

    public static void Log(string message)
    {
        InterfaceManager.Inst.PluginLog.Info(message);
    }

    public static List<ReadOnlyMemory<char>> SplitStringAsMemory(string input, char delimiter)
    {
        List<ReadOnlyMemory<char>> result = [];
        int startIndex = 0;
        for (int index = 0; index != input.Length; ++index)
        {
            if (input[index] == delimiter)
            {
                result.Add(input.AsMemory(startIndex, index));
                startIndex = index + 1;
            }
        }
        if (input.Length != startIndex) result.Add(input.AsMemory(startIndex, input.Length));
        return result;
    }

    public static Vector4? Vec4Parse(List<ReadOnlyMemory<char>> data, int x, int y, int z, int h)
    {
        if (data[x].IsEmpty || data[y].IsEmpty || data[z].IsEmpty || data[h].IsEmpty) return null;
        return new Vector4(float.Parse(data[x].Span), float.Parse(data[y].Span), float.Parse(data[z].Span), float.Parse(data[h].Span));
    }

}
