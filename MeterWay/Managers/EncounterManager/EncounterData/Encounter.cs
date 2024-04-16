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

    public Heal HealDealt { get; set; }
    public Heal HealReceived { get; set; }

    public PerSecounds PerSecounds { get; set; }

    public Encounter()
    {
        Id = Helpers.CreateId();
        Name = GetName();
        Party = new EncounterParty(this);
        RawActions = [];

        DamageDealt = new Damage();
        DamageReceived = new Damage();
        HealDealt = new Heal();
        HealReceived = new Heal();

        PerSecounds = new PerSecounds();
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

        PerSecounds.Calculate(DamageDealt, DamageReceived, HealDealt, HealReceived, this);
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