
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
using MeterWay.managers;

using System.Linq;

namespace MeterWay;

public class DataManager
{

    public List<Encounter> encounters;
    public Encounter current => encounters.Last();

    private bool lastcombatstate;

    public DataManager() 
    {
        this.encounters = new List<Encounter>();
        this.lastcombatstate = false;
    }

    void StartEncounter()
    {
        if (current != new Encounter())
        {
            EndEncounter();
        }

        //encounters.Add();
    }
    void EndEncounter()
    {
        // finalize the encounter
        this.current.End = DateTime.Now;
    }


    private bool GetInCombat()
    {
        if (PluginManager.Instance.PartyList.Length == 0)
        {
            return PluginManager.Instance.Condition[ConditionFlag.InCombat];
        }
        foreach (var player in PluginManager.Instance.PartyList)
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

    public void Receiver(JObject json)
    {

        PluginManager.Instance.ChatGui.Print("a");

        var combatstate = GetInCombat();

        // start combat
        if (combatstate == true && lastcombatstate == false)
        {

            PluginManager.Instance.PluginLog.Info("meterway detected start of combat");
            encounters.Add(new Encounter());
            lastcombatstate = true;
        }

        // end combat
        if (combatstate == false && lastcombatstate == true)
        {

            PluginManager.Instance.PluginLog.Info("meterway detected end of combat");
            //current.End();
            lastcombatstate = false;
        }

        // ignore all data when not in combat 
        if (combatstate == false)
        {
            return;
        }

        PluginManager.Instance.PluginLog.Info(json.ToString());

        // parse data
        try
        {
            var log = new IINACTNetworkLog(json);

            foreach (string val in log.data)
            {
                PluginManager.Instance.PluginLog.Info(val);
            }

        }
        catch (Exception ex)
        {
            PluginManager.Instance.PluginLog.Error(ex.ToString());
            return;
        }


        return;
    }



}