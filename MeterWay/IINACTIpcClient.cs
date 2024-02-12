using System;
using Newtonsoft.Json.Linq;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Ipc;

namespace Meterway;

public class IINACTIpcClient : IDisposable
{
    private Plugin plugin;

    private readonly ICallGateProvider<JObject, bool> subscriptionReceiver;
    private static readonly JObject IINACTSubscribeMessage = JObject.Parse("""{"call":"subscribe","events":["CombatData"]}""");
    private const string MeterwaySubscriptionReceiver = "Meterway.SubscriptionReceiver";
    private const string IINACTSubscribe = "IINACT.IpcProvider." + MeterwaySubscriptionReceiver;
    private const string IINACTUnubscribe = "IINACT.Unsubscribe";
    private const string IINACTCreateSubscriber = "IINACT.CreateSubscriber";

    private bool connectionStatus;

    public IINACTIpcClient(Plugin plugin, Func<JObject, bool> Receiver)
    {
        this.plugin = plugin;
        connectionStatus = false;

        subscriptionReceiver = plugin.PluginInterface.GetIpcProvider<JObject, bool>(MeterwaySubscriptionReceiver);
        subscriptionReceiver.RegisterFunc(Receiver);

        if (plugin.ClientState.IsLoggedIn == true)
        {
            Connect();
        }
    }

    public void Connect()
    {
        if (connectionStatus)
        {
            plugin.ChatGui.Print("Meterway is already connected to IINACT.");
            return;
        }

        try
        {
            plugin.PluginInterface.GetIpcSubscriber<string, bool>(IINACTCreateSubscriber).InvokeFunc(MeterwaySubscriptionReceiver);
            plugin.PluginInterface.GetIpcSubscriber<JObject, bool>(IINACTSubscribe).InvokeAction(IINACTSubscribeMessage);
            connectionStatus = true;

            plugin.PluginLog.Info("Meterway is connected to IINACT.");
            plugin.ChatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway is connected to IINACT."), new UIForegroundPayload(0)));
        }
        catch (Exception ex)
        {
            plugin.PluginLog.Info("Meterway was unable to connected to IINACT.");
            plugin.ChatGui.Print(new SeString(new UIForegroundPayload(540), new TextPayload("Meterway was unable to connected to IINACT."), new UIForegroundPayload(0)));
            plugin.PluginLog.Error(ex.ToString());
        }


    }
    
    public void Disconnect()
    {
        if (!connectionStatus)
        {
            return;
        }
        
        try
        {
            connectionStatus = !plugin.PluginInterface.GetIpcSubscriber<string, bool>(IINACTUnubscribe).InvokeFunc(MeterwaySubscriptionReceiver);
            connectionStatus = false;
            plugin.PluginLog.Info("Meterway disconnected.");
            plugin.ChatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway disconnected."), new UIForegroundPayload(0)));
        }
        catch (Exception ex)
        {
            plugin.PluginLog.Warning("Meterway error while disconnecting: " + ex.ToString());
            connectionStatus = false;
        }

    }

    public void Reconnect()
    {
        Disconnect();
        Connect();
    }

    public bool Status()
    {
        return this.connectionStatus;
    }

    public void Dispose()
    {
        subscriptionReceiver.UnregisterFunc();
        Disconnect();
    }
}