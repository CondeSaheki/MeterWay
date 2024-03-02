using System.Collections.Generic;
using System;
using MeterWay.Managers;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Text;

namespace MeterWay;

public class Commands : IDisposable
{
    private static readonly string CommandName = "/meterway";
    private readonly Plugin Plugin;

    public Commands(Plugin plugin)
    {
        Plugin = plugin;
        InterfaceManager.Inst.CommandManager.AddHandler(CommandName, new CommandInfo((string command, string arg) => { OnCommand(arg); })
        {
            HelpMessage = $"Display {Plugin.Name} main window.\nAditional help with the command \'{CommandName} help\'."
        });
    }

    private static string HelpMessage() 
    {
        StringBuilder Message = new();
        void helpMessageBuilder(string arg, string help) => Message.Append($"{CommandName}{(arg != "" ? " " : "")}{arg} -> {help}\n");

        helpMessageBuilder("", "Display MeterWay main window.");
        helpMessageBuilder("config", "Display MeterWay configuration window.");
        helpMessageBuilder("connect", "Try to connect to IINACT.");
        helpMessageBuilder("disconnect", "Diconnect from IINACT.");
        helpMessageBuilder("reconnect", "Try to reconnect to IINACT.");
        helpMessageBuilder("status", "Display the connection to IINACT status.");
        helpMessageBuilder("help", "Display this help message");

        Message.Remove(Message.Length - 1, 1);
        
        return Message.ToString();
    }

    private void OnCommand(string arg)
    {
        List<string> args = [.. arg.Split(' ')];
        Action handler = args[0] switch
        {
            "" when args.Count == 1 => () => { Plugin.MainWindow.IsOpen = true; },
            "config" when args.Count == 1 => () => { Plugin.ConfigWindow.IsOpen = true; },
            "connect" when args.Count == 1 => Plugin.IpcClient.Connect,
            "disconnect" when args.Count == 1 => Plugin.IpcClient.Disconnect,
            "reconnect" when args.Count == 1 => Plugin.IpcClient.Reconnect,
            "status" when args.Count == 1 => () =>
            {
                var msg = $"MeterWay is {(Plugin.IpcClient.Status() ? "connected" : "disconnected")} to IINACT.";
                InterfaceManager.Inst.ChatGui.Print(msg);
            },
            "help" when args.Count == 1 => () => { InterfaceManager.Inst.ChatGui.Print(HelpMessage()); },

            #if DEBUG

            "_debug" when args.Count == 1 => () => { Plugin.DebugWindow.IsOpen = true; },
            "_start" when args.Count == 1 => () => { EncounterManager.Start(); },
            "_Stop" when args.Count == 1 => () => { EncounterManager.Stop(); },
            "_clear" when args.Count == 1 => () =>
            {
                EncounterManager.Inst.encounters.Clear();
                EncounterManager.Inst.encounters.Add(new Data.Encounter());
            },
            
            #endif

            _ => () =>
            {
                var text = $"Invalid command \'{args}\' consult \'{CommandName} help\'.";
                InterfaceManager.Inst.ChatGui.Print(new SeString(new UIForegroundPayload(539), new TextPayload(text), new UIForegroundPayload(0)));
            }
        };
        handler();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        InterfaceManager.Inst.CommandManager.RemoveHandler(CommandName);
    }
}