using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Newtonsoft.Json.Linq;

using MeterWay.Managers;

namespace MeterWay.Connection;

public enum ClientType
{
    Iinact = 0,
    Act = 1
}

public class ConnectionManager : IDisposable
{
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

    public HashSet<SubscriptionType> Subscriptions { get; set; } = [];

    public event EventHandler<JObject> OnDataReceived = delegate { };

    public IClient? Client { get; private set; }

    public ConnectionManager()
    {
        Init();
        if (ConfigurationManager.Inst.Configuration.AutoConnect) AutoConnect();
    }

    public ClientStatus Status() => Client?.GetStatus() ?? ClientStatus.NotInitialized;

    public void Connect() => Task.Run(() =>
    {
        if (Client == null || Client.GetStatus() == ClientStatus.Connected) return;

        Client.Connect();

        if (Client.GetStatus() != ClientStatus.Connected)
        {
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload("Meterway could not connect."), new UIForegroundPayload(0)));
            return;
        }

        Subscribe();
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway is connected."), new UIForegroundPayload(0)));
    });

    public void Disconnect() => Task.Run(() =>
    {
        if (Client == null || Client.GetStatus() == ClientStatus.Disconnected) return;

        Client.Disconnect();

        if (Client.GetStatus() != ClientStatus.Disconnected)
        {
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload("Meterway could not disconnect."), new UIForegroundPayload(0)));
            return;
        }
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway is disconnected."), new UIForegroundPayload(0)));
    });

    public void Reconnect() => Task.Run(() =>
    {
        if (Client == null || Client.GetStatus() != ClientStatus.Connected) return;
        Client.Reconnect();

        if (Client.GetStatus() != ClientStatus.Connected)
        {
            Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload("Meterway could not reconnect."), new UIForegroundPayload(0)));
            return;
        }
        Subscribe();
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload("Meterway is reconnected."), new UIForegroundPayload(0)));
    });

    public void Dispose()
    {
        Client?.Dispose();
    }

    private void Send(JObject message) => Task.Run(() => Client?.Send(message));

    private void AutoConnect()
    {
        void OnLogin(object? _)
        {
            if (!Dalamud.ClientState.IsLoggedIn) return;
            Dalamud.Framework.Update -= OnLogin;
            Connect();
        }

        if (Dalamud.ClientState.IsLoggedIn) Connect();
        else Dalamud.Framework.Update += OnLogin;
    }

    public void Init()
    {
        try
        {
            if (Client != null)
            {
                if (Client.GetStatus() == ClientStatus.Connected) Client.Disconnect();
                Client.Dispose();
                Client = null;
            };

            void ReceiveAction(JObject json) => NotifyAll(json);

            Client = ConfigurationManager.Inst.Configuration.ClientType switch
            {
                ClientType.Iinact => new IinactIpc(ReceiveAction),
                ClientType.Act => new ActWebSocket(ReceiveAction)
                {
                    Uri = new(ConfigurationManager.Inst.Configuration.Address)
                },
                _ => null
            };

            if (Client == null) throw new Exception("Client is null");
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ConnectionManager Init:\n{ex}");
        }
    }

    private void NotifyAll(JObject json)
    {
        try
        {
            OnDataReceived?.Invoke(this, json);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ConnectionManager NotifyAll, Json \'{json}\':\n{ex}");
        }
    }

    private void Subscribe()
    {
        if (Subscriptions.Count == 0)
        {
            Dalamud.Log.Warning("ConnectionManager Subscribe: Subscriptions is empty");
            return;
        }

        if (Client == null || Client.GetStatus() != ClientStatus.Connected) return;

        static JObject CreateMessage(SubscriptionType type) => JObject.Parse(@$"{{""call"": ""subscribe"", ""events"": [ ""{type}"" ] }}");

        foreach (var sub in Subscriptions)
        {
            Send(CreateMessage(sub));
        }
        Dalamud.Log.Info($"ConnectionManager is Subscribed to [{string.Join(", ", Subscriptions)}]");
    }
}