using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Ipc;

using MeterWay.Managers;

namespace MeterWay.Ipc;

public delegate void ReadOnlyJObject(ref readonly JObject json);

public class IINACTClient : IIpcClient, IDisposable
{
    public List<ReadOnlyJObject> Receivers { get; init; }

    private bool connectionStatus;
    private readonly ICallGateProvider<JObject, bool> subscriptionReceiver;
    private const string MeterwaySubscriptionReceiver = "Meterway.SubscriptionReceiver";
    private const string IINACTSubscribe = "IINACT.IpcProvider." + MeterwaySubscriptionReceiver;
    private const string IINACTUnubscribe = "IINACT.Unsubscribe";
    private const string IINACTCreateSubscriber = "IINACT.CreateSubscriber";

    public enum SubscriptionType
    {
        LogLine,
        ImportedLogLines,
        ChangeZone,
        ChangePrimaryPlayer,
        OnlineStatusChanged,
        PartyChanged,
        BroadcastMessage
    }

    public IINACTClient()
    {
        Receivers = [];
        connectionStatus = false;

        subscriptionReceiver = InterfaceManager.Inst.PluginInterface.GetIpcProvider<JObject, bool>(MeterwaySubscriptionReceiver);
        subscriptionReceiver.RegisterFunc(Receiver);
    }

    private bool Receiver(JObject json)
    {
        foreach (var fn in Receivers) fn.Invoke(ref json);
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

    public void Subscribe(HashSet<SubscriptionType> Subscriptions)
    {
        if (!Subscriptions.Any(x => true))
        {
            InterfaceManager.Inst.PluginLog.Info("trying to Subscribe with no subscription types");
            return;
        }

        if (!connectionStatus)
        {
            InterfaceManager.Inst.ChatGui.Print("Meterway is not connected to IINACT.");
            return;
        }

        static JObject CreateMessage(SubscriptionType type) => JObject.Parse(@$"{{""call"": ""subscribe"", ""events"": [ {type} ] }}");

        try
        {
            foreach (var sub in Subscriptions) InterfaceManager.Inst.PluginInterface.GetIpcSubscriber<JObject, bool>(IINACTSubscribe).InvokeAction(CreateMessage(sub));
        }
        catch (Exception ex)
        {
            InterfaceManager.Inst.PluginLog.Info("Meterway was unable to subscribe to IINACT.");
            InterfaceManager.Inst.PluginLog.Error(ex.ToString());
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (!connectionStatus) return;

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

    public bool Status() => connectionStatus;

    public void Dispose()
    {
        subscriptionReceiver.UnregisterFunc();
        Disconnect();
        GC.SuppressFinalize(this);
    }
}