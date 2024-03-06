using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

using MeterWay.Overlays;
using MeterWay.Managers;
using MeterWay.Utils;

namespace MeterWay.Windows;

public class OverlayWindow : Window, IDisposable
{
    public (string, Type)[] Overlays { get; init; }
    public int OverlayIndex { get; set; }

    private IMeterwayOverlay? Overlay { get; set; }
    private uint? OverlayClientId { get; set; }

    private static readonly ImGuiWindowFlags defaultflags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground;

    public OverlayWindow(Type[] overlays) : base("OverlayWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(80, 45),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        IsOpen = true;
        RespectCloseHotkey = false;
        Flags = GetFlags();

        #if DEBUG
            ValidadeOverlays<IMeterwayOverlay>(overlays);
        #endif

        Overlays = overlays.Select(type => ((string)type.GetProperty("Name")?.GetValue(null)!, type)).ToArray();

        var index = Array.FindIndex(Overlays, x => x.Item1 == ConfigurationManager.Inst.Configuration.OverlayName);
        if (index == -1)
        {
            index = 0;
            InterfaceManager.Inst.PluginLog.Warning($"Reseting overlay type to default, Overlay with the name {ConfigurationManager.Inst.Configuration.OverlayName} was not found!");
            ConfigurationManager.Inst.Configuration.OverlayName = Overlays[0].Item1;
            ConfigurationManager.Inst.Configuration.Save();
        }
        OverlayIndex = index;

        ActivateOverlay();
    }

    public override void Draw()
    {
        ImGui.SetWindowFontScale(ConfigurationManager.Inst.Configuration.OverlayFontScale);
        Overlay?.Draw();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        UnSubscribeOverlay();
        Overlay?.Dispose();
    }

    public void ActivateOverlay()
    {
        UnSubscribeOverlay();
        Overlay?.Dispose();
        Overlay = (IMeterwayOverlay?)Activator.CreateInstance(Overlays[OverlayIndex].Item2);
        SubscribeOverlay();
    }

    public void InactivateOverlay()
    {
        UnSubscribeOverlay();
        if(Overlay != null) Overlay.Dispose();
        Overlay = null;
    }

    private void UnSubscribeOverlay()
    {
        if (Overlay == null) return;
        var elem = EncounterManager.Clients.FindIndex(x => x.Key.Equals(OverlayClientId));
        if (elem == -1) throw new InvalidOperationException($"Could not unregister Overlay {Overlay}");
        EncounterManager.Clients.RemoveAt(elem);
        OverlayClientId = null;
    }

    private void SubscribeOverlay()
    {
        if (Overlay == null) return;
        OverlayClientId = Helpers.CreateId();
        EncounterManager.Clients.Add(new KeyValuePair<uint, Action>((uint)OverlayClientId, Overlay.DataProcess)); // register client
    }

    public static ImGuiWindowFlags GetFlags()
    {
        return defaultflags | (ConfigurationManager.Inst.Configuration.OverlayClickThrough ? ImGuiWindowFlags.NoInputs : ImGuiWindowFlags.None);
    }

    private static void ValidadeOverlays<TInterface>(IEnumerable<Type> types) where TInterface : class
    {
        if (!types.Any()) throw new InvalidOperationException($"Overlays can not be empty!");
        if(types.Distinct().Count() != types.Count()) throw new Exception($"Overlays must have unique elements");
        foreach (var type in types)
        {
            if (!typeof(TInterface).IsAssignableFrom(type)) throw new Exception($"{type.Name} does not implement {typeof(TInterface)}");
            var propriety = type.GetProperty("Name");
            if (propriety == null || propriety.PropertyType != typeof(string)) throw new Exception($"{type.Name} propriety \'Name\' can not be empty!");
        }
    }
}