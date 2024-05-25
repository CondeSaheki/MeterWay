using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;

using MeterWay.Utils;
using Dalamud.Utility;

namespace MeterWay.Connection;

public class ActWebSocket : IClient
{
    public Uri? Uri { get; set; } = null;

    private ThreadSafeEnum<ClientStatus> Status { get; set; } = new(ClientStatus.Disconnected);
    private Action<JObject> ReceiveAction { get; set; }
    private ClientWebSocket? Client { get; set; } = null;
    private CancellationTokenSource? ReceiverCancelSource { get; set; } = null;
    private readonly Mutex ClientLock = new();

    public ClientStatus GetStatus() => Status.Value;

    public ActWebSocket(Action<JObject> receive)
    {
        ReceiveAction = receive ?? throw new ArgumentNullException(nameof(receive));
    }

    public void Connect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Status.Value != ClientStatus.Disconnected)
            {
                Dalamud.Log.Warning("ActWebSocket Connect, Is already connected");
                return;
            };

            if (!TryConnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Error($"ActWebSocket Connect: Connection failed");
                return;
            }

            StartReceiver();

            Status.Value = ClientStatus.Connected;
            Dalamud.Log.Info($"ActWebSocket Connect: Done");
        }
        finally
        {
            ClientLock.ReleaseMutex();
        }
    }

    public void Disconnect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Status.Value != ClientStatus.Connected)
            {
                Dalamud.Log.Warning("ActWebSocket Disconnect, Is not connected");
                return;
            };

            StopReceiver();

            if (!TryDisconnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Warning($"ActWebSocket Disconnect: failed");
                return;
            }

            Status.Value = ClientStatus.Disconnected;
            Dalamud.Log.Info($"ActWebSocket Disconnect: Done");
        }
        finally
        {
            ClientLock.ReleaseMutex();
        }
    }

    public void Reconnect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Status.Value != ClientStatus.Connected)
            {
                Dalamud.Log.Warning("ActWebSocket Reconnect, Is not connected");
                return;
            };

            StopReceiver();

            if (!TryDisconnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Warning("ActWebSocket Reconnect: Failed Disconnect");
                return;
            }

            if (!TryConnect())
            {
                Status.Value = ClientStatus.Disconnected;
                Dalamud.Log.Warning("ActWebSocket Reconnect: Failed Connect");
                return;
            }

            StartReceiver();

            Status.Value = ClientStatus.Connected;
            Dalamud.Log.Info("ActWebSocket Reconnect: Done");
        }
        finally
        {
            ClientLock.ReleaseMutex();
        }
    }

    public void Send(JObject message)
    {
        ClientLock.WaitOne();
        try
        {
            if (Client == null) throw new InvalidOperationException("Client is null");
            if (Status.Value != ClientStatus.Connected) throw new InvalidOperationException("Is not connected");

            using var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            Client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message.ToString())), WebSocketMessageType.Text, true, cancelSource.Token).Wait(cancelSource.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ActWebSocket Send, message \'{message}\':\n{ex}");
            Disconnect();
        }
        finally
        {
            ClientLock.ReleaseMutex();
        }
    }

    public void Dispose()
    {
        ClientLock.WaitOne();
        try
        {
            StopReceiver();
            ForceDisconnect();
        }
        finally
        {
            ClientLock.Dispose();
        }
    }

    public bool ChangeUri(string addres)
    {
        ClientLock.WaitOne();
        try
        {
            if (addres.IsNullOrEmpty()) return false;
            if (Uri?.OriginalString == addres) return false;

            var newUri = new Uri(addres);            
            Uri = newUri;

            return true;
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ActWebSocket ChangeUri, Uri {Uri?.OriginalString ?? "null"}, addres {addres}:\n{ex}");
            return false;
        }
        finally
        {
            ClientLock.ReleaseMutex();
        }
    }


    private bool TryConnect()
    {
        try
        {
            if (Uri == null) throw new InvalidOperationException("Uri is null");
            if (Client != null) throw new InvalidOperationException("Client is not null");

            using var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            do
            {
                Client = new();
                try
                {
                    Client.ConnectAsync(Uri, cancelSource.Token).Wait(cancelSource.Token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Dalamud.Log.Info($"ActWebSocket TryConnect, ConnectAsync: \'{ex.Message}\'");
                    Client?.Dispose();
                    Client = null;
                    Task.Delay(TimeSpan.FromMilliseconds(50), cancelSource.Token).Wait(cancelSource.Token);
                    continue;
                }

                for (var delays = 0; delays != 4 && Client.State == WebSocketState.Connecting; ++delays)
                {
                    Task.Delay(TimeSpan.FromMilliseconds(50), cancelSource.Token).Wait(cancelSource.Token);
                }
                if (Client.State == WebSocketState.Open) return true;

                Task.Delay(TimeSpan.FromMilliseconds(50), cancelSource.Token).Wait(cancelSource.Token);

                Client?.Dispose();
                Client = null;
            }
            while (!cancelSource.IsCancellationRequested);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ActWebSocket TryConnect:\n{ex}");
        }
        Client?.Dispose();
        Client = null;
        return false;
    }

    private bool TryDisconnect()
    {
        try
        {
            if (Client == null) throw new InvalidOperationException("Client is null");
            if (Client.State != WebSocketState.Open && Client.State != WebSocketState.CloseSent && Client.State != WebSocketState.CloseReceived) throw new InvalidOperationException("Is not connected");

            using var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            do
            {
                Client.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancelSource.Token).Wait(cancelSource.Token);
                if (Client.State == WebSocketState.Closed) return true;
                Task.Delay(TimeSpan.FromMilliseconds(250), cancelSource.Token).Wait(cancelSource.Token);
            }
            while (Client.State != WebSocketState.Open && Client.State != WebSocketState.CloseSent && Client.State != WebSocketState.CloseReceived);

            Dalamud.Log.Warning($"ActWebSocket TryDisconnect: Forced done");
            return true; // Client.State == WebSocketState.Closed;
        }
        catch (OperationCanceledException) { return true; }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ActWebSocket TryDisconnect, ClientState {Client?.State.ToString() ?? "null"}:\n{ex}");
            return false;
        }
        finally
        {
            Client?.Dispose();
            Client = null;
        }
    }

    private void StartReceiver() => Task.Run(Receiver);

    private void StopReceiver() => ReceiverCancelSource?.Cancel();

    private void Receiver()
    {
        if (Client == null) throw new InvalidOperationException("Client is null");
        if (Client.State != WebSocketState.Open) throw new InvalidOperationException("Is not connected");
        if (ReceiverCancelSource != null)
        {
            Dalamud.Log.Error($"ActWebSocket Receiver: Cancellation token is not null");
            return;
        }
        try
        {
            ReceiverCancelSource = new();

            while (!ReceiverCancelSource.IsCancellationRequested)
            {
                byte[] buffer = new byte[1024];
                WebSocketReceiveResult result;
                StringBuilder builder = new();

                do
                {
                    // .Result gives Agreggate Task Canceled Exception
                    // cancelling token make client state aborted
                    var receiveTask = Client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    receiveTask.Wait(ReceiverCancelSource.Token);
                    result = receiveTask.Result;

                    // IINACT close hand shake is not implemented
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Dalamud.Log.Info("ActWebSocket Receiver: disconnect requested");
                        ForceDisconnect();
                        return;
                    }
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                } while (!result.EndOfMessage && !ReceiverCancelSource.IsCancellationRequested);

                if (!ReceiverCancelSource.IsCancellationRequested) Task.Run(() => Received(builder.ToString()));
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ActWebSocket Receiver:\n{ex}");
            ForceDisconnect();
        }
        finally
        {
            ReceiverCancelSource?.Dispose();
            ReceiverCancelSource = null;
        }
    }

    private void Received(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        try
        {
            JObject json = JObject.Parse(message);
            ReceiveAction.Invoke(json); // will not throw
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ActWebSocket Received, string \'{message}\':\n{ex}");
        }
    }

    private void ForceDisconnect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Client == null) return;
            if (Client.State != WebSocketState.Open && Client.State != WebSocketState.CloseSent && Client.State != WebSocketState.CloseReceived) return;
            _ = TryDisconnect();
        }
        finally
        {
            Client?.Dispose();
            Client = null;
            Status.Value = ClientStatus.Disconnected;
            ClientLock.ReleaseMutex();
        }
    }
}