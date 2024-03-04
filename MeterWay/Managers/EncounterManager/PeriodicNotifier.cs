using System;
using System.Collections.Generic;
using System.Threading;

namespace MeterWay.Managers;

public class Notifier
{
    public List<Action> Clients { get; private init; }

    private bool TimerActive { get; set; }
    private Timer? TimerNotification { get; set; }
    private TimeSpan TimerNotificationInterval { get; set; }

    public Notifier(TimeSpan interval)
    {
        TimerNotificationInterval = interval;
        Clients = [];
        TimerActive = false;
    }

    public void ChangeNotificationInterval(TimeSpan interval)
    {
        if (TimerNotificationInterval == interval) return;
        TimerNotification?.Change(0, interval.Milliseconds);
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

    private void NotifyAll(object? state)
    {

        foreach (var client in Clients) client.Invoke();
    }

    public void StartTimer()
    {
        if (TimerActive) return;
        TimerNotification = new Timer(NotifyAll, null, 0, TimerNotificationInterval.Milliseconds);
        TimerActive = true;
    }

    public void StopTimer()
    {
        if (!TimerActive) return;
        TimerNotification!.Dispose();
        TimerActive = false;
        NotifyAll(null);
    }

}
