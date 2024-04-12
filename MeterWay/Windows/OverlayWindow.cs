using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

using MeterWay.Overlay;
using MeterWay.Managers;
using MeterWay.Utils;

namespace MeterWay.Windows;

public class OverlayWindow : Window, IDisposable
{
    public (string, Type)[] Overlays { get; private init; }

    public int OverlayIndex { get; set; }
    public IOverlay? Overlay { get; private set; }

    public static readonly ImGuiWindowFlags defaultflags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground;

    public OverlayWindow(Type[] overlays) : base("OverlayWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(80, 45),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        IsOpen = ConfigurationManager.Inst.Configuration.OverlayEnabled;
        RespectCloseHotkey = false;
        Flags = GetFlags();

        EncounterManager.Inst.ClientsNotifier.OnDataUpdate += OnDataUpdate;

        #if DEBUG
            ValidadeOverlays<IOverlay>(overlays);
        #endif

        Overlays = overlays.Select(type => ((string)type.GetProperty("Name")?.GetValue(null)!, type)).ToArray();

        var index = Array.FindIndex(Overlays, x => x.Item1 == ConfigurationManager.Inst.Configuration.OverlayName);
        if (index == -1)
        {
            index = 0;
            Dalamud.Log.Warning($"Reseting overlay type to default, Overlay with the name {ConfigurationManager.Inst.Configuration.OverlayName} was not found!");
            ConfigurationManager.Inst.Configuration.OverlayName = Overlays[0].Item1;
            ConfigurationManager.Inst.Configuration.Save();
        }
        OverlayIndex = index;

        ActivateOverlay();
    }

    public override void Draw()
    {
        if (Overlay == null) return;
        try
        {
            Overlay.Draw();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"\'{Overlays[OverlayIndex].Item1}\' overlay trow an error on \'Draw\' and got inactivated:\n{ex}");
            InactivateOverlay();
        }
    }

    public bool HasOverlayTab => Overlay != null && Overlay.GetType().GetInterfaces().Contains(typeof(IOverlayTab));

    public void DrawTab()
    {
        try
        {
            ((IOverlayTab)Overlay!).DrawTab();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"\'{Overlays[OverlayIndex].Item1}\' overlay trow an error on \'DrawTab\' and got inactivated:\n{ex}");
            InactivateOverlay();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        EncounterManager.Inst.ClientsNotifier.OnDataUpdate -= OnDataUpdate;
        Overlay?.Dispose();
    }

    // TODO improve way to change window atributtes
    public static ImGuiWindowFlags GetFlags()
    {
        return defaultflags; // | (ConfigurationManager.Inst.Configuration.OverlayClickThrough ? ImGuiWindowFlags.NoInputs : ImGuiWindowFlags.None);
    }

    public static Canvas GetCanvas()
    {
        Vector2 Min = ImGui.GetWindowContentRegionMin();
        Vector2 Max = ImGui.GetWindowContentRegionMax();

        Min.X += ImGui.GetWindowPos().X;
        Min.Y += ImGui.GetWindowPos().Y;
        Max.X += ImGui.GetWindowPos().X;
        Max.Y += ImGui.GetWindowPos().Y;
        return new Canvas((Min, Max));
    }

    public void ActivateOverlay()
    {
        Overlay?.Dispose();
        try
        {
            Overlay = (IOverlay?)Activator.CreateInstance(Overlays[OverlayIndex].Item2, [this]);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Error creating ovelay instance:\n{ex}");
            Overlay?.Dispose();
            Overlay = null;
            return;
        }
    }

    public void InactivateOverlay()
    {
        Overlay?.Dispose();
        Overlay = null;
    }

    private void OnDataUpdate(object? _, EventArgs __)
    {
        if (Overlay == null) return;
        try
        {
            Overlay?.DataUpdate();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"\'{Overlays[OverlayIndex].Item1}\' overlay trow an error on \'DataUpdate\' and got inactivated:\n{ex}");
            InactivateOverlay();
        }
    }

    private static void ValidadeOverlays<TInterface>(IEnumerable<Type> types) where TInterface : class
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