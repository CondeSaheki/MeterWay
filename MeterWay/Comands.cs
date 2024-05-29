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
        helpMessageBuilder("config", "MeterWay configuration window.");
        helpMessageBuilder("connect", "Connect.");
        helpMessageBuilder("disconnect", "Diconnect.");
        helpMessageBuilder("reconnect", "Reconnect.");
        helpMessageBuilder("status", "Connection status.");

        //helpMessageBuilder("overlay", "Run overlay command handler."); // wip

        helpMessageBuilder("help", "This help message");

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
            "connect" when args.Count == 1 => Plugin.ConnectionManager.Connect,
            "disconnect" when args.Count == 1 =>  Plugin.ConnectionManager.Disconnect,
            "reconnect" when args.Count == 1 =>  Plugin.ConnectionManager.Reconnect,
            "status" when args.Count == 1 => () => { Dalamud.Chat.Print($"MeterWay connection status: {Plugin.ConnectionManager.Status()}"); },

            // "overlay" => () => { Dalamud.Log.Info($"wip => run with args: {args}"); },

            "help" when args.Count == 1 => () => { Dalamud.Chat.Print(HelpMessage()); },

            #if DEBUG

            "_debug" when args.Count == 1 => () => { Plugin.DebugWindow.IsOpen = true; },
            "_start" when args.Count == 1 => () => { EncounterManager.Start(); },
            "_stop" when args.Count == 1 => () => { EncounterManager.Stop(); },
            "_clear" when args.Count == 1 => () =>
            {
                EncounterManager.Inst.encounters.Clear();
                EncounterManager.Inst.encounters.Add(new Data.Encounter());
            },  
            
            #endif

            _ => () =>
            {
                var text = $"Invalid command \'{args[0]}\' consult \'{CommandName} help\'.";
                Dalamud.Chat.Print(new SeString(new UIForegroundPayload(539), new TextPayload(text), new UIForegroundPayload(0)));
            }
        };
        handler();
    }

    public void Dispose()
    {
        Dalamud.Commands.RemoveHandler(CommandName);
    }
}