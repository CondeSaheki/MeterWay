using System;
using ImGuiNET;

namespace MeterWay.Utils;

/// <summary>
/// A class for choosing a PlaceHolder with tree-like button.
/// </summary>
public class PlaceHolderChoser
{
    private Action<PlaceHolder> Setter { get; set; }
    private string[] Current { get; set; }
    private string Label { get; set; }

    /// <summary>
    /// Create a new PlaceHolderChoser.
    /// </summary>
    /// <param name="label">The label of the button to open the menu.</param>
    /// <param name="current">The current format.</param>
    /// <param name="setter">The action to set the selected format.</param>
    public PlaceHolderChoser(string label, string[] current, Action<PlaceHolder> setter)
    {
        Label = label;
        Current = current;
        Setter = setter;
    }

    /// <summary>
    /// Create a new PlaceHolderChoser with the default label.
    /// </summary>
    /// <param name="current">The current format.</param>
    /// <param name="setter">The action to set the selected format.</param>
    public PlaceHolderChoser(string[] current, Action<PlaceHolder> setter) : this("Change format", current, setter) { }

    /// <summary>
    /// Draw the menu.
    /// </summary>
    public void Draw()
    {
        if (ImGui.Button(Label)) ImGui.OpenPopup($"###{Label}Popup");
        if (ImGui.BeginPopup($"###{Label}Popup"))
        {
            DrawMenu(PlaceHolder.All);
            ImGui.EndPopup();
        }
    }
    
    private void DrawMenu(PlaceHolder node)
    {
        foreach (var child in node.Children)
        {
            if (child.Children.Length > 0)
            {
                if (ImGui.BeginMenu(child.FirstName))
                {
                    DrawMenu(child);
                    ImGui.EndMenu();
                }
            }
            else if (ImGui.MenuItem(child.FirstName) && Current != child.FullName) Setter(child);
        }
    }
}
