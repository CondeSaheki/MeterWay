using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using MeterWay.Managers;
using MeterWay.Utils;

namespace MeterWay.Overlay;

public class OverlayManager : IDisposable
{
    private WindowSystem WindowSystem { get; init; } = new();
    private HashSet<OverlayWindow> Overlays { get; set; } = [];
    private OverlayInfo[] OverlayInfos { get; init; }
    private int OverlayIndexValue = 0;
    private uint UniqueId;

    private class OverlayInfo
    {
        public string Name { get; private init; }
        public string? Autor { get; private init; }
        public string? Description { get; private init; }
        public Type Type { get; private init; }

        public OverlayInfo(Type overlay)
        {
            Name = ((string?)overlay.GetProperty("Name")?.GetValue(null)) ?? "Empty";
            Autor = (string?)overlay.GetProperty("Autor")?.GetValue(null);
            Description = (string?)overlay.GetProperty("Description")?.GetValue(null);
            Type = overlay;
        }
    }

    public OverlayManager(Type[] overlays)
    {
        #if DEBUG
            ValidateOverlays<IOverlay>(overlays);
        #endif

        OverlayInfos = overlays.Select(overlay => new OverlayInfo(overlay)).ToArray();

        // load config
        foreach (var overlay in ConfigurationManager.Inst.Configuration.Overlays)
        {
            var type = ConfigGetType(OverlayInfos, overlay.Name);
            if (type == null)
            {
                Dalamud.Log.Warning($"{overlay.Name}{overlay.Id} could not be reatrived an got ignored.");
                continue;
            }

            var obj = new OverlayWindow(type, overlay.Id)
            {
                Comment = overlay.Comment,
            };
            if (overlay.Enabled) obj.Enable();
            WindowSystem.AddWindow(obj);
            Overlays.Add(obj);
        }

        UniqueId = Overlays.Count != 0 ? Overlays.Max(overlay => overlay.Id) + 1 : 1;

        Dalamud.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
    }

    private void Add(Type overlayType)
    {
        try
        {
            uint id = UniqueId++;
            OverlayWindow overlay = new(overlayType, id);
            Overlays.Add(overlay);
            WindowSystem.AddWindow(overlay);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Error adding overlay window {overlayType.Name}:\n{ex}");
            Dispose();
        }
    }

    private void Remove(uint id)
    {
        var overlay = Overlays.FirstOrDefault(x => x.Id == id);
        if (overlay == null)
        {
            Dalamud.Log.Error($"Could not find overlay window {id}");
            return;
        }
        try
        {
            WindowSystem.RemoveWindow(overlay);
            Overlays.Remove(overlay);
            overlay.Remove();
            overlay.Dispose();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Error removing overlay window {id}:\n{ex}");
            Dispose();
        }
    }

