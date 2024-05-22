using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;

using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;

namespace HelloWorld;

public class Overlay : IOverlay, IOverlayConfig, IOverlayCommandHandler
{
    public static string Name => "HelloWorld"; // required
    public static string Autor => "MeterWay";
    public static string Description => "An simple overlay.";

    private Configuration Config { get; init; }
    
    private OverlayWindow Window { get; init; }

    private Encounter Data = new();

    public Overlay(OverlayWindow overlayWindow)
    {
        Window = overlayWindow;
        Config = File.Load<Configuration>(Window.NameId);
        Window.Flags = ImGuiWindowFlags.None; // you can change the window properties if you want
    }

    // this function is called wenever you need to update data
    public void DataUpdate()
    {
        Data = EncounterManager.Inst.CurrentEncounter();
    }

    public void Draw()
    {
        ImGui.Text("Hello world this is a MeterWay overlay!");
        ImGui.Spacing();
        ImGui.Text($"Encounter Name: {Data.Name}");
        ImGui.Text($"\'Enabled\' Configuration: {(Config.Enabled ? "true" : "false")}");
    }

    public void DrawConfig()
    {
        ImGui.Text("I'm a configuration tab!\n");

        var enabledValue = Config.Enabled;
        if (ImGui.Checkbox("\'Enabled\' Configuration", ref enabledValue))
        {
            Config.Enabled = enabledValue;
            File.Save(Name, Config);
        }
    }
    
    public void Remove()
    {
        File.Delete(Window.NameId);
    }

    public string CommandHelpMessage(string? command)
    {
        StringBuilder builder = new();
        
        builder.Append("command");
        builder.Append("command");

        return builder.ToString();
    }

    public Action? OnCommand(List<string> args)
    {
        Action? handler = args[0] switch
        {
            "config" when args.Count == 1 => () =>
            {
                MeterWay.Dalamud.Chat.Print("hi");
            },
            "" when args.Count == 1 => null,
            _ => null
        };
        return handler;
    }

    public void Dispose() { }
}