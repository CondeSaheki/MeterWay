using System.Numerics;
using ImGuiNET;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;

using MeterWay.Utils;
using MeterWay.Overlay;

namespace Lazer;

public partial class Overlay : IOverlay, IOverlayConfig
{
    private static readonly uint colorWhite = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
    private static readonly uint colorBlack = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));

    private Lerp<Dictionary<uint, PlayerData>> CreateLerping()
    {
        return new(() => // Linear Interpolation
        {
            var now = DateTime.Now;
            double M = (now - Lerping!.Begin!.Value.Time).TotalMilliseconds / (Lerping.End!.Value.Time - Lerping.Begin!.Value.Time).TotalMilliseconds;

            Dictionary<uint, PlayerData> result = [];
            foreach (var key in Lerping.End.Value.Data.Keys)
            {
                result[key] = new PlayerData
                {
                    Progress = Lerping.Begin.Value.Data[key].Progress + M * (Lerping.End.Value.Data[key].Progress - Lerping.Begin.Value.Data[key].Progress),
                };
            }
            return result;
        },
        (left, right) => // Compare
        {
            foreach (var elem in left)
            {
                if (elem.Value.Progress != right[elem.Key].Progress) return false;
            }
            return true;
        });
    }

    private static void DrawJobIcon(Canvas area, uint job)
    {
        var icon = MeterWay.Dalamud.Textures.GetIcon(job + 62000u, ITextureProvider.IconFlags.None);
        if (icon == null) return;
        ImGui.GetWindowDrawList().AddImage(icon.ImGuiHandle, area.Min, area.Max);
    }

    private static void DrawProgressBar(Vector2 min, Vector2 max, uint color, float progress)
    {
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(min, new Vector2(min.X + (max.X - min.X) * progress, max.Y), colorBlack, color, color, colorBlack);
    }
}
