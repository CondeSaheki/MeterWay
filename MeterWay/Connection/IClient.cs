using System;
using Newtonsoft.Json.Linq;

namespace MeterWay.Connection;

public interface IClient : IDisposable
{
    public void Connect();
    public void Disconnect();
    public void Reconnect();
    public void Send(JObject message);
    public ClientStatus GetStatus();
}

public enum ClientStatus
{
    None, // use in initialization
    Error,
    Disconnected,
    Connected,
    NotInitialized // dont use, exclusive to ConnectionManager Status
}