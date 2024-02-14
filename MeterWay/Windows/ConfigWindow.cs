using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using System.Collections.Generic;

namespace Meterway.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Plugin plugin;

    public ConfigWindow(Plugin plugin) : base(
        "MeterWay Configurations",
        ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse) // ImGuiWindowFlags.NoResize | 
    {
        this.Size = new Vector2(400, 400);
        this.SizeCondition = ImGuiCond.Always;

        this.plugin = plugin;
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
        var InteractionValue = this.plugin.Configuration.Interaction;
        if (ImGui.Combo(" ", ref InteractionValue, interactions, interactions.Length))
        {
            this.plugin.Configuration.Interaction = InteractionValue;
            this.plugin.Configuration.Save();
        }
        ImGui.PopItemWidth();

        // b
        var SaveCombatsValue = this.plugin.Configuration.SaveCombats;
        if (ImGui.Checkbox("Save last", ref SaveCombatsValue))
        {
            this.plugin.Configuration.SaveCombats = SaveCombatsValue;
            this.plugin.Configuration.Save();
        }
        ImGui.SameLine();
        ImGui.PushItemWidth(80f);
        var CombatsValue = this.plugin.Configuration.Combats;
        if (ImGui.InputInt("combat(s)", ref CombatsValue) && CombatsValue > 0)
        {
            this.plugin.Configuration.Combats = CombatsValue;
            this.plugin.Configuration.Save();
        }
        ImGui.PopItemWidth();

        // c
        var CombatCloseValue = this.plugin.Configuration.CombatClose;
        if (ImGui.Checkbox("Automatically close when in combat", ref CombatCloseValue))
        {
            this.plugin.Configuration.CombatClose = CombatCloseValue;
            this.plugin.Configuration.Save();
        }

        // d
        var PvPValue = this.plugin.Configuration.PvP;
        if (ImGui.Checkbox("Disable in PVP", ref PvPValue))
        {
            this.plugin.Configuration.PvP = PvPValue;
            this.plugin.Configuration.Save();
        }

        // e
        var OverlayValue = this.plugin.Configuration.Overlay;
        if (ImGui.Checkbox("Overlay Mode", ref PvPValue))
        {
            this.plugin.Configuration.Overlay = OverlayValue;
            this.plugin.Configuration.Save();
        }
        */
    }

    private void DrawOverlayTab()
    {
        using var tab = ImRaii.TabItem("Overlay");
        if (!tab) return;

        //Overlay
        var OverlayValue = this.plugin.Configuration.Overlay;
        if (ImGui.Checkbox("Show Overlay", ref OverlayValue))
        {
            this.plugin.Configuration.Overlay = OverlayValue;
            this.plugin.Configuration.Save();
            if (OverlayValue)
            {
                this.plugin.WindowSystem.AddWindow(this.plugin.OverlayWindow);
            }
            else
            {
                this.plugin.WindowSystem.RemoveWindow(this.plugin.OverlayWindow);
            }
        }

        if (!this.plugin.Configuration.Overlay) ImGui.BeginDisabled();

        // OverlayClickThrough
        var OverlayClickThroughValue = this.plugin.Configuration.OverlayClickThrough;
        if (ImGui.Checkbox("Click through", ref OverlayClickThroughValue))
        {
            this.plugin.Configuration.OverlayClickThrough = OverlayClickThroughValue;
            this.plugin.Configuration.Save();
            this.plugin.OverlayWindow.Flags = this.plugin.OverlayWindow.Gerateflags();
        }

        // OverlayBackground
        var OverlayBackgroundValue = this.plugin.Configuration.OverlayBackground;
        if (ImGui.Checkbox("Background", ref OverlayBackgroundValue))
        {
            this.plugin.Configuration.OverlayBackground = OverlayBackgroundValue;
            this.plugin.Configuration.Save();
            this.plugin.OverlayWindow.Flags = this.plugin.OverlayWindow.Gerateflags();
        }

        ImGui.EndDisabled();
        if (!this.plugin.Configuration.OverlayBackground) ImGui.BeginDisabled();

        // OverlayBackgroundColor
        var OverlayBackgroundColorValue = this.plugin.Configuration.OverlayBackgroundColor;
        ImGui.Text("background color: ");
        ImGui.SameLine();

        if (ImGui.ColorEdit4("background color", ref OverlayBackgroundColorValue,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault))
        {
            this.plugin.Configuration.OverlayBackgroundColor = OverlayBackgroundColorValue;
            this.plugin.Configuration.Save();
        }

        ImGui.EndDisabled();
        if (!this.plugin.Configuration.Overlay) ImGui.BeginDisabled();

        // text config
        // OverlayFontPath


        // /data/Inter-Bold.ttf


        ImGui.Text("Font file path: ");
        ImGui.SameLine();
        var OverlayFontPathValue = this.plugin.Configuration.OverlayFontPath;
        if (ImGui.InputText("FontFile", ref OverlayFontPathValue, 1000))
        {
            this.plugin.Configuration.OverlayFontPath = OverlayFontPathValue;
            this.plugin.Configuration.Save();
            this.plugin.OverlayWindow.updatefont();
        }

        // OverlayFontSizeValue
        var OverlayFontSizeValue = this.plugin.Configuration.OverlayFontSize;
        ImGui.Text("Font size: ");
        ImGui.SameLine();
        if (ImGui.SliderFloat("FontSize", ref OverlayFontSizeValue, 0.0f, 64.0f, "%.1f", ImGuiSliderFlags.None))
        {
            this.plugin.Configuration.OverlayFontSize = OverlayFontSizeValue;
            this.plugin.Configuration.Save();
            this.plugin.OverlayWindow.updatefont();
        }
        ImGui.EndDisabled();
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

        var status = this.plugin.IpcClient.Status();

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
                this.plugin.IpcClient.Reconnect();
            }
            else
            {
                this.plugin.IpcClient.Connect();
            }
        }

        if (!status) ImGui.BeginDisabled();

        ImGui.SameLine();
        if (ImGui.Button("Stop"))
        {
            this.plugin.IpcClient.Disconnect();
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
