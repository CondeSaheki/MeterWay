using System;
using System.Numerics;
using ImGuiNET;
using Dalamud.Plugin.Services;
using Dalamud.Interface.ImGuiFontChooserDialog;
using Dalamud.Interface.FontIdentifier;

using MeterWay.Utils;
using MeterWay.Overlay;

namespace Mogu;

public partial class Overlay : BasicOverlay
{
    private static void DrawTextShadow(Vector2 position, uint color, string text, uint shadowColor)
    {
        ImGui.GetWindowDrawList().AddText(position + new Vector2(1, 1), shadowColor, text);
        ImGui.GetWindowDrawList().AddText(position, color, text);
    }

    private static void DrawProgressBar((Vector2 Min, Vector2 Max) rectangle, float progress, uint color)
    {
        rectangle.Max.X = rectangle.Min.X + ((rectangle.Max.X - rectangle.Min.X) * progress);
        ImGui.GetWindowDrawList().AddRectFilled(rectangle.Min, rectangle.Max, color);
    }

    private static bool DrawAlingmentButtons(ref int horizontal)
    {
        var result = false;
        if (ImGui.RadioButton("Left", ref horizontal, 0)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Center", ref horizontal, 1)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Right", ref horizontal, 2)) result = true;
        return result;
    }

    private static bool DrawAlingmentButtons(ref int horizontal, ref int vertical)
    {
        var result = false;
        if (ImGui.RadioButton("Left", ref horizontal, 0)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Center", ref horizontal, 1)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Right", ref horizontal, 2)) result = true;

        ImGui.Spacing();
        if (ImGui.RadioButton("Top", ref vertical, 0)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Center", ref vertical, 1)) result = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Botton", ref vertical, 2)) result = true;
        return result;
    }

    private uint GetJobColor(uint rawJob)
    {
        var index = Array.FindIndex(Config.Appearance.JobColors, element => element.Job == new Job(rawJob).Id);
        if (index != -1) return Config.Appearance.JobColors[index].Color;
        return Config.Appearance.JobDefaultColor;
    }

    private static void DrawJobIcon(Canvas area, uint job)
    {
        var icon = MeterWay.Dalamud.Textures.GetIcon(job + 62000u, ITextureProvider.IconFlags.None);
        if (icon == null) return;

        ImGui.GetWindowDrawList().AddImage(icon.ImGuiHandle, area.Min, area.Max);
    }

    public void _ImGuiBeginDisabled(ref bool value)
    {
        if (!value) ImGui.BeginDisabled();
        value = true;
    }

    public void _ImGuiEndDisabled(ref bool value)
    {
        if (value) ImGui.EndDisabled();
        value = false;
    }

    public static void _ImguiCheckboxWithTooltip(string? label, string? tooltip, bool current, Action<bool> setter)
    {
        var currentValue = current;
        if (ImGui.Checkbox(label ?? "", ref currentValue) && currentValue != current) setter(currentValue);

        if (!string.IsNullOrEmpty(tooltip) && ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(tooltip);
            ImGui.EndTooltip();
        }
    }

    private static void _ImGuiColorPick(string Label, uint current, Action<uint> setter)
    {
        var colorValue = ImGui.ColorConvertU32ToFloat4(current);
        if (ImGui.ColorEdit4(Label, ref colorValue,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault))
        {
            var converted = ImGui.ColorConvertFloat4ToU32(colorValue);
            if (current != converted) setter(ImGui.ColorConvertFloat4ToU32(colorValue));
        }
    }

    private void _SingleFontChooserDialog(string? label, SingleFontSpec? current, Action<SingleFontSpec> setter)
    {
        SingleFontChooserDialog chooser = new(MeterWay.Dalamud.PluginInterface.UiBuilder, false, null)
        {
            Title = label ?? "Font Chooser",
            PreviewText = "0.123456789 abcdefghijklmnopqrstuvxyzw",
            SelectedFont = current ?? new SingleFontSpec { FontId = DalamudDefaultFontAndFamilyId.Instance },
            IsModal = false,
        };
        chooser.ResultTask.ContinueWith(chooserTask =>
        {
            _ = chooserTask.Exception; // not needed ?
            if (chooserTask.IsCompletedSuccessfully) setter(chooserTask.Result);
            MeterWay.Dalamud.PluginInterface.UiBuilder.Draw -= chooser.Draw;
            chooser.Dispose();
        });
        MeterWay.Dalamud.PluginInterface.UiBuilder.Draw += chooser.Draw;
    }

    public class Lerp2<T>(Func<Lerp2<T>, (DateTime, T)> interpolation, Func<T, T, bool> compare) where T : notnull
    {
        public Func<Lerp2<T>, (DateTime, T)> Interpolation { get; init; } = interpolation;
        public Func<T, T, bool> Compare { get; init; } = compare;

        public (DateTime Time, T Data)? Now { get; private set; } = null;
        public (DateTime Time, T Data)? End { get; private set; } = null;

        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);

        public T? Update()
        {
            if (End == null) return default;
            if (Now != null && End.Value.Time > DateTime.Now)
            {
                Now = Interpolation.Invoke(this);
                return Now.Value.Data;
            }
            return End.Value.Data;
        }

        public void Update(T data)
        {
            DateTime now = DateTime.Now;
            T? current = Update();

            if (End != null && current != null && !Compare.Invoke(current, data))
            {
                End = (now + Interval, data);
                Now = (now, (T)current);
                return;
            }
            End = (now, data);
            Now = null;
        }

        public void Reset()
        {
            Now = null;
            End = null;
        }
    }

    private Lerp2<double> CreateLerping()
    {
        return new((lerp) =>
        {
            // now = end + (now - end) * exp(-decay*deltaTime)

            var timeNow = DateTime.Now;
            const int decay = 16;
            var now = lerp!.End!.Value.Data + (lerp.Now!.Value.Data - lerp!.End!.Value.Data) * Math.Exp(-decay * (timeNow - lerp.Now!.Value.Time).TotalSeconds);

            return (timeNow, now);
        },
        (left, right) => left == right);
    }
}