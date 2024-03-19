using System;
using System.Numerics;
using ImGuiNET;

namespace MeterWay.Utils;

public class Canvas((Vector2, Vector2) area)
{
    public Vector2 Min { get; set; } = area.Item1;
    public Vector2 Max { get; set; } = area.Item2;

    public (Vector2 Min, Vector2 Max) Area => (Min, Max);

    public float Height => Min.Y - Max.Y;
    public float Width => Min.X - Max.X;

    public enum HorizontalAlign : int
    {
        Left,
        Center,
        Right
    }

    public enum VerticalAlign : int
    {
        Top,
        Center,
        Bottom
    }

    public Vector2 Align(Vector2 element, HorizontalAlign anchorX = HorizontalAlign.Left, VerticalAlign anchorY = VerticalAlign.Top)
    {
        float xPos = anchorX switch
        {
            HorizontalAlign.Left => Min.X,
            HorizontalAlign.Center => Min.X + (Max.X - Min.X) / 2 - (element.X / 2),
            HorizontalAlign.Right => Max.X - element.X,
            _ => throw new NotImplementedException(),
        };
        float yPos = anchorY switch
        {
            VerticalAlign.Top => Min.Y,
            VerticalAlign.Center => Min.Y + (Max.Y - Min.Y) / 2 - (element.Y / 2),
            VerticalAlign.Bottom => Max.Y - element.Y,
            _ => throw new NotImplementedException()
        };
        return new Vector2(xPos, yPos);
    }
    
    public Vector2 Align(string text, HorizontalAlign anchorX = HorizontalAlign.Left, VerticalAlign anchorY = VerticalAlign.Top)
    {
        return Align(ImGui.CalcTextSize(text), anchorX, anchorY);
    }

    public void AddMin(float x, float y) => Min = new Vector2(Min.X + x, Min.Y + y);
    public void AddMax(float x, float y) => Max = new Vector2(Max.X + x, Max.Y + y);

    public void Padding(float padding)
    {
        var temp = new Vector2(padding, padding);
        Min += temp;
        Max -= temp;
    }

    public void Padding((float Horizontal, float Vertical) padding)
    {
        var temp = new Vector2(padding.Horizontal, padding.Vertical);
        Min += temp;
        Max -= temp;
    }

    public void Move(Vector2 amount)
    {
        Min += amount;
        Max += amount;
    }

    public void Move((float Horizontal, float Vertical) amount)
    {
        var temp = new Vector2(amount.Horizontal, amount.Vertical);
        Min += temp;
        Max += temp;
    }
}