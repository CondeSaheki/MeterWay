using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace MeterWay.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin { get; init; }

    public MainWindow(Plugin plugin) : base("MeterWay", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("wip");
    }
}
