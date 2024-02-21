using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System.Collections.Generic;

using MeterWay.Windows;
using MeterWay.IINACT;
using MeterWay.Commands;
using MeterWay.Managers;
using MeterWay.Overlays;

using System.Linq;
using MeterWay.Utils;
using System;

namespace MeterWay
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "MeterWay";

        //public Configuration Configuration { get; init; }
        private WindowSystem WindowSystem = new("MeterWay");

        // windows
        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }
        private OverlayWindow OverlayWindow { get; init; }

        // meterway stuff
        public IpcClient IpcClient { get; init; }
        public EncounterManager EncounterManager { get; init; }

        private const string CommandName = "/meterway";
        private List<MeterWayCommand> commands { get; init; }

        private readonly InterfaceManager pluginManager;
        private readonly ConfigurationManager configurationManager;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] IPluginLog pluginLog,
            [RequiredVersion("1.0")] IChatGui chatGui,
            [RequiredVersion("1.0")] IClientState clientState,
            [RequiredVersion("1.0")] ICondition condition,
            [RequiredVersion("1.0")] IPartyList partyList,
            [RequiredVersion("1.0")] IDataManager datamanager,
            [RequiredVersion("1.0")] IDutyState dutyState,
            [RequiredVersion("1.0")] ITextureProvider textureProvider
            )
        {
            // add all interfaces to the manager
            this.pluginManager = new InterfaceManager(WindowSystem, pluginInterface, commandManager, pluginLog, chatGui, clientState, condition, partyList, datamanager, textureProvider, dutyState);
            this.configurationManager = new ConfigurationManager();

            this.EncounterManager = new EncounterManager();
            this.IpcClient = new IpcClient();
            IpcClient.receivers.Add(EncounterManager.Receiver);

            MainWindow = new MainWindow();
            OverlayWindow = new OverlayWindow();
            ConfigWindow = new ConfigWindow(this.IpcClient, OverlayWindow);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            // register any overlay here
            OverlayWindow.Overlays = [new LazerOverlay(), new MoguOverlay()];

            // TODO make only Active Overlay subscribed
            foreach (var overlay in OverlayWindow.Overlays)
            {
                EncounterManager.Inst.Clients.Add(overlay.DataProcess);
            }

            if (ConfigurationManager.Instance.Configuration.Overlay)
            {
                WindowSystem.AddWindow(OverlayWindow);
            }

            InterfaceManager.Inst.CommandManager.AddHandler(CommandName,
                new CommandInfo(OnCommand)
                {
                    HelpMessage = "Display MeterWay main window.\nAditional help with the command \'" + CommandName + " help\'."
                }
            );
            this.commands = RegisterCommands();

            InterfaceManager.Inst.PluginInterface.UiBuilder.Draw += DrawUI;
            InterfaceManager.Inst.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;


            InterfaceManager.Inst.DutyState.DutyStarted += Teste;

            // var asdsad = new EventHandler(InterfaceManager.Inst.DutyState.DutyStarted, e);    

        }


        // private event EventHandler<ushort> DutyStarted;
        // protected virtual void OnDutyStarted()
        // {
        //     EndEncounter();
        //     Helpers.Log("Duty started event");
        // }

        public void Teste<Args>(Object? sender, Args a)
        {
            // EventHandler<ushort>
            Helpers.Log("Duty started event AAAAAAAAAAAAAAAAAAAAAAAAAAA");
        }
    

    public void Dispose()
    {
        this.WindowSystem.RemoveAllWindows();
        IpcClient.Dispose();
        ConfigWindow.Dispose();
        MainWindow.Dispose();
        InterfaceManager.Inst.CommandManager.RemoveHandler(CommandName);
    }

    private List<MeterWayCommand> RegisterCommands()
    {
        List<MeterWayCommand> commandlist = new List<MeterWayCommand>();
        commandlist.Add(new MeterWayCommand("", "Display MeterWay main window.", () =>
            {
                MainWindow.IsOpen = true;
            }
        ));
        commandlist.Add(new MeterWayCommand("config", "Display MeterWay configuration window.", () =>
            {
                ConfigWindow.IsOpen = true;
            }
        ));
        commandlist.Add(new MeterWayCommand("status", "Display the connection to IINACT status.", () =>
            {
                var msg = "MeterWay is " + (IpcClient.Status() ? "connected" : "disconnected") + " to IINACT.";
                //ChatGui.Print(new SeString(new UIForegroundPayload(540), new TextPayload(msg), new UIForegroundPayload(0)));
                InterfaceManager.Inst.ChatGui.Print(msg);
            }
        ));
        commandlist.Add(new MeterWayCommand("start", "Try to connect to IINACT.", () =>
            {
                IpcClient.Connect();
            }
        ));
        commandlist.Add(new MeterWayCommand("stop", "Diconnect from IINACT.", () =>
            {
                IpcClient.Disconnect();
            }
        ));
        commandlist.Add(new MeterWayCommand("restart", "Try to reconnect to IINACT.", () =>
            {
                IpcClient.Reconnect();
            }
        ));
        string HelpMessage = "";
        commandlist.Add(new MeterWayCommand("help", "Display this help message", () =>
            {
                InterfaceManager.Inst.ChatGui.Print(HelpMessage);
            }
        ));

        // create help message
        foreach (MeterWayCommand cmd in commandlist)
        {
            HelpMessage += CommandName + (cmd.Argument != "" ? " " : "") + cmd.Argument + " -> " + cmd.Help + (cmd.Argument != "help" ? "\n" : "");
        }

        return commandlist;
    }

    private void OnCommand(string command, string args)
    {
        var i = commands.FindIndex((MeterWayCommand cmd) =>
        {
            return cmd.Argument == args.ToLower();
        });
        commands[i].Function.Invoke();
    }

    private void DrawUI()
    {
        this.WindowSystem.Draw();
    }

    public void DrawConfigUI()
    {
        ConfigWindow.IsOpen = true;
    }
}
}