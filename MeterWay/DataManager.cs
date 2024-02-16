
using System.IO;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Newtonsoft.Json.Linq;
using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;

using MeterWay;
using System.Linq;

namespace MeterWay;

public class DataManager
{
    private Plugin plugin;

    public List<Encounter> encounters;
    public Encounter current => encounters.Last();

    private bool lastcombatstate;

    public DataManager(Plugin plugin)
    {
        this.plugin = plugin;
        this.encounters = new List<Encounter>();
        this.lastcombatstate = false;
    }

    void StartEncounter()
    {
        if (current != new Encounter(plugin))
        {
            EndEncounter();
        }

        //encounters.Add();
    }
    void EndEncounter()
    {
        // finalize the encounter
        this.current.End = "";
    }


    private bool GetInCombat()
    {
        if (this.plugin.PartyList.Length == 0)
        {
            return plugin.Condition[ConditionFlag.InCombat];
        }
        foreach (var player in this.plugin.PartyList)
        {
            if (player.GameObject == null) continue;

            var character = (Dalamud.Game.ClientState.Objects.Types.Character)player.GameObject;
            if ((character.StatusFlags & Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat) == Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat)
            {
                return true;
            }
        }

        return false;
    }

    public bool Receiver(JObject json)
    {

        PluginManager.Instance.ChatGui.Print("a");

        var combatstate = GetInCombat();

        // start combat
        if (combatstate == true && lastcombatstate == false)
        {

            this.plugin.PluginLog.Info("meterway detected start of combat");
            encounters.Add(new Encounter(this.plugin));
            lastcombatstate = true;
        }

        // end combat
        if (combatstate == false && lastcombatstate == true)
        {

            this.plugin.PluginLog.Info("meterway detected end of combat");
            current.End();
            lastcombatstate = false;
        }

        // ignore all data when not in combat 
        if (combatstate == false)
        {
            return true;
        }

        plugin.PluginLog.Info(json.ToString());

        // parse data
        try
        {
            var log = new IINACTNetworkLog(json);

            foreach (string val in log.data)
            {
                plugin.PluginLog.Info(val);
            }

        }
        catch (Exception ex)
        {
            plugin.PluginLog.Error(ex.ToString());
            return false;
        }


        return true;
    }



}