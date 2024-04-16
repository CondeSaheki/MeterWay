using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using MeterWay.Managers;
using MeterWay.Utils;

namespace MeterWay.Overlay;

[Serializable]
public struct OverlayWindowSpecs()
{
    public string Name = string.Empty;
    public uint Id = 0;
    public string Comment = string.Empty;
    public bool Enabled = false;
}

public class OverlayWindow : Window, IDisposable
{
    private IOverlay? Overlay { get; set; }

    public Type Type { get; private set; }
    public string Name { get; private set; }
    public bool HasConfigs { get; private set; }
    public uint Id { get; private set; }
    public string Comment { get; set; }
    public static readonly ImGuiWindowFlags defaultflags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground;
    public bool Enabled => Overlay != null;

    public OverlayWindow(Type overlay, uint id) : base($"{id}")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(80, 45),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        IsOpen = true;
        RespectCloseHotkey = false;
        Flags = defaultflags;
        Type = overlay;
        Name = (string)overlay.GetProperty("Name")?.GetValue(null)!;
        HasConfigs = overlay.GetInterfaces().Contains(typeof(IOverlayTab));
        Id = id;
        Comment = $"{Name}{Id}";
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
            Dalamud.Log.Error($"\'{Name}\' overlay \'{Id}\' trow an error on \'Draw\' and got inactivated:\n{ex}");
            Disable();
        }
    }

    public void DrawConfig()
    {
        if (Overlay == null) return;
        try
        {
            ((IOverlayTab)Overlay!).DrawConfig();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"\'{Name}\' overlay \'{Id}\' trow an error on \'DrawTab\' and got inactivated:\n{ex}");
            Disable();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Disable();
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

    public void Enable()
    {
        if (Overlay != null) return;
        EncounterManager.Inst.ClientsNotifier.OnDataUpdate += OnDataUpdate; ;
        Overlay?.Dispose();
        try
        {
            Overlay = (IOverlay?)Activator.CreateInstance(Type, [this]);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow \'{Id}\' trow an error creating \'{Name}\' overlay instance:\n{ex}");
            Disable();
        }
    }

    public void Disable()
    {
        if (Overlay == null) return;
        EncounterManager.Inst.ClientsNotifier.OnDataUpdate -= OnDataUpdate;
        Overlay?.Dispose();
        Overlay = null;
    }
    
    public void Remove() => Overlay?.Remove();
    
    private void OnDataUpdate(object? _, EventArgs __)
    {
        if (Overlay == null) return;
        try
        {
            Overlay?.DataUpdate();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"\'{Name}\' overlay \'{Id}\' trow an error on \'DataUpdate\' and got inactivated:\n{ex}");
            Disable();
        }
    }

    public OverlayWindowSpecs GetSpecs()
    {
        return new OverlayWindowSpecs
        {
            Name = Name,
            Id = Id,
            Comment = Comment,
            Enabled = Enabled,
        };
    }
}