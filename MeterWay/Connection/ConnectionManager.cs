using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Newtonsoft.Json.Linq;

using MeterWay.Managers;
using Dalamud.Interface.ImGuiNotification;

namespace MeterWay.Connection;

/// <summary>
/// Enumeration representing the types of clients that can be used to connect.
/// </summary>
public enum ClientType
{
    Iinact,
    Act
}

/// <summary>
/// Manages the connection.
/// </summary>
public class ConnectionManager : IDisposable
{
    /// <summary>
    /// Types of subscriptions that can be made.
    /// </summary>
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

    /// <summary>
    /// Types of subscriptions that are currently active.
    /// </summary>
    public HashSet<SubscriptionType> Subscriptions { get; private set; } = [];

    /// <summary>
    /// Event that is triggered when data is received.
    /// </summary>
    public event EventHandler<JObject> OnDataReceived = delegate { };

    /// <summary>
    /// The client used to communicate with the server.
    /// </summary>
    public IClient? Client { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
    /// </summary>
    /// <param name="subscriptions">The types of subscriptions to make.</param>
    public ConnectionManager(HashSet<SubscriptionType>? subscriptions = null)
    {
        Subscriptions = subscriptions ?? [];
        Init();
        if (ConfigurationManager.Inst.Configuration.AutoConnect) AutoConnect();
    }

    /// <summary>
    /// Gets the current status of the connection.
    /// </summary>
    /// <returns>The current status of the connection.</returns>
    public ClientStatus Status() => Client?.GetStatus() ?? ClientStatus.NotInitialized;

    /// <summary>
    /// Connects.
    /// </summary>
    public void Connect() => Task.Run(() =>
    {
        if (Client == null || Client.GetStatus() == ClientStatus.Connected) return;

        Client.Connect();

        if (Client.GetStatus() != ClientStatus.Connected)
        {
            ReportFailure("Meterway could not connect.");
            return;
        }

        Subscribe();
        ReportSuccess("Meterway is connected.");
    });

    /// <summary>
    /// Disconnects.
    /// </summary>
    public void Disconnect() => Task.Run(() =>
    {
        if (Client == null || Client.GetStatus() == ClientStatus.Disconnected) return;

        Client.Disconnect();

        if (Client.GetStatus() != ClientStatus.Disconnected)
        {
            ReportFailure("Meterway could not disconnect.");
            return;
        }
        ReportSuccess("Meterway is disconnected.");
    });

    /// <summary>
    /// Reconnects.
    /// </summary>
    public void Reconnect() => Task.Run(() =>
    {
        if (Client == null || Client.GetStatus() != ClientStatus.Connected) return;
        Client.Reconnect();

        if (Client.GetStatus() != ClientStatus.Connected)
        {
            ReportFailure("Meterway could not reconnect.");
            return;
        }
        Subscribe();
        ReportSuccess("Meterway is reconnected.");
    });

    /// <summary>
    /// Disposes of the connection manager.
    /// </summary>
    public void Dispose()
    {
        Client?.Dispose();
    }

    /// <summary>
    /// Initializes the connection manager.
    /// </summary>
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
                _ => throw new Exception("Client is null")
            };
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ConnectionManager Init:\n{ex}");
            Client?.Dispose();
            Client = null;
        }
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

    private static void ReportSuccess(string message)
    {
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(73), new TextPayload(message), new UIForegroundPayload(0)));
        Dalamud.Notifications.AddNotification(new()
        {
            Title = message,
            InitialDuration = TimeSpan.FromSeconds(30),
            Type = NotificationType.Success
        });
    }

    private static void ReportFailure(string message)
    {
        Dalamud.Chat.Print(new SeString(new UIForegroundPayload(60), new TextPayload(message), new UIForegroundPayload(0)));
        Dalamud.Notifications.AddNotification(new()
        {
            Title = message,
            InitialDuration = TimeSpan.FromSeconds(30),
            Type = NotificationType.Error
        });
    }
}
