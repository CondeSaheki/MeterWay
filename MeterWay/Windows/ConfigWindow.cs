using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using System.Collections.Generic;

using MeterWay.managers;
using MeterWay.IINACT;

namespace MeterWay.Windows;

public class ConfigWindow : Window, IDisposable
{
    private IINACTIpcClient iinactclient;

    public ConfigWindow(IINACTIpcClient iinactclient) : base(
        "MeterWay Configurations",
        ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse) // ImGuiWindowFlags.NoResize | 
    {
        this.Size = new Vector2(400, 400);
        this.SizeCondition = ImGuiCond.Always;
        this.iinactclient = ;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var bar = ImRaii.TabBar("SettingsTabs");
        if (!bar) return;

        DrawGeneralTab();
        DrawOverlayTab();
        DrawAppearenceTab();
        DrawIINACTTab();
        DrawAboutTab();
    }

    private void DrawGeneralTab()
    {
        using var tab = ImRaii.TabItem("General");
        if (!tab) return;

        ImGui.Text("WIP");

        /*
        // a
        ImGui.Text("On end of Combats");
        ImGui.PushItemWidth(130f);
        string[] interactions = { "Open recap", "Show Popup", "Chat Message", "Do nothing" };
        var InteractionValue = PluginManager.Instance.Configuration.Interaction;
        if (ImGui.Combo(" ", ref InteractionValue, interactions, interactions.Length))
        {
            PluginManager.Instance.Configuration.Interaction = InteractionValue;
            PluginManager.Instance.Configuration.Save();
        }
        ImGui.PopItemWidth();

        // b
        var SaveCombatsValue = PluginManager.Instance.Configuration.SaveCombats;
        if (ImGui.Checkbox("Save last", ref SaveCombatsValue))
        {
            PluginManager.Instance.Configuration.SaveCombats = SaveCombatsValue;
            PluginManager.Instance.Configuration.Save();
        }
        ImGui.SameLine();
        ImGui.PushItemWidth(80f);
        var CombatsValue = PluginManager.Instance.Configuration.Combats;
        if (ImGui.InputInt("combat(s)", ref CombatsValue) && CombatsValue > 0)
        {
            PluginManager.Instance.Configuration.Combats = CombatsValue;
            PluginManager.Instance.Configuration.Save();
        }
        ImGui.PopItemWidth();

        // c
        var CombatCloseValue = PluginManager.Instance.Configuration.CombatClose;
        if (ImGui.Checkbox("Automatically close when in combat", ref CombatCloseValue))
        {
            PluginManager.Instance.Configuration.CombatClose = CombatCloseValue;
            PluginManager.Instance.Configuration.Save();
        }

        // d
        var PvPValue = PluginManager.Instance.Configuration.PvP;
        if (ImGui.Checkbox("Disable in PVP", ref PvPValue))
        {
            PluginManager.Instance.Configuration.PvP = PvPValue;
            PluginManager.Instance.Configuration.Save();
        }

        // e
        var OverlayValue = PluginManager.Instance.Configuration.Overlay;
        if (ImGui.Checkbox("Overlay Mode", ref PvPValue))
        {
            PluginManager.Instance.Configuration.Overlay = OverlayValue;
            PluginManager.Instance.Configuration.Save();
        }
        */
    }

