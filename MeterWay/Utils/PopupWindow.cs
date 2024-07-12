
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ImGuiNET;

namespace MeterWay.Utils;

public class PopupWindow
{
    public Action<TaskCompletionSource> Content { get; private init; }
    public string Label { get; private init; }
    public Vector2 Size { get; private init; }
    public bool IsModal { get; private init; }
    public ImGuiWindowFlags WindowFlags { get; set; } = ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoCollapse;    

    private TaskCompletionSource TaskSource { get; set; } = new();
    private bool FirstDraw { get; set; } = true;
    
    private static int counterStatic;
    private readonly int counter;

    public PopupWindow(string label, Action<TaskCompletionSource> content, bool isModal = false, Vector2? size = null)
    {
        counter = Interlocked.Increment(ref counterStatic);

        Content = content;
        Label = $"{label}##{counter}";
        Size = size ?? new(533, 300);
        IsModal = isModal;

        Dalamud.PluginInterface.UiBuilder.Draw += Draw;
        TaskSource.Task.ContinueWith(t =>
        {
            _ = t.Exception;
            Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
        });
    }

    public PopupWindow(string label, Action content, bool isModal = false, Vector2? size = null) : this(label, taskSource => content.Invoke(), isModal, size) { }

    private void Draw()
    {
        if (FirstDraw)
        {
            if (IsModal) ImGui.OpenPopup(Label);
            Vector2 center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(Size, ImGuiCond.Always);
        }
        ImGui.SetNextWindowSizeConstraints(new Vector2(320f, 180), new Vector2(float.MaxValue));

        if (IsModal)
        {
            bool p_open = true;
            if (!ImGui.BeginPopupModal(Label, ref p_open, WindowFlags) || !p_open)
            {
                TaskSource.SetCanceled();
                return;
            }

            ImGui.GetIO().WantCaptureKeyboard = true;
            ImGui.GetIO().WantTextInput = true;
            if (ImGui.IsKeyPressed(ImGuiKey.Escape))
            {
                TaskSource.SetCanceled();
                return;
            }
        }
        else
        {
            bool p_open2 = true;
            if (!ImGui.Begin(Label, ref p_open2, WindowFlags) || !p_open2)
            {
                ImGui.End();
                TaskSource.SetCanceled();
                return;
            }
        }

        Content.Invoke(TaskSource);

        if (IsModal) ImGui.EndPopup();
        else ImGui.End();

        FirstDraw = false;
    }
}