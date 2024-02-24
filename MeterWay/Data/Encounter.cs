using System;
using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;

using MeterWay.Managers;
using MeterWay.Utils;
using MeterWay.LogParser;

namespace MeterWay.Data;

public class Encounter
{
    public uint Id { get; set; }
    public string Name { get; set; }

    public bool Active => Begin != null && End == null;
    public bool Finished => Begin != null && End != null;

    public DateTime? Begin { get; set; }
    public DateTime? End { get; set; }
    public TimeSpan Duration => _Duration();

    public List<LogLineData> RawActions { get; set; }

    public EncounterParty Party;
    public Dictionary<uint, Player> Players => Party.Players;

    public DamageData Damage { get; set; }
    public DamageData DamageTaken { get; set; }

    public HealingData Healing { get; set; }
    public HealingData HealingTaken { get; set; }

    // Calculated
    public float DamagePercent { get; set; } = 0;
    public float HealsPercent { get; set; } = 0;
    public float CritPercent { get; set; } = 0;
    public float DirecHitPercent { get; set; } = 0;
    public float DirectCritHitPercent { get; set; } = 0;
    public float Crithealspercent { get; set; } = 0;

    public float Dps { get; set; } = 0;
    public float Hps { get; set; } = 0;

    public Encounter()
    {
        Id = Helpers.CreateId();
        Name = GetName();
        Party = new EncounterParty(this);
        RawActions = [];

        Damage = new DamageData();
        DamageTaken = new DamageData();
        Healing = new HealingData();
        HealingTaken = new HealingData();

        DamagePercent = 0;
        HealsPercent = 0;
        CritPercent = 0;
        DirecHitPercent = 0;
        DirectCritHitPercent = 0;
        Crithealspercent = 0;
        Dps = 0;
        Hps = 0;
    }

    public void Start()
    {
        InterfaceManager.Inst.PluginLog.Info("Encounter started.");
        if (Begin == null) Begin = DateTime.Now; // Active = true
    }

    public void Stop()
    {
        InterfaceManager.Inst.PluginLog.Info("Encounter ended.");
        if (End == null) End = DateTime.Now; // finished = true
    }

    public void Calculate()
    {
        var seconds = Duration.TotalSeconds <= 1 ? 1 : Duration.TotalSeconds; // overflow protection
        Dps = (float)(Damage.Total / seconds);
        Hps = (float)(Healing.Total / seconds);

        DamagePercent = Damage.Total != 0 ? (Damage.Total * 100 / Damage.Total) : 0;
        HealsPercent = Healing.Total != 0 ? (Healing.Total * 100 / Healing.Total) : 0;

        CritPercent = Damage.Count.Hit != 0 ? (Damage.Count.Crit * 100 / Damage.Count.Hit) : 0;
        DirecHitPercent = Damage.Count.Hit != 0 ? (Damage.Count.Dh * 100 / Damage.Count.Hit) : 0;
        DirectCritHitPercent = Damage.Count.Hit != 0 ? (Damage.Count.CritDh * 100 / Damage.Count.Hit) : 0;
        Crithealspercent = Healing.Count.Hit != 0 ? (Healing.Count.Crit * 100 / Healing.Count.Hit) : 0;

        foreach (var player in Party.Players.Values)
        {
            player.Calculate();
        }
    }

    public void Parse()
    {
        foreach (var action in RawActions) action.Parse();
        // TODO 
    }

    public bool Update()
    {
        bool changed = false;
        var tmpName = GetName();
        if (Name != tmpName)
        {
            changed = true;
            Name = tmpName;
        }
        if (Party.HasChanged())
        {
            changed = true;
            Party.Update();
        }
        else
        {
            foreach (Player player in Party.Players.Values)
            {
                if (player.Update()) changed = true;
            }
        }

        if (changed) Id = Helpers.CreateId();
        return changed;
    }

    private static string GetName()
    {
        var locationRow = InterfaceManager.Inst.DataManager.GetExcelSheet<TerritoryType>()?.GetRow(InterfaceManager.Inst.ClientState.TerritoryType);
        var instanceContentName = locationRow?.ContentFinderCondition.Value?.Name?.ToString();
        var placeName = locationRow?.PlaceName.Value?.Name?.ToString();

        return (string.IsNullOrEmpty(instanceContentName) ? placeName : instanceContentName) ?? "";
    }

    private TimeSpan _Duration()
    {
        if (Begin == null) return new TimeSpan(0);
        if (End == null) return (TimeSpan)(DateTime.Now - Begin);
        return (TimeSpan)(End - Begin);
    }
}