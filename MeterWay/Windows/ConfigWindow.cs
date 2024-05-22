using System;
using System.Numerics;
using ImGuiNET;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility.Raii;

using MeterWay.Managers;
using MeterWay.Connection;
using System.Reflection;

namespace MeterWay.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    public ConfigWindow(Plugin plugin) : base("MeterWay Configurations", ImGuiWindowFlags.NoCollapse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(430, 255),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
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
        //DrawOverlayConfigTab();
        //DrawAppearenceTab();
        DrawConnectionTab();
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
        using var tab = ImRaii.TabItem("Overlays");
        if (!tab) return;

        ImGui.Spacing();

        if (EncounterManager.LastEncounter.Active)
        {
            ImGui.Text("You can not change overlay configs when in combat");
            return;
        }

        Plugin.OverlayManager.Draw();

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

        if (!OverlayRealtimeUpdateValue)
        {
            ImGui.PushItemWidth(50);
            var OverlayIntervalUpdateValue2 = 1000 / (float)ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate.TotalMilliseconds;
            if (ImGui.DragFloat("Update per Second", ref OverlayIntervalUpdateValue2, 0.001f, 0.01f, 60f))
            {
                ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate = TimeSpan.FromMilliseconds(1000 / OverlayIntervalUpdateValue2);
                ConfigurationManager.Inst.Configuration.Save();
                EncounterManager.Inst.ClientsNotifier.ChangeNotificationInterval(ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate);
            }
            ImGui.SameLine();
            ImGui.Text(" | ");
            ImGui.SameLine();
            var OverlayIntervalUpdateValue = (float)ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate.TotalMilliseconds / 1000;
            if (ImGui.DragFloat("Update Interval", ref OverlayIntervalUpdateValue, 0.001f, 0.01f, 60f))
            {
                ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate = TimeSpan.FromMilliseconds(OverlayIntervalUpdateValue * 1000);
                ConfigurationManager.Inst.Configuration.Save();
                EncounterManager.Inst.ClientsNotifier.ChangeNotificationInterval(ConfigurationManager.Inst.Configuration.OverlayIntervalUpdate);
            }
            ImGui.PopItemWidth();
        }
    }

    private void DrawAppearenceTab()
    {
        using var tab = ImRaii.TabItem("Appearence");
        if (!tab) return;

        ImGui.Spacing();
    }

    private void DrawConnectionTab()
    {
        using var tab = ImRaii.TabItem("Connection");
        if (!tab) return;

        static bool DrawEnumRadioButtons<TEnum>(ref int value) where TEnum : Enum
        {
            var result = false;
            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                if (ImGui.RadioButton(enumValue.ToString(), ref value, Convert.ToInt32(enumValue))) result = true;
                ImGui.SameLine();
            }
            return result;
        }

        int clientValue = (int)ConfigurationManager.Inst.Configuration.ClientType;
        if (DrawEnumRadioButtons<ClientType>(ref clientValue))
        {
            ConfigurationManager.Inst.Configuration.ClientType = (ClientType)clientValue;
            ConfigurationManager.Inst.Configuration.Save();
        }

        var status = Plugin.ConnectionManager.Status();

        ImGui.Text($"State: ");
        ImGui.SameLine();
        
        ImGui.TextColored(status == ClientStatus.Connected ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f), status.ToString());

        ImGui.Spacing();
        ImGui.Separator();

        if (ImGui.Button(status == ClientStatus.Connected ? "Restart" : "Start"))
        {
            if (status == ClientStatus.Connected)
            {
                Plugin.ConnectionManager.Reconnect();
            }
            else
            {
                Plugin.ConnectionManager.Connect();
            }
        }

        if (status != ClientStatus.Connected) ImGui.BeginDisabled();

        ImGui.SameLine();
        if (ImGui.Button("Stop"))
        {
            Plugin.ConnectionManager.Disconnect();
        }
        if (status != ClientStatus.Connected) ImGui.EndDisabled();

        // if (Plugin.ConnectionManager.Client is ActWebSocket client)
        // {
        //     var addressValue = ConfigurationManager.Inst.Configuration.Address;
        //     if (ImGui.InputTextWithHint("Address", "Default: 'ws://127.0.0.1:10501/ws'", ref addressValue, 64))
        //     {
        //         client.Uri = new(addressValue);
        //         ConfigurationManager.Inst.Configuration.Address = addressValue;
        //         ConfigurationManager.Inst.Configuration.Save();
        //     }
        // }
    }

    private void DrawAboutTab()
    {
        using var tab = ImRaii.TabItem("About");
        if (!tab) return;

        ImGui.Text("Thx everyone!");
    }
}
