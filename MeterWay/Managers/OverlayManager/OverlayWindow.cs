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

public class OverlayWindow : Window, IOverlayWindow
{
    private IOverlay? Overlay { get; set; }

    public Type Type { get; private set; }
    public string Name { get; private set; }
    public bool HasConfigs { get; private set; }
    public uint Id { get; private set; }
    public string NameId { get; private set; }
    public string Comment { get; set; }
    public Vector2 CurrentSize { get; private set; } = new(0, 0);
    public Vector2 CurrentPos { get; private set; } = new(0, 0);

    public bool Enabled => Overlay != null;

    public OverlayWindow(Type overlay, uint id) : base($"{id}")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(0, 0),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        IsOpen = false;
        RespectCloseHotkey = false;
        Flags = ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoSavedSettings;

        Type = overlay;
        Name = (string)overlay.GetProperty("Name")?.GetValue(null)!;
        HasConfigs = overlay.GetInterfaces().Contains(typeof(IOverlayConfig));
        Id = id;
        NameId = $"{Name}{Id}";
        Comment = $"{Name}{Id}";
    }

    public override void Draw()
    {
        CurrentSize = ImGui.GetWindowSize();
        CurrentPos = ImGui.GetWindowPos();
        if (Overlay == null) return;
        try
        {
            Overlay.Draw();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow Draw, overlay \'{Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Name} overlay {Id}\' Had an error, contact the autor for feedback");
            Disable();
        }
    }

    public void DrawConfig()
    {
        if (Overlay == null) return;
        try
        {
            ((IOverlayConfig)Overlay!).DrawConfig();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow DrawConfig, overlay \'{Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Name} overlay {Id}\' Had an error, contact the autor for feedback");
            Disable();
        }
    }

    public void Dispose()
    {
        Disable();
    }

    public void SetSize(Vector2 size) => ImGui.SetWindowSize($"{Id}", size);

    public void SetPosition(Vector2 position) => ImGui.SetWindowPos($"{Id}", position);

    public Canvas GetCanvas()
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
        Overlay?.Dispose();
        IsOpen = true;
        try
        {
            Overlay = (IOverlay?)Activator.CreateInstance(Type, [this as IOverlayWindow]);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow Enable, overlay \'{Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Name} overlay {Id}\' Had an error, contact the autor for feedback");
            Disable();
        }
        EncounterManager.Inst.ClientsNotifier.DataUpdate += OnDataUpdate;
    }

    public void Disable()
    {
        if (Overlay == null) return;
        Overlay?.Dispose();
        Overlay = null;
        EncounterManager.Inst.ClientsNotifier.DataUpdate -= OnDataUpdate;
        IsOpen = false;
    }

    public void Remove()
    {
        Overlay?.Remove();
        Disable();
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
            Dalamud.Log.Error($"OverlayWindow OnDataUpdate, overlay \'{Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Name} overlay {Id}\' Had an error, contact the autor for feedback");
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