using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeterWay.Utils;

/// <summary>
/// A class to represent a single placeholder in a string.
/// </summary>
[Serializable]
public class SinglePlaceHolder(int begin, int end, Node node) : Node(node)
{
    public int Begin { get; set; } = begin;
    public int End { get; set; } = end;
}

/// <summary>
/// A class to format a string with placeholders and replace them with values.
/// </summary>
[Serializable]
public class StringFormater
{
    public string Original { get; private set; }
    public SinglePlaceHolder[] PlaceHolders { get; private set; }

    public StringFormater(string original, SinglePlaceHolder[] placeHolders)
    {
        Original = original;
        PlaceHolders = placeHolders;
    }

    public StringFormater(string format)
    {
        Original = format;

        List<SinglePlaceHolder> result = [];
        int startIndex = 0;

        while (true)
        {
            int openIndex = format.IndexOf('{', startIndex);
            if (openIndex == -1) break;

            int closeIndex = format.IndexOf('}', openIndex);
            if (closeIndex == -1) break;
            startIndex = closeIndex;

            var node = PlaceHolder.Get(format[openIndex..closeIndex].Split('.'));
            if (node != null) result.Add(new(openIndex, closeIndex, node));
        }

        PlaceHolders = [.. result];
    }

    /// <summary>Replace the placeholders in the string with the given replacements.</summary>
    /// <param name="replaces">The values to replace the placeholders with.</param>
    /// <returns>The string with the placeholders replaced.</returns>
    /// <exception cref="Exception">Thrown if the number of replacements does not match the number of placeholders.</exception>
    public string Build(IEnumerable<string> replaces)
    {
        if (replaces.Count() != PlaceHolders.Length) throw new Exception("StringFormater Build: replaces size does not match PlaceHolders size");

        StringBuilder sb = new(Original);

        int index = 0;
        foreach (var placeHolder in PlaceHolders)
        {
            sb.Remove(placeHolder.Begin, placeHolder.End - placeHolder.Begin + 1);
            sb.Insert(placeHolder.Begin, replaces.ElementAt(index));
            index++;
        }

        return sb.ToString();
    }
}