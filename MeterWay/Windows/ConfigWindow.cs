﻿using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility.Raii;

using MeterWay.Managers;

namespace MeterWay.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    public ConfigWindow(Plugin plugin) : base("MeterWay Configurations", ImGuiWindowFlags.NoCollapse)
    {
        //Size = new Vector2(400, 300);
        //SizeCondition = ImGuiCond.Always;

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var bar = ImRaii.TabBar("Settings Tabs");
        if (!bar) return;

        //DrawGeneralTab();
        DrawOverlayTab();
        DrawOverlayConfigTab();
        //DrawAppearenceTab();
        DrawIINACTTab();
        //DrawAboutTab();
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

        ImGui.Spacing();

        if (EncounterManager.LastEncounter.Active)
        {
            ImGui.Text("You can not change overlay configs when in combat");
            return;
        }

        ImGui.Text("Overlay");
        ImGui.Spacing();
        var OverlayEnabledValue = ConfigurationManager.Inst.Configuration.OverlayEnabled;
        if (ImGui.Checkbox("Enable", ref OverlayEnabledValue))
        {
            ConfigurationManager.Inst.Configuration.OverlayEnabled = OverlayEnabledValue;
            ConfigurationManager.Inst.Configuration.Save();
            if (OverlayEnabledValue)
            {
                Plugin.OverlayWindow.IsOpen = true;
                Plugin.OverlayWindow.ActivateOverlay();
            }
            else
            {
                Plugin.OverlayWindow.IsOpen = false;
                Plugin.OverlayWindow.InactivateOverlay();
            }
        }
        ImGui.SameLine();
        var overlayNames = Plugin.OverlayWindow.Overlays.Select(x => x.Item1).ToArray();
        var OverlayIndexValue = Plugin.OverlayWindow.OverlayIndex;
        ImGui.PushItemWidth(160);
        if (ImGui.Combo("Overlay type", ref OverlayIndexValue, overlayNames, Plugin.OverlayWindow.Overlays.Length))
        {
            Plugin.OverlayWindow.OverlayIndex = OverlayIndexValue;
            Plugin.OverlayWindow.ActivateOverlay();
            ConfigurationManager.Inst.Configuration.OverlayName = Plugin.OverlayWindow.Overlays[OverlayIndexValue].Item1;
            ConfigurationManager.Inst.Configuration.Save();
        }
        ImGui.PopItemWidth();

        if (OverlayEnabledValue == false) return;

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Text("Update frequency");
        ImGui.Spacing();

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
    }

    private void DrawOverlayConfigTab()
    {
        if(!Plugin.OverlayWindow.HasOverlayTab) return;

        var overlayName = Plugin.OverlayWindow.Overlays[Plugin.OverlayWindow.OverlayIndex].Item1;
        using var tab = ImRaii.TabItem($"{overlayName} Config");
        if (!tab) return;
        
        Plugin.OverlayWindow.DrawTab();
    }

    private void DrawAppearenceTab()
    {
        using var tab = ImRaii.TabItem("Appearence");
        if (!tab) return;

        ImGui.Spacing();
    }

    private void DrawIINACTTab()
    {
        using var tab = ImRaii.TabItem("IINACT");
        if (!tab) return;

        var status = Plugin.IinactIpcClient.Status();

        ImGui.Text($"State: ");
        ImGui.SameLine();
        ImGui.TextColored(status ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f), status ? "Connected" : "Disconnected");

        ImGui.Spacing();
        ImGui.Separator();

        {
            if (ImGui.Button(status ? "Restart" : "Start"))
            {
                if (status)
                {
                    Plugin.IinactIpcClient.Reconnect();
                }
                else
                {
                    Plugin.IinactIpcClient.Connect();
                }
            }

            if (!status) ImGui.BeginDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Stop"))
            {
                Plugin.IinactIpcClient.Disconnect();
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
