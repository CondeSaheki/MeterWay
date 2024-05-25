using System;
using Newtonsoft.Json.Linq;
using Dalamud.Plugin.Ipc;
using System.Threading;

using MeterWay.Utils;

namespace MeterWay.Connection;

public class IinactIpc : IClient
{
    private Action<JObject> ReceiveAction { get; set; }
    private ThreadSafeEnum<ClientStatus> Status { get; set; } = new(ClientStatus.Disconnected);
    private ICallGateProvider<JObject, bool>? ReceiverIpc { get; set; }

    private const string MeterWayReceiver = "MeterWay.Receiver";
    private const string IinactReceiver = "IINACT.IpcProvider." + MeterWayReceiver;
    private const string IinactUnsubscribe = "IINACT.Unsubscribe";
    private const string IinactSubscribe = "IINACT.CreateSubscriber";

    private readonly Mutex ClientLock = new();

    public IinactIpc(Action<JObject> receive)
    {
        ReceiveAction = receive ?? throw new ArgumentNullException(nameof(receive));
    }

    public ClientStatus GetStatus() => Status.Value;

    public void Connect()
    {
        ClientLock.WaitOne();
        try
        {
            if (Status.Value == ClientStatus.Connected) return;

            if (!Register() || !Subscribe())
            {
                Unregister();
                Status.Value = ClientStatus.Error;
                return;
            }

            Status.Value = ClientStatus.Connected;
            Dalamud.Log.Info($"IinactIpc is connected");
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

            bool unsubscribeResult = Unsubscribe();
            bool unregisterResult = Unregister();

            if (!unsubscribeResult || !unregisterResult)
            {
                Status.Value = ClientStatus.Error;
                return;
            }

            Status.Value = ClientStatus.Disconnected;
            Dalamud.Log.Info($"IinactIpc is disconnected");
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
            if (Status.Value != ClientStatus.Connected) throw new Exception("IinactIpc is not connected.");

            Dalamud.PluginInterface.GetIpcSubscriber<JObject, bool>(IinactReceiver).InvokeAction(message);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"IinactIpc Send, message '{message}':\n{ex}");
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
            if (Status.Value != ClientStatus.Disconnected) Disconnect();
        }
        finally
        {
            ClientLock.Dispose();
        }
    }

    private bool Receiver(JObject json)
    {
        try
        {
            ReceiveAction.Invoke(json);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"IinactIpc Receiver, json '{json}':\n{ex}");
        }
        return true;
    }

    private bool Register()
    {
        try
        {
            ReceiverIpc = Dalamud.PluginInterface.GetIpcProvider<JObject, bool>(MeterWayReceiver);
            ReceiverIpc.RegisterFunc(Receiver);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"IinactIpc Register:\n{ex}");
            ReceiverIpc = null;
            return false;
        }
        return true;
    }

    private bool Unregister()
    {
        try
        {
            ReceiverIpc?.UnregisterFunc();
            ReceiverIpc = null;
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"IinactIpc Unregister:\n{ex}");
            return false;
        }
        return true;
    }

    private static bool Unsubscribe()
    {
        try
        {
            var unsubscribeIpc = Dalamud.PluginInterface.GetIpcSubscriber<string, bool>(IinactUnsubscribe);
            if (!unsubscribeIpc.InvokeFunc(MeterWayReceiver))
            {
                throw new Exception("Could not unsubscribe receiver");
            }
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"IinactIpc Unsubscribe:\n{ex}");
            return false;
        }
        return true;
    }

    private static bool Subscribe()
    {
        try
        {
            var subscribeIpc = Dalamud.PluginInterface.GetIpcSubscriber<string, bool>(IinactSubscribe);
            if (!subscribeIpc.InvokeFunc(MeterWayReceiver))
            {
                throw new Exception("Could not subscribe receiver");
            }
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"IinactIpc Subscribe:\n{ex}");
            return false;
        }
        return true;
    }
}