    public void Draw()
    {
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var size = ImGui.GetContentRegionAvail();
        size.Y -= 140;
        ImGui.BeginChild("##Overlays", size, border: false, ImGuiWindowFlags.AlwaysVerticalScrollbar);

        if (Overlays.Count == 0)
        {
            ImGui.Text("No overlays, click the \'Add\' button to start");
        }

        foreach (var overlay in Overlays)
        {
            bool enableValue = overlay.Enabled;
            if (ImGui.Checkbox($"##Checkbox{overlay.Id}", ref enableValue))
            {
                if (!enableValue) overlay.Disable();
                else overlay.Enable();

                ConfigurationManager.Inst.Configuration.Overlays = Overlays.Select(x => x.GetSpecs()).ToList();
                ConfigurationManager.Inst.Configuration.Save();
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(160);
            var commentValue = overlay.Comment;
            const int commentSize = 16;
            if (ImGui.InputText($"##comment{overlay.Id}", ref commentValue, commentSize, ImGuiInputTextFlags.AutoSelectAll))
            {
                overlay.Comment = commentValue.Length > commentSize ? commentValue[..commentSize] : commentValue;

                ConfigurationManager.Inst.Configuration.Overlays = Overlays.Select(x => x.GetSpecs()).ToList();
                ConfigurationManager.Inst.Configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.SameLine();
            if (ImGui.Button($"remove##{overlay.Id}"))
            {
                Remove(overlay.Id);

                ConfigurationManager.Inst.Configuration.Overlays = Overlays.Select(x => x.GetSpecs()).ToList();
                ConfigurationManager.Inst.Configuration.Save();
                break;
            }

            if (!overlay.Enabled) ImGui.BeginDisabled();

            if (overlay.HasConfigs)
            {
                ImGui.SameLine();
                if (ImGui.Button($"Config##{overlay.Id}"))
                {
                    Helpers.PopupWindow overlayConfig = new($"{overlay.Name} Configurations##{overlay.Id}", overlay.DrawConfig);
                }
            }

            if (!overlay.Enabled) ImGui.EndDisabled();

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
            var addOverlayPopup = new Helpers.PopupWindow("Add overlay",
            (TaskSource) =>
            {
                ImGui.PushItemWidth(160);
                var overlayNames = OverlayInfos.Select(x => x.Name).ToArray();
                ImGui.Combo("Overlay type", ref OverlayIndexValue, overlayNames, overlayNames.Length);
                ImGui.PopItemWidth();

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Text($"Description");
                ImGui.TextWrapped($"{OverlayInfos[OverlayIndexValue].Description ?? "Empty"}");
                ImGui.Text($"Autor");
                ImGui.TextWrapped($"{OverlayInfos[OverlayIndexValue].Autor ?? "Empty"}");

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                if (ImGui.Button("Confirm"))
                {
                    var overlayInfo = Array.Find(OverlayInfos, x => x.Name == overlayNames[OverlayIndexValue]);
                    if (overlayInfo != null)
                    {
                        Add(overlayInfo.Type);
                        ConfigurationManager.Inst.Configuration.Overlays = Overlays.Select(x => x.GetSpecs()).ToList();
                        ConfigurationManager.Inst.Configuration.Save();
                        TaskSource.SetResult();
                    }
                    else
                    {
                        Dalamud.Log.Warning($"Overlay info for {overlayNames[OverlayIndexValue]} not found.");
                    }
                }
            });
        }
    }

    public void Dispose()
    {
        if (WindowSystem != null)
        {
            WindowSystem.RemoveAllWindows();
            Dalamud.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        }
        foreach (var overlay in Overlays) overlay?.Dispose();
        Overlays?.Clear();
    }

    private static Type? ConfigGetType(OverlayInfo[] overlayInfos, string name)
    {
        var value = overlayInfos.FirstOrDefault(x => x.Name == name);
        if (value == default)
        {
            Dalamud.Log.Warning($"Overlay type with name \'{name}\' not found.");
            return null;
        }
        return value.Type;
    }

    private static void ValidateOverlays<TInterface>(IEnumerable<Type> types) where TInterface : class
    {
        if (!types.Any()) throw new Exception($"Overlays can not be empty!");
        if (types.Distinct().Count() != types.Count()) throw new Exception($"Overlays must have unique elements!");
        List<string> values = [];
        foreach (var type in types)
        {
            if (!typeof(TInterface).IsAssignableFrom(type)) throw new Exception($"{type.Name} do not implement {typeof(TInterface)}!");
            var propriety = type.GetProperty("Name") ?? throw new Exception($"{type.Name} do not implement propriety \'Name\'!");
            if (propriety.PropertyType != typeof(string)) throw new Exception($"{type.Name} propriety \'Name\' must be a string!");
            var value = (string)propriety.GetValue(null)!;
            if (value == string.Empty) throw new Exception($"{type.Name} propriety \'Name\' can not be empty!");
            values.Add(value);
        }
        if (values.Distinct().Count() != values.Count) throw new Exception($"Overlays must have elements with unique names!");
    }
}