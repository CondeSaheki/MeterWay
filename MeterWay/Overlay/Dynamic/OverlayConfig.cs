using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Overlay;
using MeterWay.Utils;

namespace Dynamic;

public partial class Overlay : IOverlay, IOverlayConfig
{
    public void DrawScriptTab()
    {
        using var tab = ImRaii.TabItem("Script");
        if (!tab) return;

        bool disabled = false;

        if (ImGui.Button("Choose Script"))
        {
            var fileDialog = new FileDialogManager();
            fileDialog.OpenFileDialog("Choose Script", ".lua",
            (bool isOk, List<string> selectedFiles) =>
            {
                MeterWay.Dalamud.PluginInterface.UiBuilder.Draw -= fileDialog.Draw;
                if (!isOk || selectedFiles.Count == 0) return;
                Config.ScriptFile = selectedFiles[0];
                File.Save($"{Window.Name}{Window.Id}", Config);
            }, 1, null, true);
            MeterWay.Dalamud.PluginInterface.UiBuilder.Draw += fileDialog.Draw;
        }

        ImGui.BeginTable("ScriptDetailsTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg);

        ImGui.TableSetupColumn("Property", ImGuiTableColumnFlags.WidthFixed, 50.0f);
        ImGui.TableSetupColumn("Value");

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text("Status");
        ImGui.TableNextColumn();
        if (StatusScript()) ImGui.TextColored(new Vector4(0f, 1f, 0f, 1f), "Active");
        else ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), "Deactive");

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text("File");
        ImGui.TableNextColumn();
        ImGui.Text(Script != null ? Script.FilePath.FullName : "Empty");
        ImGui.EndTable();

        if (Config.ScriptFile == null) ImGui_Disable(ref disabled);
        if (!StatusScript())
        {
            if (ImGui.Button("Load")) LoadScript();
        }
        else
        {
            if (ImGui.Button("Reload")) ReloadScript();
        }

        ImGui.SameLine();
        if (!StatusScript()) ImGui_Disable(ref disabled);
        if (ImGui.Button("Unload")) UnLoadScript();

        ImGui.SameLine();
        if (Script != null && !Script.HasDrawConfig) ImGui_Disable(ref disabled);
        if (ImGui.Button("Config"))
        {
            if (Script != null)
            {
                Helpers.PopupWindow popup = new($"Script {Script.Name} configurations", 
                () => 
                {
                    if (Script == null) return;
                    Script.ExecuteDrawTab();
                    if (!Script.Status) Window.IsOpen = false;
                });
            }
        }
        ImGui_Enable(ref disabled);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var startupValue = Config.Startup;
        if (ImGui.Checkbox("Load on startup", ref startupValue))
        {
            Config.Startup = startupValue;
            File.Save($"{Window.Name}{Window.Id}", Config);
        }
    }

    public void DrawConfig()
    {
        using var bar = ImRaii.TabBar("Overlay Settings Tabs");
        if (!bar) return;

        DrawScriptTab();
        DrawAboutTab();
    }

    private void DrawAboutTab()
    {
        using var tab = ImRaii.TabItem("About");
        if (!tab) return;

        DrawAbout();
    }

    private void DrawAbout()
    {
        ImGui.Text($"Description");
        ImGui.TextWrapped($"{Description}");
        ImGui.Text($"Autor");
        ImGui.TextWrapped($"{Autor}");
    }
}