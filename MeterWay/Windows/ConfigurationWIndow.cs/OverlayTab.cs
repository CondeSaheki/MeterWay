using System;
using System.Linq;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

using MeterWay.Managers;
using MeterWay.Utils;
using MeterWay.Overlay;

namespace MeterWay.Windows;

public partial class ConfigWindow : Window, IDisposable
{
    private int OverlayIndexValue = 0;

    private void DrawOverlayTab()
    {
        using var tab = ImRaii.TabItem("Overlays");
        if (!tab) return;

        ImGui.Spacing();

        if (EncounterManager.LastEncounter != null && EncounterManager.LastEncounter.Active)
        {
            ImGui.Text("You can not change overlay configs when in combat");
            return;
        }

        DrawOverlayManager();

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

    public void DrawOverlayManager()
    {
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var size = ImGui.GetContentRegionAvail();
        size.Y -= 140;
        ImGui.BeginChild("##Overlays", size, border: false, ImGuiWindowFlags.AlwaysVerticalScrollbar);

        if (Plugin.OverlayManager.Windows.Count == 0)
        {
            ImGui.Text("No overlays, click the \'Add\' button to start");
        }

        foreach (var window in Plugin.OverlayManager.Windows)
        {
            bool enableValue = window.Value.IsEnabled();
            if (ImGui.Checkbox($"##Checkbox{window.Key}", ref enableValue))
            {
                if (!enableValue) window.Value.Disable();
                else window.Value.Enable();

                ConfigurationManager.Inst.Configuration.Overlays = Plugin.OverlayManager.Windows.Values.Select(OverlayManager.GetSpec).ToList();
                ConfigurationManager.Inst.Configuration.Save();
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(250);
            var commentValue = window.Value.Comment;
            const int commentSize = 16;
            if (ImGui.InputText($"##comment{window.Key}", ref commentValue, commentSize, ImGuiInputTextFlags.AutoSelectAll))
            {
                window.Value.Comment = commentValue.Length > commentSize ? commentValue[..commentSize] : commentValue;

                ConfigurationManager.Inst.Configuration.Overlays = Plugin.OverlayManager.Windows.Values.Select(OverlayManager.GetSpec).ToList();
                ConfigurationManager.Inst.Configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.SameLine();
            if (ImGui.Button($"remove##{window.Key}"))
            {
                Plugin.OverlayManager.Remove(window.Key);

                ConfigurationManager.Inst.Configuration.Overlays = Plugin.OverlayManager.Windows.Values.Select(OverlayManager.GetSpec).ToList();
                ConfigurationManager.Inst.Configuration.Save();
                break;
            }

            if (!window.Value.IsEnabled()) ImGui.BeginDisabled();

            if (window.Value.HasConfigurationWindow)
            {
                ImGui.SameLine();
                if (ImGui.Button($"Config##{window.Key}"))
                {
                    PopupWindow overlayConfig = new($"{window.Value.Info.Name} Configuration##{window.Key}",
                        window.Value.DrawConfiguration, false, window.Value.ConfigurationWindowSize);
                }
            }

            if (!window.Value.IsEnabled()) ImGui.EndDisabled();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }
        ImGui.EndChild();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Add overlay"))
        {
            var addOverlayPopup = new PopupWindow("Add overlay",
            (TaskSource) =>
            {
                var infos = Plugin.OverlayManager.Types.Select(OverlayManager.GetInfo).ToArray();
                var names = infos.Select(info => info.Name).ToArray();

                ImGui.PushItemWidth(160);
                ImGui.Combo("Select Overlay", ref OverlayIndexValue, names, names.Length);
                ImGui.PopItemWidth();

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Text($"Description");
                ImGui.TextWrapped($"{infos[OverlayIndexValue].Description ?? "Empty"}");
                ImGui.Text($"Author");
                ImGui.TextWrapped($"{infos[OverlayIndexValue].Author ?? "Empty"}");

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                if (ImGui.Button("Confirm"))
                {
                    Plugin.OverlayManager.Add(Plugin.OverlayManager.Types[OverlayIndexValue]);
                    ConfigurationManager.Inst.Configuration.Overlays = Plugin.OverlayManager.Windows.Values.Select(OverlayManager.GetSpec).ToList();
                    ConfigurationManager.Inst.Configuration.Save();
                    TaskSource.SetResult();
                }
            }, true, new(533, 300));
        }
    }
}