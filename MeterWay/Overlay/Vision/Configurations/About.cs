using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Overlay;

namespace Vision;

public partial class Overlay : BasicOverlay
{
    private void DrawAboutTab()
    {
        using var tab = ImRaii.TabItem("About");
        if (!tab) return;

        ImGui.Text($"Description");
        ImGui.TextWrapped($"{Info.Description}");
        ImGui.Text($"Autor");
        ImGui.TextWrapped($"{Info.Author}");
    }
}