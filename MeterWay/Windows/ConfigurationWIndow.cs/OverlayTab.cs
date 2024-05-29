using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using System;

using MeterWay.Managers;

namespace MeterWay.Windows;

public partial class ConfigWindow : Window, IDisposable
{
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

        if (OverlayRealtimeUpdateValue) ImGui.BeginDisabled();
        
        ImGui.Indent();
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
        ImGui.Unindent();

        if (OverlayRealtimeUpdateValue) ImGui.EndDisabled();
    }
}