using System;
using System.Numerics;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

using MeterWay.Overlay;

namespace Mogu;

[Serializable]
public class General()
{
    public Vector2 Size { get; set; } = new(320f, 180f);
    public Vector2 Position { get; set; } = new(192, 108f);

    public bool NoInput { get; set; } = false;
    public bool NoMove { get; set; } = false;
    public bool NoResize { get; set; } = false;

    public bool FrameCalc { get; set; } = true;
}

public partial class Overlay : BasicOverlay
{
    private void DrawGeneralTab()
    {
        using var tab = ImRaii.TabItem("General");
        if (!tab) return;

        ImGui.Spacing();
        ImGui.Text("Window");
        ImGui.Spacing();

        var isDisabled = false;
        if (!Window.IsOpen) _ImGuiBeginDisabled(ref isDisabled);

        ImGui.PushItemWidth(50);
        var heigthValue = (int)(Window.CurrentSize?.X ?? Config.General.Size.X);
        var widthValue = (int)(Window.CurrentSize?.Y ?? Config.General.Size.Y);
        if (ImGui.DragInt("##heigth", ref heigthValue) && heigthValue != (int)Config.General.Size.X && Window.CurrentSize != null)
        {
            var vec = new Vector2(heigthValue, widthValue);
            Window.SetSize(vec);
            Config.General.Size = vec;
            Save(Window.WindowName, Config);
        }
        ImGui.SameLine();

        if (ImGui.DragInt("##width", ref widthValue) && widthValue != (int)Config.General.Size.Y && Window.CurrentSize != null)
        {
            var vec = new Vector2(heigthValue, widthValue);
            Window.SetSize(vec);
            Config.General.Size = vec;
            Save(Window.WindowName, Config);
        }
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.Text("Size");

        ImGui.PushItemWidth(50);
        var XValue = (int)(Window.CurrentPosition?.X ?? Config.General.Position.X);
        var YValue = (int)(Window.CurrentPosition?.Y ?? Config.General.Position.Y);
        if (ImGui.DragInt("##XPos", ref XValue) && XValue != (int)Config.General.Position.X && Window.CurrentPosition != null)
        {
            var vec = new Vector2(XValue, YValue);
            Window.SetPosition(vec);
            Config.General.Position = vec;
            Save(Window.WindowName, Config);
        }
        ImGui.SameLine();
        if (ImGui.DragInt("##YPos", ref YValue) && YValue != (int)Config.General.Position.Y && Window.CurrentPosition != null)
        {
            var vec = new Vector2(XValue, YValue);
            Window.SetPosition(vec);
            Config.General.Position = vec;
            Save(Window.WindowName, Config);
        }
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.Text("Position");

        _ImGuiEndDisabled(ref isDisabled);

        ImGui.Spacing();
        ImGui.Text("Window Behavior");
        ImGui.Spacing();

        _ImguiCheckboxWithTooltip("No Input", "Disables all input for this window.", Config.General.NoInput, (value) =>
        {
            Config.General.NoInput = value;
            Save(Window.WindowName, Config);

            if (value) Window.Flags |= ImGuiWindowFlags.NoInputs;
            else Window.Flags &= ~ImGuiWindowFlags.NoInputs;
        });

        ImGui.Indent();
        if (Config.General.NoInput) _ImGuiBeginDisabled(ref isDisabled);

        _ImguiCheckboxWithTooltip("No Move", "Prevents the window from being moved.", Config.General.NoMove, (value) =>
        {
            Config.General.NoMove = value;
            Save(Window.WindowName, Config);

            if (value) Window.Flags |= ImGuiWindowFlags.NoMove;
            else Window.Flags &= ~ImGuiWindowFlags.NoMove;
        });

        _ImguiCheckboxWithTooltip("No Resize", "Prevents the window from being resized.", Config.General.NoResize, (value) =>
        {
            Config.General.NoResize = value;
            Save(Window.WindowName, Config);

            if (value) Window.Flags |= ImGuiWindowFlags.NoResize;
            else Window.Flags &= ~ImGuiWindowFlags.NoResize;
        });
        _ImGuiEndDisabled(ref isDisabled);
        ImGui.Unindent();

        ImGui.Spacing();
        ImGui.Text("Other");
        ImGui.Spacing();


        _ImguiCheckboxWithTooltip("Calculate per frame", "Atualize values for all data every frame", Config.General.FrameCalc, (value) =>
        {
            Config.General.FrameCalc = value;
            Save(Window.WindowName, Config);
        });

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Reset all configurations", new Vector2(240, 24)))
        {
            Config = new Configuration();
            Init();
            Save(Window.WindowName, Config);
        }
    }
}