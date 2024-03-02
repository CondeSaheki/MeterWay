namespace MeterWay.Ipc;

public interface IIpcClient
{
    public void Connect();
    public void Disconnect();
    public void Reconnect();
    public bool Status();
}