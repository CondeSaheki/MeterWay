using System.Collections.Generic;
using System;
using MeterWay.Managers;
using Dalamud.Game.Command;
using MeterWay.Windows;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace MeterWay;

public class Commands : IDisposable
{
    private static readonly string CommandName = "/meterway";
    private readonly Plugin Plugin;

    public Commands(Plugin plugin)
    {
        Plugin = plugin;
        Dictionary<string, Action> args = [];

        // add command
        InterfaceManager.Inst.CommandManager.AddHandler(CommandName, new CommandInfo((string command, string arg) =>
        {
            if (args.TryGetValue(arg, out Action? value)) value.Invoke();
            else
            {
                var text = $"The argument \'{arg}\' does not exist! consult \'{CommandName} help\'.";
                InterfaceManager.Inst.ChatGui.Print(new SeString(new UIForegroundPayload(539), new TextPayload(text), new UIForegroundPayload(0)));
            }
        })
        {
            HelpMessage = $"Display {Plugin.Name} main window.\nAditional help with the command \'{CommandName} help\'."
        });

        // add command arg
        string AddArg(string arg, string help, Action fn)
        {
            args.Add(arg, fn);
            return $"{CommandName}{(arg != "" ? " " : "")}{arg} -> {help}\n";
        }

        string help = AddArg("", "Display MeterWay main window.", () =>
        {
            Plugin.MainWindow.IsOpen = true;
        });

        help += AddArg("config", "Display MeterWay configuration window.", () =>
        {
            Plugin.ConfigWindow.IsOpen = true;
        });

        help += AddArg("status", "Display the connection to IINACT status.", () =>
        {
            var msg = $"MeterWay is {(Plugin.IpcClient.Status() ? "connected" : "disconnected")} to IINACT.";
            InterfaceManager.Inst.ChatGui.Print(msg);
        });

        help += AddArg("connect", "Try to connect to IINACT.", () =>
        {
            Plugin.IpcClient.Connect();
        });

        help += AddArg("disconnect", "Diconnect from IINACT.", () =>
        {
            Plugin.IpcClient.Disconnect();
        });

        help += AddArg("reconnect", "Try to reconnect to IINACT.", () =>
        {
            Plugin.IpcClient.Reconnect();
        });

        help += AddArg("help", "Display this help message", () =>
        {
            InterfaceManager.Inst.ChatGui.Print(help.Remove(help.Length - 1));
        });

#if DEBUG

        args.Add("_debug", () =>
        {
            Plugin.DebugWindow.IsOpen = true;
        });

        args.Add("_clear", () =>
        {
            EncounterManager.Inst.encounters.Clear();
            EncounterManager.Inst.encounters.Add(new Data.Encounter());
        });

        args.Add("_Stop", () =>
        {
            EncounterManager.Stop();
        });

        args.Add("_start", () =>
        {
            EncounterManager.Start();
        });

#endif
    }

    public void Dispose()
    {
        InterfaceManager.Inst.CommandManager.RemoveHandler(CommandName);
    }
}