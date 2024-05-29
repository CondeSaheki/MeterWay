using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

using MeterWay.Overlay;

namespace Lazer;

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
