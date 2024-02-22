using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

using MeterWay.Managers;
using MeterWay.IINACT;

namespace MeterWay.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly IpcClient iinactIpcClient;
    private readonly OverlayWindow OverlayWindow;

    public ConfigWindow(IpcClient iinactIpcClient, OverlayWindow OverlayWindow) : base(
        "MeterWay Configurations",
        ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse) // ImGuiWindowFlags.NoResize | 
    {
        this.Size = new Vector2(400, 400);
        this.SizeCondition = ImGuiCond.Always;
        this.iinactIpcClient = iinactIpcClient;

        this.OverlayWindow = OverlayWindow;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var bar = ImRaii.TabBar("SettingsTabs");
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

        //Overlay
        var OverlayValue = ConfigurationManager.Instance.Configuration.Overlay;
        if (ImGui.Checkbox("Show Overlay", ref OverlayValue))
        {
            ConfigurationManager.Instance.Configuration.Overlay = OverlayValue;
            ConfigurationManager.Instance.Configuration.Save();
            if (OverlayValue)
            {
                InterfaceManager.Inst.WindowSystem.AddWindow(OverlayWindow);
            }
            else
            {
                InterfaceManager.Inst.WindowSystem.RemoveWindow(OverlayWindow);
            }
        }

        // OverlayClickThrough
        var OverlayClickThroughValue = ConfigurationManager.Instance.Configuration.OverlayClickThrough;
        if (ImGui.Checkbox("Click through", ref OverlayClickThroughValue))
        {
            ConfigurationManager.Instance.Configuration.OverlayClickThrough = OverlayClickThroughValue;
            ConfigurationManager.Instance.Configuration.Save();
            OverlayWindow.Flags = OverlayWindow.Gerateflags();

        }

        // OverlayBackground
        var OverlayBackgroundValue = ConfigurationManager.Instance.Configuration.OverlayBackground;
        if (ImGui.Checkbox("Background", ref OverlayBackgroundValue))
        {
            ConfigurationManager.Instance.Configuration.OverlayBackground = OverlayBackgroundValue;
            ConfigurationManager.Instance.Configuration.Save();
            OverlayWindow.Flags = OverlayWindow.Gerateflags();
        }

        if (OverlayBackgroundValue)
        {
            // OverlayBackgroundColor
            var OverlayBackgroundColorValue = ConfigurationManager.Instance.Configuration.OverlayBackgroundColor;
            ImGui.Text("background color: ");
            ImGui.SameLine();

            if (ImGui.ColorEdit4("background color", ref OverlayBackgroundColorValue,
                ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault))
            {
                ConfigurationManager.Instance.Configuration.OverlayBackgroundColor = OverlayBackgroundColorValue;
                ConfigurationManager.Instance.Configuration.Save();
            }
        }
        var OverlayFontScaleValue = ConfigurationManager.Instance.Configuration.OverlayFontScale;
        ImGui.Text("Font scale: ");
        ImGui.SameLine();
        if (ImGui.SliderFloat("##FontScale", ref OverlayFontScaleValue, 1f, 5.0f, "%.1f", ImGuiSliderFlags.None))
        {
            ConfigurationManager.Instance.Configuration.OverlayFontScale = OverlayFontScaleValue;
            ConfigurationManager.Instance.Configuration.Save();

            OverlayWindow.updatefont();
        }
    }

    private void DrawAppearenceTab()
    {
        using var tab = ImRaii.TabItem("Appearence");
        if (!tab) return;

        ImGui.Text("select overlay type");
        var OverlayTypeValue = ConfigurationManager.Instance.Configuration.OverlayType;
        if (ImGui.InputInt("Overlay Type", ref OverlayTypeValue))
        {
            if (OverlayTypeValue == 1 || OverlayTypeValue == 0 || OverlayTypeValue == 2)
            {
                ConfigurationManager.Instance.Configuration.OverlayType = OverlayTypeValue;
                ConfigurationManager.Instance.Configuration.Save();
            }
        }
    }

    private void DrawIINACTTab()
    {
        using var tab = ImRaii.TabItem("IINACT");
        if (!tab) return;

        var status = iinactIpcClient.Status();

        ImGui.Text("State: ");
        ImGui.SameLine();
        ImGui.Text(status ? "Connected" : "Disconnected");

        ImGui.Separator();
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
        ImGui.EndDisabled();
    }

    private void DrawAboutTab()
    {
        using var tab = ImRaii.TabItem("About");
        if (!tab) return;

        ImGui.Text("WIP");
    }
}
