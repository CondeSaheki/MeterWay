using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;
using System.Linq;
using System.Threading.Tasks;

using MeterWay.Managers;
using MeterWay.Connection;

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
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();

            foreach (TEnum enumValue in enumValues[..^1])
            {
                if (ImGui.RadioButton(enumValue.ToString(), ref value, Convert.ToInt32(enumValue))) result = true;
                ImGui.SameLine();
            }
            if (enumValues.Length == 0) return result;

            TEnum lastEnumValue = enumValues[^1];
            if (ImGui.RadioButton(lastEnumValue.ToString(), ref value, Convert.ToInt32(lastEnumValue))) result = true;

            return result;
        }

        ImGui.Text("Client");
        ImGui.SameLine();

        int clientValue = (int)ConfigurationManager.Inst.Configuration.ClientType;
        if (DrawEnumRadioButtons<ClientType>(ref clientValue) && (ClientType)clientValue != ConfigurationManager.Inst.Configuration.ClientType)
        {
            ConfigurationManager.Inst.Configuration.ClientType = (ClientType)clientValue;
            ConfigurationManager.Inst.Configuration.Save();
            Plugin.ConnectionManager.Init();
        }

        ImGui.Separator();
        ImGui.Spacing();

        var status = Plugin.ConnectionManager.Status();

        ImGui.Text("Status:");
        ImGui.SameLine();
        ImGui.TextColored(status == ClientStatus.Connected ? new Vector4(0f, 1f, 0f, 1f) : new Vector4(1f, 0f, 0f, 1f), status.ToString());

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button(status == ClientStatus.Connected ? "Reconnect" : "Connect"))
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

        ImGui.SameLine();
        if (status != ClientStatus.Connected)
        {
            ImGui.BeginDisabled();
            ImGui.Button("Disconnect");
            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button("Disconnect"))
            {
                Plugin.ConnectionManager.Disconnect();
            }
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var autoConnectValue = ConfigurationManager.Inst.Configuration.AutoConnect;
        if (ImGui.Checkbox("Connect on login", ref autoConnectValue))
        {
            ConfigurationManager.Inst.Configuration.AutoConnect = autoConnectValue;
            ConfigurationManager.Inst.Configuration.Save();
        }

        ImGui.Spacing();

        if (Plugin.ConnectionManager.Client is ActWebSocket client)
        {
            var addressValue = ConfigurationManager.Inst.Configuration.Address;
            ImGui.Text("WebSocket Settings:");
            if (ImGui.InputTextWithHint("Address", "Default: 'ws://127.0.0.1:10501/ws'", ref addressValue, 64))
            {
                ConfigurationManager.Inst.Configuration.Address = addressValue;
                ConfigurationManager.Inst.Configuration.Save();
                Task.Run(() => client.ChangeUri(addressValue));
            }
        }
    }

    private void DrawAboutTab()
    {
        using var tab = ImRaii.TabItem("About");
        if (!tab) return;

        ImGui.Text("Thx everyone!");
    }
}
