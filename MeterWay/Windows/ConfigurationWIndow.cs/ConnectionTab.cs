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

public partial class ConfigWindow : Window, IDisposable
{
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

}