using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;

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

    public class PopupWindow
    {
        public Action Content { get; private init; }
        public string Label { get; private init; }
        public Vector2 Size { get; private init; }

        private TaskCompletionSource TaskSource { get; set; } = new();
        private bool FirstDraw { get; set; } = true;

        public PopupWindow(string label, Action content, Vector2? size = null)
        {
            Content = content;
            Label = label;
            Size = size ?? new(600, 400);

            Dalamud.PluginInterface.UiBuilder.Draw += Draw;
            TaskSource.Task.ContinueWith(t =>
            {
                _ = t.Exception;
                Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
            });
        }

        private void Draw()
        {
            if (FirstDraw)
            {
                ImGui.OpenPopup(Label);
                Vector2 center = ImGui.GetMainViewport().GetCenter();
                ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
                ImGui.SetNextWindowSize(Size, ImGuiCond.Always);
            }
            ImGui.SetNextWindowSizeConstraints(new Vector2(320f, 180), new Vector2(float.MaxValue));

            bool p_open = true;
            if (!ImGui.BeginPopupModal(Label, ref p_open) || !p_open)
            {
                TaskSource.SetCanceled();
                return;
            }

            Content.Invoke();

            ImGui.EndPopup();

            FirstDraw = false;
        }
    }
}