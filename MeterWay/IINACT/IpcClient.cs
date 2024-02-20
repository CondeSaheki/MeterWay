using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Ipc;

namespace MeterWay.IINACT;
using MeterWay.Managers;

public class IpcClient : IDisposable
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

    public IpcClient()
    {
        this.receivers = new List<Action<JObject>>();
        connectionStatus = false;

        subscriptionReceiver = InterfaceManager.Inst.PluginInterface.GetIpcProvider<JObject, bool>(MeterwaySubscriptionReceiver);
        subscriptionReceiver.RegisterFunc(Receiver);

        if (InterfaceManager.Inst.ClientState.IsLoggedIn == true)
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
            InterfaceManager.Inst.ChatGui.Print("Meterway is already connected to IINACT.");
            return;
        }

        try
        {
            InterfaceManager.Inst.PluginInterface.GetIpcSubscriber<string, bool>(IINACTCreateSubscriber).InvokeFunc(MeterwaySubscriptionReceiver);
            InterfaceManager.Inst.PluginInterface.GetIpcSubscriber<JObject, bool>(IINACTSubscribe).InvokeAction(IINACTSubscribeMessage);
            connectionStatus = true;

            InterfaceManager.Inst.PluginLog.Info("Meterway is connected to IINACT.");
            InterfaceManager.Inst.ChatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway is connected to IINACT."), new UIForegroundPayload(0)));
        }
        catch (Exception ex)
        {
            InterfaceManager.Inst.PluginLog.Info("Meterway was unable to connected to IINACT.");
            InterfaceManager.Inst.ChatGui.Print(new SeString(new UIForegroundPayload(540), new TextPayload("Meterway was unable to connected to IINACT."), new UIForegroundPayload(0)));
            InterfaceManager.Inst.PluginLog.Error(ex.ToString());
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
            connectionStatus = !InterfaceManager.Inst.PluginInterface.GetIpcSubscriber<string, bool>(IINACTUnubscribe).InvokeFunc(MeterwaySubscriptionReceiver);
            connectionStatus = false;
            InterfaceManager.Inst.PluginLog.Info("Meterway disconnected.");
            InterfaceManager.Inst.ChatGui.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway disconnected."), new UIForegroundPayload(0)));
        }
        catch (Exception ex)
        {
            InterfaceManager.Inst.PluginLog.Warning("Meterway error while disconnecting: " + ex.ToString());
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