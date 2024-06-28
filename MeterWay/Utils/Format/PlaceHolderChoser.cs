using System;
using System.Numerics;
using ImGuiNET;

namespace MeterWay.Utils.Format;

/// <summary>
/// A class for choosing a PlaceHolder with tree-like button.
/// </summary>
public static partial class FormatHelpers
{
    /// <summary>
    /// Draws a tree-like button for choosing a PlaceHolder.
    /// </summary>
    /// <param name="current">The current selected placeholder names.</param>
    /// <param name="setter">The action to call when a placeholder is selected.</param>
    /// <param name="label">The label for the button.</param>
    /// <param name="triangle">Whether to show the triangle indicator.</param>
    /// <param name="showCurrent">Whether to show the current selection.</param>
    public static void PlaceHolderChoser(string[] current, Action<PlaceHolder> setter, string label = "Change format", bool triangle = true, bool showCurrent = true)
    {
        void DrawMenu(PlaceHolder node)
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
                else if (ImGui.MenuItem(child.FirstName) && current != child.FullName) setter(child);
            }
        }

        Vector2 buttonPos = ImGui.GetCursorScreenPos();

        if (ImGui.Button($"{label}{(triangle ? "  \u25BC" : string.Empty)}"))
        {
            ImGui.OpenPopup($"###{label}Popup");
            ImGui.SetNextWindowPos(buttonPos + new Vector2(0, 24)); // TODO: 24 works on default dalamud scale and font size. Make this generic for all sizes
        }

        if (ImGui.BeginPopup($"###{label}Popup"))
        {
            if (showCurrent && current.Length != 0)
            {
                ImGui.Text("Current:");
                ImGui.SameLine();
                ImGui.TextColored(new(0.0f, 0.75f, 1.0f, 1.0f), string.Join(" > ", current));

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }

            DrawMenu(PlaceHolder.All);
            ImGui.EndPopup();
        }
    }
}
