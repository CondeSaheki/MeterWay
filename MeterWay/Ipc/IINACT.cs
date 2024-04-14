using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Ipc;

namespace MeterWay.Ipc;

public class IINACTClient : IIpcClient, IDisposable
{
    public event EventHandler<JObject> OnDataReceived = delegate { };

    private bool connectionStatus;
    private readonly ICallGateProvider<JObject, bool>? subscriptionReceiver;
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
        connectionStatus = false;

        try
        {
            subscriptionReceiver = Dalamud.PluginInterface.GetIpcProvider<JObject, bool>(MeterwaySubscriptionReceiver);
            subscriptionReceiver.RegisterFunc(Receiver);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Info($"Error while creating IINACTClient instance:\n{ex}");
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(540), new TextPayload("Meterway was unable to create an IINACT client."), new UIForegroundPayload(0)));
            subscriptionReceiver = null;
        }
    }

    private bool Receiver(JObject json)
    {
        try
        {
            OnDataReceived?.Invoke(this, json);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Unhandeld error when dispatching received data from iinact:\n{ex}");
            Disconnect();
            throw;
        }
        return true;
    }

    public void Connect()
    {
        if (connectionStatus)
        {
            Dalamud.Chat.Print("Meterway is already connected to IINACT.");
            return;
        }

        try
        {
            Dalamud.PluginInterface.GetIpcSubscriber<string, bool>(IINACTCreateSubscriber).InvokeFunc(MeterwaySubscriptionReceiver);
            connectionStatus = true;

            Dalamud.Log.Info("Meterway is connected to IINACT.");
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway is connected to IINACT."), new UIForegroundPayload(0)));
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Meterway was unable to connected to IINACT:\n{ex}");
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(540), new TextPayload("Meterway was unable to connected to IINACT."), new UIForegroundPayload(0)));
        }
    }

    public void Subscribe(HashSet<SubscriptionType> Subscriptions)
    {
        if (!Subscriptions.Any(x => true))
        {
            Dalamud.Log.Info("trying to Subscribe with no subscription types");
            return;
        }

        if (!connectionStatus) return;

        static JObject CreateMessage(SubscriptionType type) => JObject.Parse(@$"{{""call"": ""subscribe"", ""events"": [ ""{type}"" ] }}");

        try
        {
            foreach (var sub in Subscriptions) Dalamud.PluginInterface.GetIpcSubscriber<JObject, bool>(IINACTSubscribe).InvokeAction(CreateMessage(sub));
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Meterway was unable to subscribe to IINACT:\n{ex}");
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (!connectionStatus) return;

        try
        {
            connectionStatus = !Dalamud.PluginInterface.GetIpcSubscriber<string, bool>(IINACTUnubscribe).InvokeFunc(MeterwaySubscriptionReceiver);
            connectionStatus = false;
            Dalamud.Log.Info("Meterway disconnected.");
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway disconnected."), new UIForegroundPayload(0)));
        }
        catch (Exception ex)
        {
            Dalamud.Log.Warning($"Meterway error while disconnecting:\n{ex}");
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
        subscriptionReceiver?.UnregisterFunc();
        Disconnect();
        GC.SuppressFinalize(this);
    }
}