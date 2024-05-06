
using System;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;

namespace MeterWay.Utils;
    
public class PopupWindow
{
    public Action<TaskCompletionSource> Content { get; private init; }
    public string Label { get; private init; }
    public Vector2 Size { get; private init; }

    private TaskCompletionSource TaskSource { get; set; } = new();
    private bool FirstDraw { get; set; } = true;

    public PopupWindow(string label, Action<TaskCompletionSource> content, Vector2? size = null)
    {
        Content = content;
        Label = label;
        Size = size ?? new(600, 400);

        Dalamud.PluginInterface.UiBuilder.Draw += Draw;
        TaskSource.Task.ContinueWith(t =>
        {
            _ = t.Exception;
            Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
        });
    }

    public PopupWindow(string label, Action content, Vector2? size = null) : this(label, taskSource => content.Invoke(), size) {}

    private void Draw()
    {
        if (FirstDraw)
        {
            ImGui.OpenPopup(Label);
            Vector2 center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(Size, ImGuiCond.Always);
        }
        ImGui.SetNextWindowSizeConstraints(new Vector2(320f, 180), new Vector2(float.MaxValue));

        bool p_open = true;
        if (!ImGui.BeginPopupModal(Label, ref p_open) || !p_open)
        {
            TaskSource.SetCanceled();
            return;
        }

        Content.Invoke(TaskSource);

        ImGui.EndPopup();

        FirstDraw = false;
    }
}