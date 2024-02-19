
using System.Numerics;
using ImGuiNET;
using Meterway.Managers;

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
        ImGui.SetWindowFontScale(scale * ConfigurationManager.Instance.Configuration.OverlayFontScale);
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
            position.Y += 3f * scale * ConfigurationManager.Instance.Configuration.OverlayFontScale;
        }

        if (dropShadow)
        {
            ImGui.GetWindowDrawList().AddText(position + new Vector2(1, 1), Helpers.Color(0, 0, 0, 255), text);
        }

        ImGui.GetWindowDrawList().AddText(position, color, text);
        ImGui.SetWindowFontScale(ConfigurationManager.Instance.Configuration.OverlayFontScale);
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
        return new Vector2(width * ConfigurationManager.Instance.Configuration.OverlayFontScale, ImGui.GetFontSize() * ImGui.GetFont().Scale * ConfigurationManager.Instance.Configuration.OverlayFontScale);
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
}