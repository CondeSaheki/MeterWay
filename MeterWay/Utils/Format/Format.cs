using System;
using System.Collections.Generic;
using System.Text;

namespace MeterWay.Utils;

/// <summary>
/// Contains information about the placeholder and its position in the original string.
/// </summary>
public class FormatInfo(PlaceHolder? placeHolder, (int Begin, int End) position, Format format)
{
    public PlaceHolder? PlaceHolder { get; set; } = placeHolder;
    public (int Begin, int End) Position { get; set; } = position;
    
    private readonly Format Format = format;

    /// <summary>
    /// Returns the target string of the format placeholder.
    /// </summary>
    /// <param name="info">The format placeholder information.</param>
    /// <returns>The target string of the format placeholder.</returns>
    public string Target => Format.Original[Position.Begin..Position.End];

}

/// <summary>
/// A class to format a string with placeholders and replace them with values.
/// </summary>
public class Format
{
    public string Original { get; private set; }
    public FormatInfo[] Infos { get; private set; }

    public Format(string format)
    {
        Original = format;

        List<FormatInfo> result = [];
        var isFound = false;
        var beginIndex = -1;

        for (var index = 0; index != format.Length; ++index)
        {
            if (format[index] == '{' && !isFound)
            {
                isFound = true;
                beginIndex = index;
            }
            else if (format[index] == '}' && isFound)
            {
                isFound = false;
                result.Add(new(PlaceHolder.Find(format[(beginIndex + 1)..index].Split('.')), (beginIndex, index + 1), this));
            }
        }
        Infos = [.. result];
    }

    /// <summary>Replace the placeholders in the string with the given replacements.</summary>
    /// <param name="replacements">The values to replace the placeholders with.</param>
    /// <returns>The string with the placeholders replaced.</returns>
    /// <exception cref="Exception">Thrown if the number of replacements does not match the number of placeholders.</exception>
    public string Build(List<string> replacements)
    {
        if (Infos.Length != replacements.Count) throw new Exception("Replacements and placeholders length do not match.");

        StringBuilder result = new();
        int currentIndex = 0;

        for (var index = 0; index != Infos.Length; ++index)
        {
            result.Append(Original[currentIndex..Infos[index].Position.Begin]);
            result.Append(replacements[index]);
            currentIndex = Infos[index].Position.End;
        }

        if (currentIndex < Original.Length) result.Append(Original[currentIndex..]);

        return result.ToString();
    }
}