using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Security.AccessControl;
using Meterway.Managers;

namespace MeterWay.Utils
{
    public static class Helpers
    {

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
                if(ignore(i)) sorted.Add(i.Key);
            }

            return sorted;
        }

public static void Log(string message)
{
    PluginManager.Instance.PluginLog.Info(message);
}
    }
}