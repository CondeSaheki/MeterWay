﻿using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

using Meterway.Windows;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;

namespace Meterway
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "MeterWay";

        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("Meterway");

        // dalamud interfaces
        public DalamudPluginInterface PluginInterface { get; init; }
        public ICommandManager CommandManager { get; init; }
        public IPluginLog PluginLog { get; init; }
        public IChatGui ChatGui { get; init; }
        public IClientState ClientState { get; init; }
        public ICondition Condition { get; init; }
        public IPartyList PartyList { get; init; }

        public ITextureProvider TextureProvider { get; init; }

        // window
        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }
        public OverlayWindow OverlayWindow { get; init; }

        // meterway stuff
        public IINACTIpcClient IpcClient { get; init; }
        public DataManager dataManager { get; init; }

        private const string CommandName = "/meterway";
        private List<MeterWayCommand> commands { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] IPluginLog pluginLog,
            [RequiredVersion("1.0")] IChatGui chatGui,
            [RequiredVersion("1.0")] IClientState clientState,
            [RequiredVersion("1.0")] ICondition condition,
            [RequiredVersion("1.0")] IPartyList partyList,
            [RequiredVersion("1.0")] ITextureProvider textureProvider
            )
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.PluginLog = pluginLog;
            this.ChatGui = chatGui;
            this.ClientState = clientState;
            this.Condition = condition;
            this.PartyList = partyList;
            this.TextureProvider = textureProvider;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            this.dataManager = new DataManager(this);
            this.IpcClient = new IINACTIpcClient(this, dataManager.Receiver);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);
            OverlayWindow = new OverlayWindow(this);


            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            if (Configuration.Overlay)
            {
                WindowSystem.AddWindow(OverlayWindow);
            }




            this.CommandManager.AddHandler(CommandName,
                new CommandInfo(OnCommand)
                {
                    HelpMessage = "Display MeterWay main window.\nAditional help with the command \'" + CommandName + " help\'."
                }
            );
            this.commands = RegisterCommands();

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            IpcClient.Dispose();
            ConfigWindow.Dispose();
            MainWindow.Dispose();
            this.CommandManager.RemoveHandler(CommandName);
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
                    ChatGui.Print(msg);
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
                    ChatGui.Print(HelpMessage);
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