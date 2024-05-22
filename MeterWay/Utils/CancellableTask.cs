using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeterWay.Utils;

/// <summary>
/// wait task cancellation before doing anything
/// This was desingned to be used in ActWebSocket Client but was not needed keept here in case its needed some were else.
/// </summary>
public class TaskCancellation : IDisposable
{
    public Action<CancellationToken> RunningAction { get; private set; }
    public Task? RunningTask { get; private set; } = null;
    private CancellationTokenSource? CancelSource { get; set; } = null;
    private readonly object _tokenLock = new();

    public TaskCancellation(Action<CancellationToken> action)
    {
        RunningAction = action ?? throw new ArgumentNullException(nameof(action));
    }

    public void Cancel()
    {
        lock (_tokenLock)
        {
            CancelSource?.Cancel();
        }
        RunningTask?.GetAwaiter().GetResult();
    }

    public void Run()
    {
        lock (_tokenLock)
        {
            if (CancelSource != null) return;
            CancelSource = new CancellationTokenSource();
        }

        RunningTask = Task.Run(() =>
        {
            try
            {
                RunningAction.Invoke(CancelSource.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Dalamud.Log.Error($"TaskCancellation Run:\n{ex}");
            }
            finally
            {
                lock (_tokenLock)
                {
                    CancelSource?.Dispose();
                    CancelSource = null;
                }
            }
        });
    }

    public void Dispose()
    {
        Cancel();
        RunningTask?.Dispose();
    }
}