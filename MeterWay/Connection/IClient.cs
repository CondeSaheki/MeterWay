using System;
using Newtonsoft.Json.Linq;

namespace MeterWay.Connection;

public interface IClient : IDisposable
{
    public ClientStatus GetStatus();
    public void Connect();
    public void Disconnect();
    public void Reconnect();
    public void Send(JObject message);
}

public enum ClientStatus
{
    NotInitialized, // do not use, exclusive to ConnectionManager Status
    Connected,
    Disconnected,
    Error,
}