using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;

using MeterWay.Managers;
using MeterWay.Utils;
using MeterWay.LogParser;

namespace MeterWay.Data;

public class Encounter
{
    public uint Id { get; set; }
    public string Name { get; set; }

    public bool Active => Start != null && End == null;
    public bool Finished => Start != null && End != null;

    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public TimeSpan Duration => _Duration();

    public List<LogLineData> RawActions { get; set; }

    public uint PartyListId { get; set; }
    public Dictionary<uint, Player> Players { get; set; }
    public Dictionary<uint, uint> Pets { get; set; }

    public DamageData Damage { get; set; }
    public DamageData DamageTaken { get; set; }

    public HealingData Healing { get; set; }
    public HealingData HealingTaken { get; set; }

    //calculated
    public float DamagePercent { get; set; } = 0;
    public float HealsPercent { get; set; } = 0;
    public float CritPercent { get; set; } = 0;
    public float DirecHitPercent { get; set; } = 0;
    public float DirectCritHitPercent { get; set; } = 0;
    public float Crithealspercent { get; set; } = 0;
    
    public float Dps { get; set; } = 0;
    public float Hps { get; set; } = 0;

    // constructor
    public Encounter()
    {
        this.Id = CreateId();
        this.PartyListId = CreateId();
        this.Name = GetEncounterName();
        this.Players = GetPlayers();
        this.Pets = [];
        this.Damage = new DamageData();
        this.DamageTaken = new DamageData();
        this.Healing = new HealingData();
        this.HealingTaken = new HealingData();
        this.DamagePercent = 0;
        this.HealsPercent = 0;
        this.CritPercent = 0;
        this.DirecHitPercent = 0;
        this.DirectCritHitPercent = 0;
        this.Crithealspercent = 0;
        this.Dps = 0;
        this.Hps = 0;
        this.RawActions = [];
    }

    private static uint CreateId()
    {
        return (uint)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
    }

    public void Parse()
    {
        foreach (var action in RawActions)
        {
            action.Parse();
            if (false)
            {
                // TODO 
            }
        }
    }

    private Dictionary<uint, Player> GetPlayers()
    {
        var tmpPlayers = new Dictionary<uint, Player>();
        var partyList = InterfaceManager.Inst.PartyList;
        if (partyList.Count() != 0)
        {
            foreach (var player in partyList)
            {
                if (player.GameObject == null) continue;

                var character = (Character)player.GameObject;
                tmpPlayers.Add(character.ObjectId, new Player(character, this));
            }
        }
        else
        {
            var localPlayer = InterfaceManager.Inst.ClientState.LocalPlayer;
            if (localPlayer != null)
            {
                var character = (Character)localPlayer;
                tmpPlayers.Add(character.ObjectId, new Player(character, this));
            }
        }
        return tmpPlayers;
    }

    public void UpdateParty() // vREALVERDADEIROULTIMATE
    {
        Helpers.Log("Party update got triggred");
        var partyList = InterfaceManager.Inst.PartyList;

        if (!PartyMembersChanged(partyList))
        {
            Helpers.Log("Party did not change, player got a DC or changed area");
            return;
        }
        Helpers.Log("Party has changed");

        // keep only you
        if (partyList.Count() == 0)
        {
            var you = InterfaceManager.Inst.ClientState.LocalPlayer;
            if (you == null) return; // you are null

            foreach (var player in Players)
            {
                if (player.Key != you.ObjectId)
                {
                    if (!this.Active) Players.Remove(player.Key);
                    else player.Value.IsActive = false;
                }
            }
            this.PartyListId = CreateId();
            Helpers.Log("Party updated");
            return; // done
        }

        List<PartyMember> newPlayers = [];
        List<uint> commonPlayers = [];

        // fill commonPlayers & newPlayers
        foreach (var player in partyList)
        {
            var tmpPlayerId = GetPartyMemberOrRecoverId(player);
            if (tmpPlayerId == null) continue;

            if (!Players.ContainsKey((uint)tmpPlayerId))
            {
                newPlayers.Add(player);
                continue;
            }
            else if (Players[(uint)tmpPlayerId].IsActive == false)
            {
                Players[(uint)tmpPlayerId].IsActive = true;
            }
            commonPlayers.Add((uint)tmpPlayerId);
        }
        if (commonPlayers.Count + newPlayers.Count == 0) return; // all players are null

        // remove players not in common
        foreach (var player in Players)
        {
            if (!commonPlayers.Contains(player.Key))
            {
                if (!this.Active)
                {
                    Players.Remove(player.Key);
                }
                else
                {
                    player.Value.IsActive = false;
                }
            }
        }

        // add new players 
        foreach (var player in newPlayers)
        {
            var character = player.GameObject as Character;
            if (character == null) continue; // error could not retrieve player

            Players.Add(character.ObjectId, new Player(character, this));
        }
        Helpers.Log("Party updated");
        this.PartyListId = CreateId();
    }

