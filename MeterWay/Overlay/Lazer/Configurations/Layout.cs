using System;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using MeterWay.Overlay;

namespace Lazer;

[Serializable]
public class Layout
{
    public bool DisplayCombatName { get; set; } = true;
    public bool DisplayJobAcronym { get; set; } = true;
    public bool DisplayTotalDamage { get; set; } = true;
    public bool DisplayYourName { get; set; } = false;
}


public partial class Overlay : BasicOverlay
{
    private void DrawAppearanceLayoutTab()
    {
        using var tab = ImRaii.TabItem("Layout");
        if (!tab) return;

        ImGui.Spacing();
        ImGui.Text("Customize the layout of the overlay");
        ImGui.Spacing();

        _ImguiCheckboxWithTooltip("Display Job Acronyms", "Let you toggle the visibility of the Job Acronyms in the overlay.", Config.Appearance.Layout.DisplayJobAcronym, newValue =>
        {
            Config.Appearance.Layout.DisplayJobAcronym = newValue;
            Save(Window.WindowName, Config);
        });

        _ImguiCheckboxWithTooltip("Display Total Damage", "Let you toggle the visibility of the Total Damage in the overlay.", Config.Appearance.Layout.DisplayTotalDamage, newValue =>
        {
            Config.Appearance.Layout.DisplayTotalDamage = newValue;
            Save(Window.WindowName, Config);
        });

        _ImguiCheckboxWithTooltip("Display Combat Names", "Let you toggle the visibility of the current combat name in the header bar.", Config.Appearance.Layout.DisplayCombatName, newValue =>
        {
            Config.Appearance.Layout.DisplayCombatName = newValue;
            Save(Window.WindowName, Config);
        });

        _ImguiCheckboxWithTooltip("Display Your Name", "Let you choose whether you show your own name in the DPS meter or \"YOU\".", Config.Appearance.Layout.DisplayYourName, newValue =>
        {
            Config.Appearance.Layout.DisplayYourName = newValue;
            Save(Window.WindowName, Config);
        });
    }

}
