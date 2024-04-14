using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MeterWay;

public class WebSocketClient : IDisposable
{
    private ClientWebSocket Client { get; set; }
    private CancellationTokenSource Cancel { get; set; }

    public event EventHandler Receiver = delegate { };

    public WebSocketClient()
    {
        Client = new();
        Cancel = new();
        Uri uri = new("ws://echo.websocket.org");
        Connect(uri, TimeSpan.FromSeconds(5)).Wait();
        SendMessage("").Wait();
        // ReceiveMessages();
        // ReceiveMessages();
    }

    private async Task Connect(Uri uri, TimeSpan duration)
    {
        try
        {
            var connectTask = Client.ConnectAsync(uri, Cancel.Token);
            var timeoutTask = Task.Delay((int)duration.TotalMilliseconds);
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Cancel.Cancel();
                throw new Exception("Connection timeout.");
            }

            await connectTask;
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error("Error connecting to WebSocket server.", ex);
        }
    }

    private async Task Disconnect(TimeSpan duration)
    {
        if (!Status() || Cancel.Token.IsCancellationRequested) return;

        try
        {
            var closeTask = Client.CloseAsync(WebSocketCloseStatus.NormalClosure, null, Cancel.Token);
            var timeoutTask = Task.Delay((int)duration.TotalMilliseconds);
            var completedTask = await Task.WhenAny(closeTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Cancel.Cancel();
                throw new Exception("Connection timeout.");
            }
            await closeTask;
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error("Error connecting to WebSocket server.", ex);
        }
        finally
        {
            Dispose();
        }
    }

    private async Task SendMessage(string message)
    {
        if (!Status() || Cancel.Token.IsCancellationRequested) return;

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await Client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, Cancel.Token);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error("Error sending message to WebSocket server.", ex);
        }
    }

    private async Task ReceiveMessages()
    {
        if (!Status() || Cancel.Token.IsCancellationRequested) return;
        
        try
        {
            while (!Cancel.Token.IsCancellationRequested)
            {
                byte[] buffer = new byte[1024];
                WebSocketReceiveResult result;
                StringBuilder builder = new();

                do
                {
                    result = await Client.ReceiveAsync(new ArraySegment<byte>(buffer), Cancel.Token);
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                } while (!result.EndOfMessage && !Cancel.Token.IsCancellationRequested);

                builder.ToString();
            }
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error("Error receiving messages to WebSocket server.", ex);
        }
    }


    public bool Status() => Client.State == WebSocketState.Open;


    public void Dispose()
    {
        Cancel.Cancel();
        Client?.Dispose();
    }
}