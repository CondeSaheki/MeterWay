using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;


namespace MeterWay.Windows;

public class MainWindow : Window, IDisposable
{
    public MainWindow() : base("MeterWay", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("wip");
    }

    private void DrawProgressBarWithText(float size, uint color, float progress, string text)
    {
        //wip
        var windowMin = ImGui.GetCursorScreenPos();
        var windowMax = new Vector2(ImGui.GetWindowSize().X + windowMin.X - 32, windowMin.Y + size);
        DrawProgressBar(windowMin, windowMax, color, progress);
        DrawBorder(windowMin, windowMax, color);
        ImGui.GetWindowDrawList().AddText(new Vector2(windowMin.X + 15, windowMin.Y + 10), Color(255, 255, 255, 255), text);
    }

    private void DrawBorder(Vector2 windowmin, Vector2 windowmax, uint color)
    {
        ImGui.GetWindowDrawList().AddRect(windowmin, windowmax, color);
    }
    private void DrawProgressBar(Vector2 pmin, Vector2 pmax, uint color, float progress)
    {
        ImGui.GetWindowDrawList().AddRectFilled(pmin, new Vector2(pmin.X + (pmax.X - pmin.X) * progress, pmax.Y), color);
    }
    private void DrawProgressBarCheckpoint(Vector2 pmin, Vector2 pmax, uint color, float checkpoint)
    {
        var p = pmin.X + (pmax.X - pmin.X) * checkpoint;
        ImGui.GetWindowDrawList().AddLine(new Vector2(p, pmin.Y), new Vector2(p, pmax.Y), Color(255, 255, 255, 255));
    }
    public static uint Color(byte r, byte g, byte b, byte a) { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }
}
