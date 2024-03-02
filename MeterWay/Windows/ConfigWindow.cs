using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility.Raii;

using MeterWay.Managers;
using MeterWay.Ipc;

namespace MeterWay.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly IINACTClient iinactIpcClient;
    private readonly OverlayWindow OverlayWindow;

    public ConfigWindow(IINACTClient iinactIpcClient, OverlayWindow OverlayWindow) : base(
        "MeterWay Configurations",
        ImGuiWindowFlags.NoCollapse |
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse) // ImGuiWindowFlags.NoResize | 
    {
        Size = new Vector2(400, 300);
        SizeCondition = ImGuiCond.Always;

        this.iinactIpcClient = iinactIpcClient;
        this.OverlayWindow = OverlayWindow;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var bar = ImRaii.TabBar("Settings Tabs");
        if (!bar) return;

        DrawGeneralTab();
        DrawOverlayTab();
        DrawAppearenceTab();
        DrawIINACTTab();
        DrawAboutTab();
    }

    private void DrawGeneralTab()
    {
        using var tab = ImRaii.TabItem("General");
        if (!tab) return;

        ImGui.Text("WIP");
    }

    private void DrawOverlayTab()
    {
        using var tab = ImRaii.TabItem("Overlay");
        if (!tab) return;

        ImGui.Text("Overlay");

        ImGui.PushItemWidth(160);
        string[] items = OverlayWindow.Overlays.Select(x => x.Name).ToArray();
        var OverlayTypeValue = ConfigurationManager.Inst.Configuration.OverlayType;
        if (ImGui.Combo("Overlay type", ref OverlayTypeValue, items, items.Length))
        {
            if (OverlayTypeValue >= 0 && OverlayTypeValue <= OverlayWindow.Overlays.Count)
            {
                ConfigurationManager.Inst.Configuration.OverlayType = OverlayTypeValue;
                ConfigurationManager.Inst.Configuration.Save();

            }
        }
        ImGui.PopItemWidth();

        var OverlayValue = ConfigurationManager.Inst.Configuration.Overlay;
        if (ImGui.Checkbox("Enable", ref OverlayValue))
        {
            ConfigurationManager.Inst.Configuration.Overlay = OverlayValue;
            ConfigurationManager.Inst.Configuration.Save();
            if (OverlayValue)
            {
                InterfaceManager.Inst.WindowSystem.AddWindow(OverlayWindow);
            }
            else
            {
                InterfaceManager.Inst.WindowSystem.RemoveWindow(OverlayWindow);
            }
        }

        var OverlayClickThroughValue = ConfigurationManager.Inst.Configuration.OverlayClickThrough;
        if (ImGui.Checkbox("Click through", ref OverlayClickThroughValue))
        {
            ConfigurationManager.Inst.Configuration.OverlayClickThrough = OverlayClickThroughValue;
            ConfigurationManager.Inst.Configuration.Save();
            OverlayWindow.Flags = OverlayWindow.GetFlags();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Text("Font configs");

        ImGui.PushItemWidth(90);
        var OverlayFontScaleValue = ConfigurationManager.Inst.Configuration.OverlayFontScale;
        if (ImGui.DragFloat("Scale", ref OverlayFontScaleValue, 0.01f, 1f, 5f))
        {
            ConfigurationManager.Inst.Configuration.OverlayFontScale = OverlayFontScaleValue;
            ConfigurationManager.Inst.Configuration.Save();
        }
        ImGui.PopItemWidth();
    }

    private void DrawAppearenceTab()
    {
        using var tab = ImRaii.TabItem("Appearence");
        if (!tab) return;

        ImGui.Text("Configs just for DEMO");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Text("Background");

        var OverlayBackgroundValue = ConfigurationManager.Inst.Configuration.OverlayBackground;
        if (ImGui.Checkbox("Enable", ref OverlayBackgroundValue))
        {
            ConfigurationManager.Inst.Configuration.OverlayBackground = OverlayBackgroundValue;
            ConfigurationManager.Inst.Configuration.Save();
            OverlayWindow.Flags = OverlayWindow.GetFlags();
        }

        var OverlayBackgroundColorValue = ConfigurationManager.Inst.Configuration.OverlayBackgroundColor;
        if (ImGui.ColorEdit4("Color", ref OverlayBackgroundColorValue,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
        {
            ConfigurationManager.Inst.Configuration.OverlayBackgroundColor = OverlayBackgroundColorValue;
            ConfigurationManager.Inst.Configuration.Save();
        }
    }

    private void DrawIINACTTab()
    {
        using var tab = ImRaii.TabItem("IINACT");
        if (!tab) return;

        var status = iinactIpcClient.Status();

        ImGui.Text($"State: ");
        ImGui.SameLine();
        ImGui.TextColored(status ? new Vector4(0, 255, 0, 255) : new Vector4(255, 0, 0, 255), status ? "Connected" : "Disconnected");

        ImGui.Spacing();
        ImGui.Separator();
        
        {
            if (ImGui.Button(status ? "Restart" : "Start"))
            {
                if (status)
                {
                    iinactIpcClient.Reconnect();
                }
                else
                {
                    iinactIpcClient.Connect();
                }
            }

            if (!status) ImGui.BeginDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Stop"))
            {
                iinactIpcClient.Disconnect();
            }
            if (!status) ImGui.EndDisabled();
        }
    }

    private void DrawAboutTab()
    {
        using var tab = ImRaii.TabItem("About");
        if (!tab) return;

        ImGui.Text("Thx everyone!");
    }
}
