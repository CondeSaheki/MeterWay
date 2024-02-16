using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Ipc;

namespace MeterWay.IINACT;
using MeterWay.managers;

public class IINACTIpcClient : IDisposable
{

    public List<Action<JObject>> receivers;

    private readonly ICallGateProvider<JObject, bool> subscriptionReceiver;
    // CombatData json ->  JObject.Parse("""{"call":"subscribe","events":["CombatData"]}""");
    private static readonly JObject IINACTSubscribeMessage = JObject.Parse("""{"call":"subscribe","events":["LogLine"]}""");
    private const string MeterwaySubscriptionReceiver = "Meterway.SubscriptionReceiver";
    private const string IINACTSubscribe = "IINACT.IpcProvider." + MeterwaySubscriptionReceiver;
    private const string IINACTUnubscribe = "IINACT.Unsubscribe";
    private const string IINACTCreateSubscriber = "IINACT.CreateSubscriber";

    private bool connectionStatus;

    public IINACTIpcClient()
    {
        this.receivers = new List<Action<JObject>>();
        connectionStatus = false;

        subscriptionReceiver = PluginManager.Instance.PluginInterface.GetIpcProvider<JObject, bool>(MeterwaySubscriptionReceiver);
        subscriptionReceiver.RegisterFunc(Receiver);

        if (PluginManager.Instance.ClientState.IsLoggedIn == true)
        {
            Connect();
        }
    }

    private bool Receiver(JObject json)
    {
        foreach (Action<JObject> fn in receivers)
        {
            fn(json);
        }
        return true;
    }

    public void Connect()
    {
        if (connectionStatus)
        {
            PluginManager.Instance.ChatGui.Print("Meterway is already connected to IINACT.");
            return;
        }

        try
        {
            PluginManager.Instance.PluginInterface.GetIpcSubscriber<string, bool>(IINACTCreateSubscriber).InvokeFunc(MeterwaySubscriptionReceiver);
            PluginManager.Instance.PluginInterface.GetIpcSubscriber<JObject, bool>(IINACTSubscribe).InvokeAction(IINACTSubscribeMessage);
            connectionStatus = true;

            PluginManager.Instance.PluginLog.Info("Meterway is connected to IINACT.");
            PluginManager.Instance.ChatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway is connected to IINACT."), new UIForegroundPayload(0)));
        }
        catch (Exception ex)
        {
            PluginManager.Instance.PluginLog.Info("Meterway was unable to connected to IINACT.");
            PluginManager.Instance.ChatGui.Print(new SeString(new UIForegroundPayload(540), new TextPayload("Meterway was unable to connected to IINACT."), new UIForegroundPayload(0)));
            PluginManager.Instance.PluginLog.Error(ex.ToString());
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
            connectionStatus = !PluginManager.Instance.PluginInterface.GetIpcSubscriber<string, bool>(IINACTUnubscribe).InvokeFunc(MeterwaySubscriptionReceiver);
            connectionStatus = false;
            PluginManager.Instance.PluginLog.Info("Meterway disconnected.");
            PluginManager.Instance.ChatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway disconnected."), new UIForegroundPayload(0)));
        }
        catch (Exception ex)
        {
            PluginManager.Instance.PluginLog.Warning("Meterway error while disconnecting: " + ex.ToString());
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