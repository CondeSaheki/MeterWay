using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Overlay;

namespace Mogu;

[Serializable]
public class JobColors
{
    public uint Default { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0.8549f, 0.6157f, 0.1804f, 0.25f));

    public bool Override { get; set; } = false;
    public float Opacity { get; set; } = 0.25f;

    public uint Tank { get; set; } = ImGui.ColorConvertFloat4ToU32(new(48f / 255f, 58f / 255f, 124f / 255f, 0.25f));
    public uint Healer { get; set; } = ImGui.ColorConvertFloat4ToU32(new(64f / 255f, 101f / 255f, 45f / 255f, 0.25f));
    public uint MeleeDps { get; set; } = ImGui.ColorConvertFloat4ToU32(new(106f / 255f, 45f / 255f, 43f / 255f, 0.25f));
    public uint PhysicalRangedDps { get; set; } = ImGui.ColorConvertFloat4ToU32(new(106f / 255f, 45f / 255f, 43f / 255f, 0.25f));
    public uint MagicalRangedDps { get; set; } = ImGui.ColorConvertFloat4ToU32(new(106f / 255f, 45f / 255f, 43f / 255f, 0.25f));

    // Tank

    public uint Paladin { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.6588f, 0.8235f, 0.902f, 0.25f));
    public uint Warrior { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.8118f, 0.149f, 0.1294f, 0.25f));
    public uint DarkKnight { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.8196f, 0.149f, 0.8f, 0.25f));
    public uint GunBreaker { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.4745f, 0.4275f, 0.1882f, 0.25f));

    // MeleeDPS

    public uint Monk { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.8392f, 0.6118f, 0f, 0.25f));
    public uint Dragoon { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.2549f, 0.3922f, 0.8039f, 0.25f));
    public uint Ninja { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.6863f, 0.098f, 0.3922f, 0.25f));
    public uint Samurai { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.8941f, 0.4275f, 0.0157f, 0.25f));
    public uint Reaper { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.5882f, 0.3529f, 0.5647f, 0.25f));

    // Healer

    public uint WhiteMage { get; set; } = ImGui.ColorConvertFloat4ToU32(new(1f, 0.9412f, 0.8627f, 0.25f));
    public uint Scholar { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.5255f, 0.3412f, 1f, 0.25f));
    public uint Astrologian { get; set; } = ImGui.ColorConvertFloat4ToU32(new(1f, 0.9059f, 0.2902f, 0.25f));
    public uint Sage { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.5647f, 0.6902f, 1f, 0.25f));

    // PhysicalRangedDps

    public uint Bard { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.5686f, 0.7294f, 0.3686f, 0.25f));
    public uint Machinist { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.4314f, 0.8824f, 0.8392f, 0.25f));
    public uint Dancer { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.8863f, 0.6902f, 0.6863f, 0.25f));

    // MagicalRangedDps

    public uint BlackMage { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.6471f, 0.4745f, 0.8392f, 0.25f));
    public uint Summoner { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.1765f, 0.6078f, 0.4706f, 0.25f));
    public uint RedMage { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0.9098f, 0.4824f, 0.4824f, 0.25f));
    public uint BlueMage { get; set; } = ImGui.ColorConvertFloat4ToU32(new(0f, 0.7255f, 0.9686f, 0.25f));
}
public partial class Overlay : BasicOverlay
{
    private void DrawRoleJobColorsTab()
    {
        using var tab = ImRaii.TabItem("Role and Job Colors");
        if (!tab) return;

        ImGui.Spacing();
        ImGui.Text("Customize Mogu Role and Job colors");
        ImGui.Spacing();

        ImGui.Spacing();
        ImGui.Text("Role Colors");
        ImGui.Spacing();

        _ImGuiColorPick("Tank", Config.Appearance.JobColors.Tank, (newValue) =>
        {
            Config.Appearance.JobColors.Tank = newValue;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Healer", Config.Appearance.JobColors.Healer, (newValue) =>
        {
            Config.Appearance.JobColors.Healer = newValue;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Melee DPS", Config.Appearance.JobColors.MeleeDps, (newValue) =>
        {
            Config.Appearance.JobColors.MeleeDps = newValue;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Physical Ranged DPS", Config.Appearance.JobColors.PhysicalRangedDps, (newValue) =>
        {
            Config.Appearance.JobColors.PhysicalRangedDps = newValue;
            Save(Window.WindowName, Config);
        });

        _ImGuiColorPick("Magical Ranged DPS", Config.Appearance.JobColors.MagicalRangedDps, (newValue) =>
        {
            Config.Appearance.JobColors.MagicalRangedDps = newValue;
            Save(Window.WindowName, Config);
        });

        ImGui.Spacing();
        ImGui.Text("Job Colors");
        ImGui.Spacing();

        if (ImGui.CollapsingHeader("Tanks", ImGuiTreeNodeFlags.None))
        {
            ImGui.Spacing();

            _ImGuiColorPick("Paladin", Config.Appearance.JobColors.Paladin, (newValue) =>
            {
                Config.Appearance.JobColors.Paladin = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Warrior", Config.Appearance.JobColors.Warrior, (newValue) =>
            {
                Config.Appearance.JobColors.Warrior = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Dark Knight", Config.Appearance.JobColors.DarkKnight, (newValue) =>
            {
                Config.Appearance.JobColors.DarkKnight = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Gun Breaker", Config.Appearance.JobColors.GunBreaker, (newValue) =>
            {
                Config.Appearance.JobColors.GunBreaker = newValue;
                Save(Window.WindowName, Config);
            });

            ImGui.Spacing();
            ImGui.Separator();
        }

        if (ImGui.CollapsingHeader("Melee Dps", ImGuiTreeNodeFlags.None))
        {
            ImGui.Spacing();

            _ImGuiColorPick("Monk", Config.Appearance.JobColors.Monk, (newValue) =>
            {
                Config.Appearance.JobColors.Monk = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Dragoon", Config.Appearance.JobColors.Dragoon, (newValue) =>
            {
                Config.Appearance.JobColors.Dragoon = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Ninja", Config.Appearance.JobColors.Ninja, (newValue) =>
            {
                Config.Appearance.JobColors.Ninja = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Samurai", Config.Appearance.JobColors.Samurai, (newValue) =>
            {
                Config.Appearance.JobColors.Samurai = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Reaper", Config.Appearance.JobColors.Reaper, (newValue) =>
            {
                Config.Appearance.JobColors.Reaper = newValue;
                Save(Window.WindowName, Config);
            });

            ImGui.Spacing();
            ImGui.Separator();
        }

        if (ImGui.CollapsingHeader("Healers", ImGuiTreeNodeFlags.None))
        {
            ImGui.Spacing();

            _ImGuiColorPick("White Mage", Config.Appearance.JobColors.WhiteMage, (newValue) =>
            {
                Config.Appearance.JobColors.WhiteMage = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Scholar", Config.Appearance.JobColors.Scholar, (newValue) =>
            {
                Config.Appearance.JobColors.Scholar = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Astrologian", Config.Appearance.JobColors.Astrologian, (newValue) =>
            {
                Config.Appearance.JobColors.Astrologian = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Sage", Config.Appearance.JobColors.Sage, (newValue) =>
            {
                Config.Appearance.JobColors.Sage = newValue;
                Save(Window.WindowName, Config);
            });

            ImGui.Spacing();
            ImGui.Separator();
        }

        if (ImGui.CollapsingHeader("Pysical Ranged Dps", ImGuiTreeNodeFlags.None))
        {
            ImGui.Spacing();

            _ImGuiColorPick("Bard", Config.Appearance.JobColors.Bard, (newValue) =>
            {
                Config.Appearance.JobColors.Bard = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Machinist", Config.Appearance.JobColors.Machinist, (newValue) =>
            {
                Config.Appearance.JobColors.Machinist = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Dancer", Config.Appearance.JobColors.Dancer, (newValue) =>
            {
                Config.Appearance.JobColors.Dancer = newValue;
                Save(Window.WindowName, Config);
            });

            ImGui.Spacing();
            ImGui.Separator();
        }

        if (ImGui.CollapsingHeader("Magical Ranged Dps", ImGuiTreeNodeFlags.None))
        {
            ImGui.Spacing();

            _ImGuiColorPick("Black Mage", Config.Appearance.JobColors.BlackMage, (newValue) =>
            {
                Config.Appearance.JobColors.BlackMage = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Summoner", Config.Appearance.JobColors.Summoner, (newValue) =>
            {
                Config.Appearance.JobColors.Summoner = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Red Mage", Config.Appearance.JobColors.RedMage, (newValue) =>
            {
                Config.Appearance.JobColors.RedMage = newValue;
                Save(Window.WindowName, Config);
            });

            _ImGuiColorPick("Blue Mage", Config.Appearance.JobColors.BlueMage, (newValue) =>
            {
                Config.Appearance.JobColors.BlueMage = newValue;
                Save(Window.WindowName, Config);
            });

            ImGui.Spacing();
            ImGui.Separator();
        }

        ImGui.Spacing();
        ImGui.Text("Other");
        ImGui.Spacing();

        _ImGuiColorPick("Default", Config.Appearance.JobColors.Default, (newValue) =>
        {
            Config.Appearance.JobColors.Default = newValue;
            Save(Window.WindowName, Config);
        });
        
        _ImguiCheckboxWithTooltip("Override", "Replace the opacity value for all colors, jobs or roles.", Config.Appearance.JobColors.Override, (newValue) =>
        {
            Config.Appearance.JobColors.Override = newValue;
            Save(Window.WindowName, Config);
        });

        bool isDisabled = false;
        if(!Config.Appearance.JobColors.Override) _ImGuiBeginDisabled(ref isDisabled);
        
        ImGui.SameLine();
        ImGui.PushItemWidth(150);
        var opacityValue = (int)(Config.Appearance.JobColors.Opacity * 100);
        if (ImGui.SliderInt("Opacity", ref opacityValue, 0, 100))
        {
            Config.Appearance.JobColors.Opacity = (float)opacityValue / 100;
            Save(Window.WindowName, Config);
        }
        ImGui.PopItemWidth();
        
        _ImGuiEndDisabled(ref isDisabled);
    }
}