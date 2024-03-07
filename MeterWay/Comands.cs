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
        Dalamud.Commands.AddHandler(CommandName, new CommandInfo((string command, string arg) => { OnCommand(arg); })
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
            "connect" when args.Count == 1 => Plugin.IinactIpcClient.Connect,
            "disconnect" when args.Count == 1 => Plugin.IinactIpcClient.Disconnect,
            "reconnect" when args.Count == 1 => Plugin.IinactIpcClient.Reconnect,
            "status" when args.Count == 1 => () =>
            {
                var msg = $"MeterWay is {(Plugin.IinactIpcClient.Status() ? "connected" : "disconnected")} to IINACT.";
                Dalamud.Chat.Print(msg);
            },
            "help" when args.Count == 1 => () => { Dalamud.Chat.Print(HelpMessage()); },

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
                Dalamud.Chat.Print(new SeString(new UIForegroundPayload(539), new TextPayload(text), new UIForegroundPayload(0)));
            }
        };
        handler();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dalamud.Commands.RemoveHandler(CommandName);
    }
}