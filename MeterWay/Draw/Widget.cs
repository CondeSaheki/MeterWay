
using System.Numerics;
using ImGuiNET;
using Meterway;
using Meterway.Utils;

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
        var formerScale = ImGui.GetFont().Scale;
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

    public static void JobIcon(string job, Vector2 position, float scale = 1f)
    {
        var jobIcon = Job.GetIcon(job);

        var size = new Vector2(32 * scale, 32 * scale);
        ImGui.GetWindowDrawList().AddImage(jobIcon, position, position + size);
    }


    public static Vector2 CalcTextSize(string text)
    {
        float height = ImGui.GetFontSize();
        float width = 0;

        foreach (char c in text)
        {
            width += ImGui.GetFont().GetCharAdvance(c) * ImGui.GetFont().Scale;
        }
        return new Vector2(width, height);
    }
}