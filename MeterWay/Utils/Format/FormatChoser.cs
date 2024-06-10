using System;
using ImGuiNET;

namespace MeterWay.Utils;

/// <summary>
/// A class for choosing a format from a tree-like button.
/// </summary>
public class FormatChoser
{
    private Action<Node> Setter { get; set; }
    private string[] Current { get; set; }
    private string Label { get; set; }

    /// <summary>
    /// Create a new FormatChoser.
    /// </summary>
    /// <param name="label">The label of the button to open the menu.</param>
    /// <param name="current">The current format.</param>
    /// <param name="setter">The action to set the selected format.</param>
    public FormatChoser(string label, string[] current, Action<Node> setter)
    {
        Label = label;
        Current = current;
        Setter = setter;
    }

    /// <summary>
    /// Create a new FormatChoser with the default label.
    /// </summary>
    /// <param name="current">The current format.</param>
    /// <param name="setter">The action to set the selected format.</param>
    public FormatChoser(string[] current, Action<Node> setter) : this("Change format", current, setter) { }

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
    
    private void DrawMenu(Node node)
    {
        foreach (var child in node.Children)
        {
            if (child.Children.Length > 0)
            {
                if (ImGui.BeginMenu(child.Name))
                {
                    DrawMenu(child);
                    ImGui.EndMenu();
                }
            }
            else if (ImGui.MenuItem(child.Name) && Current != child.FullName) Setter(child);
        }
    }
}
