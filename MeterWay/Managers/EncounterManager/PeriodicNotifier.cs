using System;
using System.Threading;

namespace MeterWay.Managers;

public class Notifier
{
    public event EventHandler OnDataUpdate = delegate { };

    private bool TimerActive { get; set; }
    private Timer? TimerNotification { get; set; }
    private TimeSpan TimerNotificationInterval { get; set; }

    public Notifier(TimeSpan interval)
    {
        TimerNotificationInterval = interval;
        TimerActive = false;
    }

    public void ChangeNotificationInterval(TimeSpan interval)
    {
        if (TimerNotificationInterval == interval) return;
        TimerNotification?.Change(0, (int)interval.TotalMilliseconds);
        TimerNotificationInterval = interval;
    }

    public void Notify()
    {
        if (ConfigurationManager.Inst.Configuration.OverlayRealtimeUpdate)
        {
            StartTimer();
            return;
        }
        if (EncounterManager.Inst.CurrentEncounter().Finished) return;
        NotifyAll(null);
    }

    private void NotifyAll(object? _) => OnDataUpdate?.Invoke(this, EventArgs.Empty);

    public void StartTimer()
    {
        if (TimerActive) return;
        TimerNotification = new Timer(NotifyAll, null, 0, (int)TimerNotificationInterval.TotalMilliseconds);
        TimerActive = true;
    }

    public void StopTimer()
    {
        if (!TimerActive) return;
        TimerNotification?.Dispose();
        TimerActive = false;
        NotifyAll(null);
    }

}
