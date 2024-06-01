using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using MeterWay.Managers;
using MeterWay.Utils;

namespace MeterWay.Overlay;

public class OverlayWindow : Window, IOverlayWindow
{
    public Type Type { get; private set; }
    public uint Id { get; private set; }

    public string Comment { get; set; }

    public bool HasConfigurationWindow { get; set; } = true;
    public Vector2 ConfigurationWindowSize { get; set; } = new(360, 640);

    public Vector2? CurrentSize { get; private set; }
    public Vector2? CurrentPosition { get; private set; }

    private BasicOverlay? Overlay { get; set; }

    public OverlayWindow(Type overlay, uint id) : base($"{id}")
    {
        Type = overlay;
        Id = id;
        Comment = $"{Info.Name}{Id}";
        
        Init();
    }

    public OverlayWindow(Type overlay, OverlayWindowSpec spec) : base($"{spec.Id}")
    {
        Type = overlay;
        Comment = spec.Comment;

        Init();

        if (spec.IsEnabled) Enable();
    }

    private void Init()
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
    }

    public bool IsEnabled() => Overlay != null;

    public BasicOverlayInfo Info => (BasicOverlayInfo?)Type.GetProperty("Info")?.GetValue(null) ?? throw new Exception("Missing Basic Overlay Information");

    public void SetSize(Vector2 size)
    {
        ImGui.SetWindowSize($"{Id}", size);
        CurrentSize = size;
    }

    public void SetPosition(Vector2 position)
    {
        ImGui.SetWindowPos($"{Id}", position);
        CurrentPosition = position;
    }

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
        if (Overlay != null)
        {
            Dalamud.Log.Warning("OverlayWindow Enable: Overlay is not null");
            return;
        }
        try
        {
            EncounterManager.Inst.ClientsNotifier.DataUpdate += OnEncounterUpdate;
            EncounterManager.Inst.EncounterBegin += OnEncounterBegin;
            EncounterManager.Inst.EncounterEnd += OnEncounterEnd;

            Overlay = (BasicOverlay?)Activator.CreateInstance(Type, [this as IOverlayWindow]);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow Enable, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
    }

    public void Disable()
    {
        if (Overlay == null)
        {
            Dalamud.Log.Warning("OverlayWindow Disable: Overlay is null");
        }
        try
        {
            EncounterManager.Inst.EncounterEnd -= OnEncounterEnd;
            EncounterManager.Inst.EncounterBegin -= OnEncounterBegin;
            EncounterManager.Inst.ClientsNotifier.DataUpdate -= OnEncounterUpdate;

            Overlay?.Dispose();
            Overlay = null;
            IsOpen = false;
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow Disable, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
    }

    public override void Draw()
    {
        if (Overlay == null) return;
        CurrentSize = ImGui.GetWindowSize();
        CurrentPosition = ImGui.GetWindowPos();
        try
        {
            Overlay.Draw();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow Draw, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
    }

    public override void Update()
    {
        try
        {
            Overlay?.Update();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow Update, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
    }

    public override void OnOpen()
    {
        try
        {
            Overlay?.OnOpen();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow OnOpen, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
    }

    public override void OnClose()
    {
        try
        {
            Overlay?.OnClose();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow OnClose, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
    }

    public void OnEncounterUpdate(object? _, EventArgs __)
    {
        try
        {
            Overlay?.OnEncounterUpdate();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow OnEncounterUpdate, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
    }

    public void OnEncounterBegin(object? _, EventArgs __)
    {
        try
        {
            Overlay?.OnEncounterBegin();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow OnEncounterBegin, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
    }

    public void OnEncounterEnd(object? _, EventArgs __)
    {
        try
        {
            Overlay?.OnEncounterEnd();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow OnEncounterEnd, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
    }

    public void DrawConfiguration()
    {
        try
        {
            Overlay?.DrawConfiguration();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow DrawConfig, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
    }

    public void Remove()
    {
        try
        {
            Overlay?.Remove();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayWindow Remove, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
            Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
            Disable();
        }
        Disable();
    }

    public void Dispose()
    {
        if (IsEnabled()) Disable();
    }
}