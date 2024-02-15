
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

    public bool Receiver(JObject json)
    {
        if (plugin.Condition[ConditionFlag.InCombat] == false)
        {
            return true;
        }

        // parse data
        try
        {
            CurrentCombatData = new CombatData(plugin, json);
        }

        catch (Exception ex)
        {
            plugin.PluginLog.Error(ex.ToString());
            return false;
        }

        return true;
    }
    // wip create chat link notifications
    public void RecapChatLinkCreate()
    {
        var chatLinkPayload = plugin.PluginInterface.AddChatLinkHandler(0, RecapChatLinkHandler);
        var teste = new SeString(chatLinkPayload, new UIForegroundPayload(578), new TextPayload("Description Show damage Recap"), new UIForegroundPayload(0), RawPayload.LinkTerminator);

        plugin.ChatGui.Print(teste);
    }

    public void RecapChatLinkHandler(uint cmdId, SeString msg)
    {
        plugin.PluginInterface.RemoveChatLinkHandler(cmdId);
        plugin.ChatGui.Print("RecapChatLinkHandler id: " + cmdId.ToString());

    }
}