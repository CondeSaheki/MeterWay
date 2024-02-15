
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

namespace Meterway;

public class DataManager
{
    private Plugin plugin;
    public List<CombatData> Combat;
    private bool combatstate;

    public CombatData? CurrentCombatData;

    public DataManager(Plugin plugin)
    {
        this.plugin = plugin;
        this.Combat = new List<CombatData>();
        combatstate = plugin.Condition[ConditionFlag.InCombat];
    }

    private bool partyincomabat()
    {
        foreach (var player in this.plugin.PartyList)
        {
            if (player.GameObject == null) continue;

            var teste = (Dalamud.Game.ClientState.Objects.Types.Character)player.GameObject;
            if ((teste.StatusFlags & Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat) == Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat)
            {
                return true;
            }
        }
        return false;
    }

    public bool Receiver(JObject json)
    {
        bool combatstatevalue = false;

        if (this.plugin.PartyList.Length == 0)
        {
            combatstatevalue = plugin.Condition[ConditionFlag.InCombat];
        }
        else
        {
            combatstatevalue = partyincomabat();
        }

        // last data
        if (combatstate == true && combatstatevalue == false)
        {
            // end combat
            this.plugin.PluginLog.Info("meterway detected end of combat");
            combatstate = false;
        }

        if (combatstate == false && combatstatevalue == true)
        {
            // start combat
            this.plugin.PluginLog.Info("meterway detected start of combat");
            combatstate = true;
        }

        // ignore all data when not in combat 
        if (combatstatevalue == false)
        {
            return true;
        }

        // parse data
        try
        {
            //Combat.Add(new CombatData(plugin, json));
            CurrentCombatData = new CombatData(plugin, json);
            //plugin.PluginLog.Info("data: " + json.ToString());
            //plugin.PluginLog.Info("Meterway received data!");
        }
        catch (Exception ex)
        {
            plugin.PluginLog.Error(ex.ToString());
            return false;
        }
        return true;
    }

    public void RecapChatLinkHandler(uint cmdId, SeString msg)
    {
        plugin.PluginInterface.RemoveChatLinkHandler(cmdId);
        plugin.ChatGui.Print("RecapChatLinkHandler id: " + cmdId.ToString());

    }
}