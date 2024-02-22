using System.Collections.Generic;
using System;
using MeterWay.Managers;
using Dalamud.Game.Command;

namespace MeterWay;

public class Commands : IDisposable
{
    private class BasicCommand
    {
        public string Argument;
        public string Help;
        public Action Function;

        public BasicCommand(string argument, string help, Action function)
        {
            this.Argument = argument;
            this.Help = help;
            this.Function = function;
        }
    }

    private readonly List<BasicCommand> List;
    private const string CommandName = "/meterway";
    private readonly Plugin Plugin;

    public Commands(Plugin plugin)
    {
        Plugin = plugin;
        List = CreateComands();

        InterfaceManager.Inst.CommandManager.AddHandler(CommandName,
        new CommandInfo(OnCommand)
        {
            HelpMessage = "Display MeterWay main window.\nAditional help with the command \'" + CommandName + " help\'."
        }
        );
    }

    private void OnCommand(string command, string args)
    {
        var i = List.FindIndex((BasicCommand cmd) =>
        {
            return cmd.Argument == args.ToLower();
        });
        List[i].Function.Invoke();
    }

    private List<BasicCommand> CreateComands()
    {
        List<BasicCommand> commands =
            [
                new BasicCommand("", "Display MeterWay main window.", () =>
                {
                    Plugin.MainWindow.IsOpen = true;
                }
            ),
            new BasicCommand("config", "Display MeterWay configuration window.", () =>
                {
                    Plugin.ConfigWindow.IsOpen = true;
                }
            ),
            new BasicCommand("status", "Display the connection to IINACT status.", () =>
                {
                    var msg = "MeterWay is " + (Plugin.IpcClient.Status() ? "connected" : "disconnected") + " to IINACT.";
                    //ChatGui.Print(new SeString(new UIForegroundPayload(540), new TextPayload(msg), new UIForegroundPayload(0)));
                    InterfaceManager.Inst.ChatGui.Print(msg);
                }
            ),
            new BasicCommand("start", "Try to connect to IINACT.", () =>
                {
                    Plugin.IpcClient.Connect();
                }
            ),
            new BasicCommand("stop", "Diconnect from IINACT.", () =>
                {
                    Plugin.IpcClient.Disconnect();
                }
            ),
            new BasicCommand("restart", "Try to reconnect to IINACT.", () =>
                {
                    Plugin.IpcClient.Reconnect();
                }
            ),
            new BasicCommand("debug_reset", "reset encounter", () =>
                {
                    EncounterManager.ResetEncounter();
                    
                }
            ),
            new BasicCommand("debug_end", "end encounter", () =>
                {
                    EncounterManager.EndEncounter();
                }
            ),
            new BasicCommand("debug_start", "start encounter", () =>
                {
                    EncounterManager.StartEncounter();
                }
            )
        ];
        string HelpMessage = "";
        commands.Add(new BasicCommand("help", "Display this help message", () =>
                {
                    InterfaceManager.Inst.ChatGui.Print(HelpMessage);
                }
            ));

        // create help message
        foreach (BasicCommand command in commands)
        {
            HelpMessage += CommandName + (command.Argument != "" ? " " : "") + command.Argument + " -> " + command.Help + (command.Argument != "help" ? "\n" : "");
        }

        return commands;
    }

    public void Dispose()
    {
        InterfaceManager.Inst.CommandManager.RemoveHandler(CommandName);
    }
}