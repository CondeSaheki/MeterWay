
using System.Numerics;
using ImGuiNET;
using MeterWay.Managers;

namespace MeterWay.Utils.Draw;

public static class Widget
{
    public enum TextAnchor
    {
        Left,
        Center,
        Right
    }

    public static void Text(string text, Vector2 position, uint color, Vector2 windowMin, Vector2 windowMax, bool fullLine = true, TextAnchor anchor = TextAnchor.Left, bool dropShadow = false, float scale = 1f)
    {
        ImGui.SetWindowFontScale(scale * ConfigurationManager.Inst.Configuration.OverlayFontScale);
        var size = CalcTextSize(text);
        switch (anchor)
        {
            case TextAnchor.Right:
                position.X -= size.X * scale;
                break;
            case TextAnchor.Center:
                if (fullLine)
                {
                    position.X = windowMax.X / 2 + position.X / 2;
                    position.X -= (size.X * scale) / 2;
                    break;
                }
                position.X -= (size.X * scale) / 2;
                break;
        }

        if (scale != 1)
        {
            position.Y += 3f * scale * ConfigurationManager.Inst.Configuration.OverlayFontScale;
        }

        if (dropShadow)
        {
            ImGui.GetWindowDrawList().AddText(position + new Vector2(1, 1), Helpers.Color(0, 0, 0, 255), text);
        }

        ImGui.GetWindowDrawList().AddText(position, color, text);
        ImGui.SetWindowFontScale(ConfigurationManager.Inst.Configuration.OverlayFontScale);
    }

    public static void JobIcon(uint job, Vector2 position, float scale = 1f, bool drawBorder = false)
    {
        var jobIcon = Job.GetIcon(job);

        var size = new Vector2(scale, scale);
        if (drawBorder)
        {
            ImGui.GetWindowDrawList().AddRectFilled(position, position + size, Helpers.Color(30, 30, 30, 190));
            ImGui.GetWindowDrawList().AddRect(position, position + size, Helpers.Color(80, 80, 80, 194));
        }
        ImGui.GetWindowDrawList().AddImage(jobIcon, position, position + size);
    }

    public static Vector2 CalcTextSize(string text)
    {
        float width = 0;

        foreach (char c in text)
        {
            width += ImGui.GetFont().GetCharAdvance(c) * ImGui.GetFont().Scale;
        }
        return new Vector2(width * ConfigurationManager.Inst.Configuration.OverlayFontScale, ImGui.GetFontSize() * ImGui.GetFont().Scale * ConfigurationManager.Inst.Configuration.OverlayFontScale);
    }

    public static void DrawProgressBar(Vector2 startPoint, Vector2 endPoint, uint color, float progress)
    {
        uint blackColor = Helpers.Color(0, 0, 0, 255);
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(startPoint, new Vector2(startPoint.X + (endPoint.X - startPoint.X) * progress, endPoint.Y), blackColor, color, color, blackColor);
    }

    public static void DrawBorder(Vector2 startPoint, Vector2 endPoint, uint color)
    {
        ImGui.GetWindowDrawList().AddRect(startPoint, endPoint, color);
    }

    public static void DrawProgressBarWithText(float size, uint color, float progress, string text)
    {
        //wip
        var windowMin = ImGui.GetCursorScreenPos();
        var windowMax = new Vector2(ImGui.GetWindowSize().X + windowMin.X - 32, windowMin.Y + size);
        DrawProgressBar(windowMin, windowMax, color, progress);
        DrawBorder(windowMin, windowMax, color);
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMin.X + 15, windowMin.Y + 10), Helpers.Color(255, 255, 255, 255), text);
    }

    public static void DrawProgressBarCheckpoint(Vector2 pmin, Vector2 pmax, uint color, float checkpoint)
    {
        var p = pmin.X + (pmax.X - pmin.X) * checkpoint;
        ImGui.GetWindowDrawList().AddLine(new Vector2(p, pmin.Y), new Vector2(p, pmax.Y), Helpers.Color(255, 255, 255, 255));
    }
}