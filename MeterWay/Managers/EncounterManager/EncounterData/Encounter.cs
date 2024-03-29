using System;
using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;

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

    public Damage DamageDealt { get; set; }
    public Damage DamageReceived { get; set; }

    public HealValue HealDealt { get; set; }
    public HealValue HealReceived { get; set; }

    public float Dps { get; set; } = 0;
    public float Hps { get; set; } = 0;

    public Encounter()
    {
        Id = Helpers.CreateId();
        Name = GetName();
        Party = new EncounterParty(this);
        RawActions = [];

        DamageDealt = new Damage();
        DamageReceived = new Damage();
        HealDealt = new HealValue();
        HealReceived = new HealValue();

        Dps = 0;
        Hps = 0;
    }

    public void Start()
    {
        Dalamud.Log.Info("Encounter started.");
        if (Begin == null) Begin = DateTime.Now; // Active = true
    }

    public void Stop()
    {
        Dalamud.Log.Info("Encounter ended.");
        if (End == null) End = DateTime.Now; // finished = true
    }

    public void Calculate()
    {
        Party.Calculate();
        DamageDealt.Calculate();
        DamageReceived.Calculate();
        HealDealt.Calculate();
        HealReceived.Calculate();
        
        // var seconds = Duration.TotalSeconds <= 1 ? 1 : Duration.TotalSeconds; // overflow protection
        // Dps = (float)(DamageDealt.Total / seconds);
        // Hps = (float)(HealDealt.Total / seconds); 
    }

    public void Parse() { foreach (var action in RawActions) LoglineParser.Parse(action, this); }

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
        var locationRow = Dalamud.GameData.GetExcelSheet<TerritoryType>()?.GetRow(Dalamud.ClientState.TerritoryType);
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