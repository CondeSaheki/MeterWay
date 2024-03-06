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
        ImGuiWindowFlags.NoResize | 
        ImGuiWindowFlags.NoScrollWithMouse)
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

        if (EncounterManager.LastEncounter.Active)
        {
            ImGui.Text("You can not change overlay configs when in combat");
            return;
        }

        ImGui.Text("Overlay");

        var overlayNames = OverlayWindow.Overlays.Select(x => x.Item1).ToArray();

        ImGui.PushItemWidth(160);
        var OverlayIndexValue = OverlayWindow.OverlayIndex;
        if (ImGui.Combo("Overlay type", ref OverlayIndexValue, overlayNames, OverlayWindow.Overlays.Length))
        {
            OverlayWindow.OverlayIndex = OverlayIndexValue;
            OverlayWindow.ActivateOverlay();
            ConfigurationManager.Inst.Configuration.OverlayName = OverlayWindow.Overlays[OverlayIndexValue].Item1;
            ConfigurationManager.Inst.Configuration.Save();
        }
        ImGui.PopItemWidth();

        var OverlayEnabledValue = ConfigurationManager.Inst.Configuration.OverlayEnabled;
        if (ImGui.Checkbox("Enable", ref OverlayEnabledValue))
        {
            ConfigurationManager.Inst.Configuration.OverlayEnabled = OverlayEnabledValue;
            ConfigurationManager.Inst.Configuration.Save();
            if (OverlayEnabledValue)
            {
                InterfaceManager.Inst.WindowSystem.AddWindow(OverlayWindow);
                OverlayWindow.ActivateOverlay();
            }
            else
            {
                InterfaceManager.Inst.WindowSystem.RemoveWindow(OverlayWindow);
                OverlayWindow.InactivateOverlay();
            }
        }
        if (OverlayEnabledValue == false) return;

        var OverlayClickThroughValue = ConfigurationManager.Inst.Configuration.OverlayClickThrough;
        if (ImGui.Checkbox("Click through", ref OverlayClickThroughValue))
        {
            ConfigurationManager.Inst.Configuration.OverlayClickThrough = OverlayClickThroughValue;
            ConfigurationManager.Inst.Configuration.Save();
            OverlayWindow.Flags = OverlayWindow.GetFlags();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Text("Update frequency");

        var OverlayRealtimeUpdateValue = ConfigurationManager.Inst.Configuration.OverlayRealtimeUpdate;
        if (ImGui.Checkbox("Realtime", ref OverlayRealtimeUpdateValue))
        {
            ConfigurationManager.Inst.Configuration.OverlayRealtimeUpdate = OverlayRealtimeUpdateValue;
            ConfigurationManager.Inst.Configuration.Save();
        }

        if (OverlayRealtimeUpdateValue == false)
        {
            ImGui.PushItemWidth(50);
            var OverlayIntervalUpdateValue2 = 1000 / (float)ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate.TotalMilliseconds;
            if (ImGui.DragFloat("Update per Second", ref OverlayIntervalUpdateValue2, 0.001f, 0.01f, 60f))
            {
                ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate = TimeSpan.FromMilliseconds(1000 / OverlayIntervalUpdateValue2);
                ConfigurationManager.Inst.Configuration.Save();
            }
            ImGui.SameLine();
            ImGui.Text(" | ");
            ImGui.SameLine();
            var OverlayIntervalUpdateValue = (float)ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate.TotalMilliseconds / 1000;
            if (ImGui.DragFloat("Update Interval", ref OverlayIntervalUpdateValue, 0.001f, 0.01f, 60f))
            {
                ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate = TimeSpan.FromMilliseconds(OverlayIntervalUpdateValue * 1000);
                ConfigurationManager.Inst.Configuration.Save();
            }
            ImGui.PopItemWidth();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Text("Font configs");

        ImGui.PushItemWidth(50);
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
