using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;

using MeterWay.Utils;

namespace MeterWay.Connection;

public class ActWebSocket : IClient
{
    public Uri? Uri { get; set; } = null;

    private ThreadSafeEnum<ClientStatus> Status { get; set; } = new(ClientStatus.None);
    private Action<JObject> ReceiveAction { get; set; }
    private ClientWebSocket Client { get; } = new();
    private Task? ReceiverTask { get; set; } = null;
    private CancellationTokenSource? ReceiverCancelSource { get; set; } = null;

    private readonly object ReceiverCancelSourceLock = new();
    private readonly Mutex ClientLock = new();

    public ActWebSocket(Action<JObject> receive)
    {
        ReceiveAction = receive ?? throw new ArgumentNullException(nameof(receive));
    }

    public ClientStatus GetStatus()
    {
        return Status.Value;
    }

    public void Connect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Status.Value == ClientStatus.Connected) return;

            if (!_Connect() || !StartReceiving())
            {
                Disconnect();
                Status.Value = ClientStatus.Error;
                return;
            }

            Status.Value = ClientStatus.Connected;
            Dalamud.Log.Info($"ActWebSocket is connected");
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
            if (Status.Value == ClientStatus.Disconnected) return;

            var disconnectResult = _Disconnect();
            var stopReceivingResult = StopReceiving();

            if (!disconnectResult || !stopReceivingResult)
            {
                Status.Value = ClientStatus.Error;
                return;
            }

            Status.Value = ClientStatus.Disconnected;
            Dalamud.Log.Info($"ActWebSocket is disconnected");
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
            if (Status.Value != ClientStatus.Connected) return;
            Disconnect();
            Connect();
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
            if (Status.Value != ClientStatus.Connected) throw new Exception("Client is not connected.");

            var sendResult = Helpers.TimeOutTask((tokenSource) =>
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message.ToString());
                try
                {
                    Client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, tokenSource).Wait(tokenSource);
                }
                catch (OperationCanceledException) { }
            });
            sendResult.Wait();
            if (sendResult.IsFaulted) throw new Exception($"sendResult Task, exception:\n{sendResult.Exception.InnerException}");
            if (sendResult.Result == false) throw new Exception("sendResult Task, Timeout");
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ActWebSocket Send, Uri \'{Uri?.OriginalString ?? "null"}\', message \'{message}\':\n{ex}");
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
            Disconnect();
            ReceiverTask?.Dispose();
            Client?.Dispose();
        }
        finally
        {
            ClientLock.Dispose();
        }
    }

    private bool _Connect()
    {
        try
        {
            if (Uri == null) return false;
            var connectResult = Helpers.TimeOutTask((tokenSource) =>
            {
                try
                {
                    Client.ConnectAsync(Uri, tokenSource).Wait(tokenSource);
                }
                catch (OperationCanceledException) { }
            });
            connectResult.Wait();
            if (connectResult.IsFaulted) throw new Exception($"connectResult Task, exception:\n{connectResult.Exception.InnerException}");
            if (connectResult.Result == false) throw new Exception("connectResult Task, Timeout");
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ActWebSocket Connect, Uri \'{Uri?.OriginalString ?? "null"}\':\n{ex}");
            return false;
        }
        return true;
    }

    private bool _Disconnect()
    {
        try
        {
            if (Uri == null) return false;

            var disconnectResult = Helpers.TimeOutTask((tokenSource) =>
            {
                try
                {
                    Client.ConnectAsync(Uri, tokenSource).Wait(tokenSource);
                }
                catch (OperationCanceledException) { }
            });
            disconnectResult.Wait();
            if (disconnectResult.IsFaulted) throw new Exception($"disconnectResult Task, exception:\n{disconnectResult.Exception.InnerException}");
            if (disconnectResult.Result == false) throw new Exception("disconnectResult Task, Timeout");
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"ActWebSocket Disconnect, Uri \'{Uri?.OriginalString ?? "null"}\':\n{ex}");
            return false;
        }
        return true;
    }

    private bool StartReceiving()
    {
        lock (ReceiverCancelSourceLock)
        {
            if (ReceiverCancelSource != null) return false;
            ReceiverCancelSource = new CancellationTokenSource();
        }

        ReceiverTask = Task.Run(() =>
        {
            try
            {
                Receiver();
            }
            finally
            {
                lock (ReceiverCancelSourceLock)
                {
                    ReceiverCancelSource?.Dispose();
                    ReceiverCancelSource = null;
                }
            }
        });
        return true;
    }

    private bool StopReceiving()
    {
        lock (ReceiverCancelSourceLock)
        {
            if (ReceiverCancelSource == null) return false;
            ReceiverCancelSource.Cancel();
        }

        Dalamud.Log.Info("ActWebSocket StopReceiving is waiting cancellation");
        ReceiverTask?.GetAwaiter().GetResult();
        Dalamud.Log.Info("ActWebSocket StopReceiving cancellation done");
        return ReceiverTask?.IsFaulted ?? true;
    }

    private void Receiver()
    {
        try
        {
            while (!ReceiverCancelSource!.IsCancellationRequested)
            {
                byte[] buffer = new byte[1024];
                WebSocketReceiveResult result;
                StringBuilder builder = new();

                do
                {
                    result = Client.ReceiveAsync(new ArraySegment<byte>(buffer), ReceiverCancelSource.Token).Result;
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                } while (!result.EndOfMessage && !ReceiverCancelSource.IsCancellationRequested);

                Task.Run(() => Received(builder.ToString()));
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {

            Console.Error.WriteLine($"ActWebSocket Receiver:\n{ex}");

            ClientLock.WaitOne();
            try
            {
                if (Status.Value == ClientStatus.Connected) _Disconnect();
                Status.Value = ClientStatus.Error;
            }
            finally
            {
                ClientLock.ReleaseMutex();
            }
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
}