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

/// <summary>
/// Represents the status of the client.
/// </summary>
public enum ClientStatus
{
    /// <summary>
    /// Do not use, exclusive to ConnectionManager Status
    /// </summary>
    NotInitialized,
    
    Connected,
    Disconnected,
    Error,
}