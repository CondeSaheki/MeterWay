using System;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using MeterWay.Managers;
using MeterWay.Utils;

namespace MeterWay.Overlay;

public class OverlayWindow : Window, IOverlayWindow
{
    public Type Type { get; private init; }
    public uint Id { get; private init; }

    public string Comment { get; set; }

    public bool HasConfigurationWindow { get; set; } = true;
    public Vector2 ConfigurationWindowSize { get; set; } = new(360, 640);

    public Vector2? CurrentSize { get; private set; }
    public Vector2? CurrentPosition { get; private set; }

    private BasicOverlay? Overlay { get; set; }

    private bool PendingSetWindowSize { get; set; } = false;
    private bool PendingSetWindowPosition { get; set; } = false;

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
        Id = spec.Id;
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
        CurrentSize = size;
        PendingSetWindowSize = true;
    }

    public void SetPosition(Vector2 position)
    {
        CurrentPosition = position;
        PendingSetWindowPosition = true;
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
            ReportException("Enable", ex);
            Disable();
        }
    }

    public void Disable()
    {
        try
        {
            if (Overlay == null)
            {
                Dalamud.Log.Warning("OverlayWindow Disable: Overlay is null");
            }
            IsOpen = false;
            Overlay?.Dispose();
        }
        catch (Exception ex)
        {
            ReportException("Disable", ex);
        }
        finally
        {
            Overlay = null;
            IsOpen = false;

            EncounterManager.Inst.EncounterEnd -= OnEncounterEnd;
            EncounterManager.Inst.EncounterBegin -= OnEncounterBegin;
            EncounterManager.Inst.ClientsNotifier.DataUpdate -= OnEncounterUpdate;
        }
    }

    public override void Draw()
    {
        if (Overlay == null) return;
        if (PendingSetWindowSize)
        {
            ImGui.SetWindowSize(WindowName, CurrentSize ?? new(0, 0));

            if (ImGui.GetWindowSize() != CurrentSize)
            {
                Dalamud.Log.Warning("OverlayWindow Draw: Size mismatch");
            }
            else
            {
                PendingSetWindowSize = false;
            }
        }
        else
        {
            CurrentSize = ImGui.GetWindowSize();
        }
        if (PendingSetWindowPosition)
        {
            ImGui.SetWindowPos(WindowName, CurrentPosition ?? new(0, 0));
            if (ImGui.GetWindowPos() != CurrentPosition)
            {
                Dalamud.Log.Warning("OverlayWindow Draw: Position mismatch");
            }
            else
            {
                PendingSetWindowPosition = false;
            }
        }
        else
        {
            CurrentPosition = ImGui.GetWindowPos();
        }

        try
        {
            Overlay.Draw();
        }
        catch (Exception ex)
        {
            ReportException("Draw", ex);
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
            ReportException("Update", ex);
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
            ReportException("OnOpen", ex);
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
            ReportException("OnClose", ex);
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
            ReportException("OnEncounterUpdate", ex);
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
            ReportException("OnEncounterBegin", ex);
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
            ReportException("OnEncounterEnd", ex);
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
            ReportException("DrawConfiguration", ex);
            Disable();
        }
    }

    public void Remove()
    {
        try
        {
            if (IsEnabled()) Disable();
            var removeMethod = Type.GetMethod("Remove", BindingFlags.Static | BindingFlags.Public, null, [typeof(IOverlayWindow)], null);
            removeMethod?.Invoke(null, [this]);
        }
        catch (Exception ex)
        {
            ReportException("Remove", ex);
            Disable();
        }
    }

    public void Dispose()
    {
        IsOpen = false;
        if (IsEnabled()) Disable();
    }

    private void ReportException(string origin, Exception ex)
    {
        Dalamud.Log.Error($"OverlayWindow {origin}, overlay \'{Info.Name}\', id \'{Id}\':\n{ex}");
        Dalamud.Chat.Print($"[Meterway] \'{Info.Name}\' encountered an error. Contact \'{Info.Author}\' for support.");
        Dalamud.Notifications.AddNotification(new()
        {
            Title = $"[Meterway] Error in {Info.Name}",
            Content = $"An error occurred in the overlay '{Info.Name}' (ID: {Id}) during {origin}. Please contact '{Info.Author}' for support.",
            InitialDuration = TimeSpan.FromSeconds(30),
            Type = NotificationType.Error
        });
    }
}