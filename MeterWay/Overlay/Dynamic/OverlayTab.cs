using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Overlay;

namespace Dynamic;

public partial class Overlay : IOverlay, IOverlayTab
{
    public void DrawTab()
    {
        using var bar = ImRaii.TabBar("Overlay Settings Tabs");
        if (!bar) return;
        DrawGeneral();
        DrawScriptTab();
    }

    void DrawScriptTab()
    {
        if (Script != null)
        {
            using var tab = ImRaii.TabItem("Lua Script");
            if (!tab) return;

            Script?.ExecuteDrawTab();
        }
    }

    public void DrawGeneral()
    {
        using var tab = ImRaii.TabItem("General");
        if (!tab) return;

        var loadInitValue = Config.LoadInit;
        if (ImGui.Checkbox("Load on init", ref loadInitValue))
        {
            Config.LoadInit = loadInitValue;
            File.Save(Name, Config);
        }

        var scriptNames = Config.Scripts.Select(x => x.Key).ToArray();
        int scriptIndexValue = Config.ScriptName == null ? 0 : Array.FindIndex(scriptNames, x => x == Config.ScriptName);
        if (scriptIndexValue == -1) scriptIndexValue = 0;
        ImGui.PushItemWidth(160);
        if (ImGui.Combo("Lua Script", ref scriptIndexValue, scriptNames, Config.Scripts.Count))
        {
            Config.ScriptName = scriptNames[scriptIndexValue];
            File.Save(Name, Config);
        }
        ImGui.PopItemWidth();
        ImGui.SameLine();
        if (Script != null)
        {
            if (ImGui.Button("Reload")) Script.Reload();
        }
        else
        {
            if (ImGui.Button("Load")) LoadScript();
        }
        ImGui.SameLine();
        if (Script == null) ImGui.BeginDisabled();
        if (ImGui.Button("Unload"))
        {
            Script?.Dispose();
            Script = null;
        }
        if (Script == null) ImGui.EndDisabled();

        if (ImGui.Button("Add"))
        {
            FileDialogManager fileDialogManager = new();
            fileDialogManager.OpenFileDialog("Chose Script", ".lua",
            (bool isOk, List<string> selectedFolder) =>
            {
                if (isOk)
                {
                    var file = new System.IO.FileInfo(selectedFolder.First());
                    if (file.Exists)
                    {
                        Config.Scripts.Add(file.Name, file.FullName);
                        File.Save(Name, Config);

                        MeterWay.Dalamud.Log.Info($"FileDialogManager completed:\n{selectedFolder}");
                    }
                }

                MeterWay.Dalamud.PluginInterface.UiBuilder.Draw -= fileDialogManager.Draw;
            }, 1, null, true);
            MeterWay.Dalamud.PluginInterface.UiBuilder.Draw += fileDialogManager.Draw;
        }
        ImGui.SameLine();
        if (ImGui.Button("Remove"))
        {
            if (Config.ScriptName != null)
            {
                if (Script != null)
                {
                    Script.Dispose();
                    Script = null;
                }
                Config.Scripts.Remove(Config.ScriptName);
                File.Save(Name, Config);
            }
        }
    }
}