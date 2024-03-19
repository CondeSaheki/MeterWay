using System.Numerics;
using ImGuiNET;
using System;
using Dalamud.Plugin.Services;
using Dalamud.Interface.ImGuiFontChooserDialog;
using Dalamud.Interface.ManagedFontAtlas;

using MeterWay.Utils;
using MeterWay.Overlay;

namespace Mogu;

public partial class Overlay : IOverlay, IOverlayTab
{
    private static void DrawTextShadow(Vector2 position, uint color, string text, uint shadowColor)
    {
        ImGui.GetWindowDrawList().AddText(position + new Vector2(1, 1), shadowColor, text);
        ImGui.GetWindowDrawList().AddText(position, color, text);
    }

    private void DrawFontChanger(IFontAtlas fontAtlas, IFontHandle fontHandle, string fontname, Vector4 configcolor)
    {
        ImGui.Spacing();
        ImGui.Text(fontname);
        ImGui.Spacing();

        var FontColorValue = configcolor;
        if (ImGui.ColorEdit4("Color", ref FontColorValue,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault))
        {
            configcolor = FontColorValue;
            File.Save(Name, Config);
        }

        if (ImGui.Button("Choose"))
        {
            SingleFontChooserDialog chooser = new(MeterWay.Dalamud.PluginInterface.UiBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async)) // need a new instance
            {
                Title = "Font Chooser",
                PreviewText = "0.123456789 abcdefghijklmnopqrstuvxyzw",
                //SelectedFont = new SingleFontSpec { FontId = DalamudDefaultFontAndFamilyId.Instance } // load id from config
                // FontFamilyExcludeFilter = x => x is DalamudDefaultFontAndFamilyId; // exclude especific fonts from being selected
                // FontId = original.FontId;
                // SizePx = original.SizePx;
            };
            chooser.ResultTask.ContinueWith(chooserTask =>
            {
                if (chooserTask.IsCompletedSuccessfully)
                {
                    fontHandle?.Dispose();
                    fontHandle = chooserTask.Result.CreateFontHandle(fontAtlas);
                    // TODO save config
                }
                MeterWay.Dalamud.PluginInterface.UiBuilder.Draw -= chooser.Draw;
                chooser.Dispose();
            });
            MeterWay.Dalamud.PluginInterface.UiBuilder.Draw += chooser.Draw;
        }
        ImGui.SameLine();
        if (ImGui.Button("Default"))
        {
            fontHandle?.Dispose();
            fontHandle = fontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(20)));
        }
        FontMogu.Push();
        string text = "abcdefghijklmnopqrstuvxyzw";
        ImGui.InputText("##text", ref text, 200);
        FontMogu.Pop();
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

    public uint GetJobColor(uint rawJob)
    {
        var index = Array.FindIndex(Config.JobColors, element => element.Job == new Job(rawJob).Id);
        if (index != -1) return Config.JobColors[index].Color;
        return Config.JobDefaultColor;
    }

    public static void DrawJobIcon(Vector2 cursor, float size, uint job)
    {
        var icon = MeterWay.Dalamud.Textures.GetIcon(job + 62000u, ITextureProvider.IconFlags.None);
        if (icon == null) return;

        ImGui.GetWindowDrawList().AddImage(icon.ImGuiHandle, cursor, new Vector2(cursor.X + size, cursor.Y + size));
    }
}