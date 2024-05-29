using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Overlay;

namespace Mogu;

public partial class Overlay : IOverlay, IOverlayConfig
{
    private void DrawAboutTab()
    {
        using var tab = ImRaii.TabItem("About");
        if (!tab) return;

        ImGui.Text($"Description");
        ImGui.TextWrapped($"{Description}");
        ImGui.Text($"Autor");
        ImGui.TextWrapped($"{Autor}");
    }
}