    public uint? GetPartyMemberOrRecoverId(PartyMember player)
    {
        if (player.GameObject != null) return player.GameObject.ObjectId; // no recover

        // attempt recover
        uint? recovered = null;
        foreach (KeyValuePair<uint, Player> cachedPlayer in Players)
        {
            if (cachedPlayer.Value.Name == player.Name.ToString() && cachedPlayer.Value.World == player.World.Id)
            {
                recovered = cachedPlayer.Key;
                break;
            }
        }
        return recovered;
    }

    public bool PartyMembersChanged(IPartyList partyList)
    {
        uint activeCount = 0;
        foreach (var player in Players) if (player.Value.IsActive) ++activeCount;

        // party change is evident
        if (partyList.Count() != activeCount)
        {
            if (!(partyList.Count() == 0 && activeCount == 1)) return true;
        }

        // party sizes are equal and need to be checked
        foreach (var player in partyList)
        {
            var recoveredId = GetPartyMemberOrRecoverId(player);
            if (recoveredId == null) return true; // id did not got recovered
            if (!Players.ContainsKey((uint)recoveredId)) return true; // new player
        }
        return false;
    }

    public void UpdateEncounter()
    {
        this.Name = GetEncounterName();
        var partyList = InterfaceManager.Inst.PartyList;
        if (PartyMembersChanged(partyList)) UpdateParty();
        // fix jobs and stuff from player
        foreach (var player in partyList)
        {
            if (Players.ContainsKey(player.ObjectId) && Players[player.ObjectId].IsActive)
            {
                Players[player.ObjectId].Name = player.Name.ToString();
                Players[player.ObjectId].World = player.World.Id;
                Players[player.ObjectId].Job = player.ClassJob.Id;
            }
        }

        // TODO
        // this.Pets = new Dictionary<uint, uint>();
    }

    private static string GetEncounterName()
    {
        var locationRow = InterfaceManager.Inst.DataManager.GetExcelSheet<TerritoryType>()?.GetRow(InterfaceManager.Inst.ClientState.TerritoryType);
        var instanceContentName = locationRow?.ContentFinderCondition.Value?.Name?.ToString();
        var placeName = locationRow?.PlaceName.Value?.Name?.ToString();

        return (string.IsNullOrEmpty(instanceContentName) ? placeName : instanceContentName) ?? "";
    }

    private TimeSpan _Duration()
    {
        if (Start == null) return new TimeSpan(0);
        if (End == null) return (TimeSpan)(DateTime.Now - Start);
        return (TimeSpan)(End - Start);
    }

    public void StartEncounter()
    {
        InterfaceManager.Inst.PluginLog.Info("Encounter started.");
        if (this.Start == null) this.Start = DateTime.Now; // Active = true
    }

    public void EndEncounter()
    {
        InterfaceManager.Inst.PluginLog.Info("Encounter ended.");
        if (this.End == null) this.End = DateTime.Now; // finished = true
    }

    public void RecalculateData()
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

        foreach (var player in this.Players.Values)
        {
            player.RecalculateData();
        }
    }

}