    private void DrawOverlayTab()
    {
        using var tab = ImRaii.TabItem("Overlay");
        if (!tab) return;

        //Overlay
        var OverlayValue = ConfigurationManager.Instance.Configuration.Overlay;
        if (ImGui.Checkbox("Show Overlay", ref OverlayValue))
        {
            ConfigurationManager.Instance.Configuration.Overlay = OverlayValue;
            ConfigurationManager.Instance.Configuration.Save();
            if (OverlayValue)
            {
                WindowSystem.AddWindow(PluginManager.Instance.OverlayWindow);
            }
            else
            {
                WindowSystem.RemoveWindow(PluginManager.Instance.OverlayWindow);
            }
        }

        // OverlayClickThrough
        var OverlayClickThroughValue = ConfigurationManager.Instance.Configuration.OverlayClickThrough;
        if (ImGui.Checkbox("Click through", ref OverlayClickThroughValue))
        {
            ConfigurationManager.Instance.Configuration.OverlayClickThrough = OverlayClickThroughValue;
            ConfigurationManager.Instance.Configuration.Save();
            OverlayWindow.Flags = PluginManager.Instance.OverlayWindow.Gerateflags();
            
        }

        // OverlayBackground
        var OverlayBackgroundValue = PluginManager.Instance.Configuration.OverlayBackground;
        if (ImGui.Checkbox("Background", ref OverlayBackgroundValue))
        {
            ConfigurationManager.Instance.Configuration.OverlayBackground = OverlayBackgroundValue;
            ConfigurationManager.Instance.Configuration.Save();
            OverlayWindow.Flags = PluginManager.Instance.OverlayWindow.Gerateflags();
        }

        if (OverlayBackgroundValue)
        {
            // OverlayBackgroundColor
            var OverlayBackgroundColorValue = ConfigurationManager.Instance.Configuration.OverlayBackgroundColor;
            ImGui.Text("background color: ");
            ImGui.SameLine();

            if (ImGui.ColorEdit4("background color", ref OverlayBackgroundColorValue,
                ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault))
            {
                ConfigurationManager.Instance.Configuration.OverlayBackgroundColor = OverlayBackgroundColorValue;
                ConfigurationManager.Instance.Configuration.Save();
            }
        }
        // text config
        // OverlayFontPath


        // /data/Inter-Bold.ttf


        // ImGui.Text("Font file path: ");
        // ImGui.SameLine();
        // var OverlayFontPathValue = PluginManager.Instance.Configuration.OverlayFontPath;
        // if (ImGui.InputText("FontFile", ref OverlayFontPathValue, 1000))
        // {
        //     PluginManager.Instance.Configuration.OverlayFontPath = OverlayFontPathValue;
        //     PluginManager.Instance.Configuration.Save();
        //     PluginManager.Instance.OverlayWindow.updatefont();
        // }

        // // OverlayFontSizeValue
        // var OverlayFontSizeValue = PluginManager.Instance.Configuration.OverlayFontSize;
        // ImGui.Text("Font size: ");
        // ImGui.SameLine();
        // if (ImGui.SliderFloat("FontSize", ref OverlayFontSizeValue, 0.0f, 64.0f, "%.1f", ImGuiSliderFlags.None))
        // {
        //     PluginManager.Instance.Configuration.OverlayFontSize = OverlayFontSizeValue;
        //     PluginManager.Instance.Configuration.Save();
        //     PluginManager.Instance.OverlayWindow.updatefont();
        // }

        // // OverlayFontScaleValue
        var OverlayFontScaleValue = ConfigurationManager.Instance.Configuration.OverlayFontScale;
        ImGui.Text("Font scale: ");
        ImGui.SameLine();
        if (ImGui.SliderFloat("##FontScale", ref OverlayFontScaleValue, 0.1f, 5.0f, "%.1f", ImGuiSliderFlags.None))
        {
            ConfigurationManager.Instance.Configuration.OverlayFontScale = OverlayFontScaleValue;
            ConfigurationManager.Instance.Configuration.Save();
            
            OverlayWindow.updatefont();
        }
    }


    private void DrawAppearenceTab()
    {
        using var tab = ImRaii.TabItem("Appearence");
        if (!tab) return;

        ImGui.Text("WIP");
    }

    private void DrawIINACTTab()
    {
        using var tab = ImRaii.TabItem("IINACT");
        if (!tab) return;

        var status = PluginManager.Instance.IpcClient.Status();

        //a
        ImGui.Text("State: ");
        ImGui.SameLine();
        ImGui.Text(status ? "Connected" : "Disconnected");

        //b
        ImGui.Separator();
        if (ImGui.Button(status ? "Restart" : "Start"))
        {
            if (status)
            {
                PluginManager.Instance.IpcClient.Reconnect();
            }
            else
            {
                PluginManager.Instance.IpcClient.Connect();
            }
        }

        if (!status) ImGui.BeginDisabled();

        ImGui.SameLine();
        if (ImGui.Button("Stop"))
        {
            PluginManager.Instance.IpcClient.Disconnect();
        }
        ImGui.EndDisabled();
    }

    private void DrawAboutTab()
    {
        using var tab = ImRaii.TabItem("About");
        if (!tab) return;

        ImGui.Text("WIP");
    }
}
