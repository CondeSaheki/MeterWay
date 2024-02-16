
using System.Numerics;
using ImGuiNET;
using MeterWay.managers;
using MeterWay.Utils;

public static class Widget
{
    public enum TextAnchor
    {
        Left,
        Center,
        Right
    }

    public static void Text(string text, Vector2 position, uint color, bool fullLine = true, TextAnchor anchor = TextAnchor.Right, bool dropShadow = false, float scale = 1f)
    {
        // r
        var formerScale = ConfigurationManager.Instance.Configuration.OverlayFontScale;
        //
        ImGui.SetWindowFontScale(scale * formerScale);
        var size = CalcTextSize(text) * scale;
        switch (anchor)
        {
            case TextAnchor.Left:
                position.X -= size.X;
                break;
            case TextAnchor.Center:
                if (fullLine)
                {
                    position.X /= 2;
                    position.X -= size.X / 2;
                    break;
                }
                position.X -= size.X / 2;
                break;
        }

        if (dropShadow)
        {
            ImGui.GetWindowDrawList().AddText(position + new Vector2(1, 1) * scale, Helpers.Color(0, 0, 0, 255), text);
        }

        ImGui.GetWindowDrawList().AddText(position, color, text);
        ImGui.SetWindowFontScale(formerScale);
    }

    public static void JobIcon(uint job, Vector2 position, float scale = 1f)
    {
        var jobIcon = Job.GetIcon(job);

        var size = new Vector2(scale, scale);
        ImGui.GetWindowDrawList().AddImage(jobIcon, position, position + size);
    }


    public static Vector2 CalcTextSize(string text)
    {
        float height = ConfigurationManager.Instance.Configuration.OverlayFontSize;
        float width = 0;
        // r
        float scale = ConfigurationManager.Instance.Configuration.OverlayFontScale;
        //

        foreach (char c in text)
        {
            width += ImGui.GetFont().GetCharAdvance(c) * scale;
        }
        return new Vector2(width, height);
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